// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

// define TRACE_LEAKS to get additional diagnostics that can lead to the leak sources. note: it will
// make everything about 2-3x slower
//
//#define DETECT_LEAKS
//#define TRACE_LEAKS

// define DETECT_LEAKS to detect possible leaks
//#if DEBUG
//#define DETECT_LEAKS  //for now always enable DETECT_LEAKS in debug.
//#endif

using System;
using System.Diagnostics;
using System.Threading;

#if DETECT_LEAKS
using System.Runtime.CompilerServices;

#endif

namespace Core.Framework.Memory
{
    using Debug = System.Diagnostics.Debug;

    /// <summary>
    /// Generic implementation of object pooling pattern with predefined pool size limit. The main
    /// purpose is that limited number of frequently used objects can be kept in the pool for
    /// further recycling.
    ///
    /// Notes:
    /// 1) it is not the goal to keep all returned objects. Pool is not meant for storage. If there
    ///    is no space in the pool, extra returned objects will be dropped.
    ///
    /// 2) it is implied that if object was obtained from a pool, the caller will return it back in
    ///    a relatively short time. Keeping checked out objects for long durations is ok, but
    ///    reduces usefulness of pooling. Just new up your own.
    ///
    /// Not returning objects to the pool in not detrimental to the pool's work, but is a bad practice.
    /// Rationale:
    ///    If there is no intent for reusing the object, do not use pool - just use "new".
    /// </summary>
    public class ObjectPool<T> : Object where T : class
    {
        [DebuggerDisplay("{value,nq}")]
        private struct Element
        {
            internal T value;
        }

        /// <remarks>
        /// Not using System.Func{T} because this file is linked into the (debugger) Formatter,
        /// which does not have that type (since it compiles against .NET 2.0).
        /// </remarks>
        public delegate T Factory();

        // Storage for the pool objects. The first item is stored in a dedicated field because we
        // expect to be able to satisfy most requests from it.
        private T _firstItem;

        private Element[] _items;

        // factoryFunc is stored for the lifetime of the pool. We will call this only when pool needs to
        // expand. compared to "new T()", Func gives more flexibility to implementers and faster
        // than "new T()".
        private readonly Factory _factoryFunc;

        private int _size;

        public int UsableCount { get; private set; }
        public int CreatedObjectCount { get; private set; }

        public int Size
        {
            get { return _size; }
            set
            {
                if (value != _size)
                {
                    int itemSize = value - 1;
                    Element[] newItem = new Element[itemSize];
                    int pickCount = 0;
                    for (int i = 0; i < _items.Length; i++)
                    {
                        Element e = _items[i];
                        if (e.value != null)
                        {
                            newItem[pickCount] = e;
                            ++pickCount;
                        }

                        if (pickCount >= itemSize)
                        {
                            break;
                        }
                    }

                    if (_firstItem != null)
                    {
                        ++pickCount;
                    }

                    UsableCount = pickCount;
                    _items = newItem;
                }
                _size = value;
            }
        }

        public int AllocateCount { get; private set; }
        public int FreeCount { get; private set; }

#if DETECT_LEAKS
        private static readonly Dictionary<T, LeakTracker> leakTrackers = new Dictionary<T, LeakTracker>();

        private class LeakTracker : IDisposable
        {
            private volatile bool disposed;

#if TRACE_LEAKS
            internal volatile object Trace = null;
#endif

            public void Dispose()
            {
                disposed = true;
                GC.SuppressFinalize(this);
            }

            private string GetTrace()
            {
#if TRACE_LEAKS
                return Trace == null ? "" : Trace.ToString();
#else
                return "Leak tracing information is disabled. Define TRACE_LEAKS on ObjectPool`1.cs to get more info \n";
#endif
            }

            ~LeakTracker()
            {
                if (!this.disposed && !Environment.HasShutdownStarted)
                {
                    var trace = GetTrace();

                    // If you are seeing this message it means that object has been allocated from the pool
                    // and has not been returned back. This is not critical, but turns pool into rather
                    // inefficient kind of "new".
                    IGG.Debug.LogFormat("TRACEOBJECTPOOLLEAKS_BEGIN\nPool detected potential leaking of {0}. \n Location of the leak: \n {1} TRACEOBJECTPOOLLEAKS_END",
                        typeof(T), GetTrace());
                }
            }
        }
#endif

        public ObjectPool(Factory factoryFunc)
            : this(factoryFunc, Environment.ProcessorCount * 2)
        {
        }

        public ObjectPool(Factory factoryFunc, int size)
        {
            Debug.Assert(size >= 1);
            _factoryFunc = factoryFunc;
            _items = new Element[size - 1];
            Size = size;
        }

        private T CreateInstance()
        {
            var inst = _factoryFunc();
            if (inst != null)
            {
                ++CreatedObjectCount;
                ++UsableCount;
            }
            return inst;
        }

        /// <summary>
        /// Produces an instance.
        /// </summary>
        /// <remarks>
        /// Search strategy is a simple linear probing which is chosen for it cache-friendliness.
        /// Note that Free will try to store recycled objects close to the start thus statistically
        /// reducing how far we will typically search.
        /// </remarks>
        public T Allocate()
        {
            // PERF: Examine the first element. If that fails, AllocateSlow will look at the remaining elements.
            // Note that the initial read is optimistically not synchronized. That is intentional.
            // We will interlock only when we have a candidate. in a worst case we may miss some
            // recently returned objects. Not a big deal.
            lock (this)
            {
                T inst = _firstItem;
                if (inst == null || inst != Interlocked.CompareExchange(ref _firstItem, null, inst))
                {
                    inst = AllocateSlow();
                }

                --UsableCount;
                ++AllocateCount;

#if DETECT_LEAKS
            var tracker = new LeakTracker();
            leakTrackers.Add(inst, tracker);

#if TRACE_LEAKS
            var frame = CaptureStackTrace();
            tracker.Trace = frame;
#endif
#endif
                return inst;
            }
        }

        private T AllocateSlow()
        {
            var items = _items;

            for (int i = 0; i < items.Length; i++)
            {
                // Note that the initial read is optimistically not synchronized. That is intentional.
                // We will interlock only when we have a candidate. in a worst case we may miss some
                // recently returned objects. Not a big deal.
                T inst = items[i].value;
                if (inst != null)
                {
                    if (inst == Interlocked.CompareExchange(ref items[i].value, null, inst))
                    {
                        return inst;
                    }
                }
            }

            return CreateInstance();
        }

        /// <summary>
        /// Returns objects to the pool.
        /// </summary>
        /// <remarks>
        /// Search strategy is a simple linear probing which is chosen for it cache-friendliness.
        /// Note that Free will try to store recycled objects close to the start thus statistically
        /// reducing how far we will typically search in Allocate.
        /// </remarks>
        public void Free(T obj)
        {
            lock (this)
            {
                Validate(obj);
                ForgetTrackedObject(obj);

                ++FreeCount;
                if (_firstItem == null)
                {
                    // Intentionally not using interlocked here.
                    // In a worst case scenario two objects may be stored into same slot.
                    // It is very unlikely to happen and will only mean that one of the objects will get collected.
                    _firstItem = obj;
                    ++UsableCount;
                }
                else
                {
                    if (FreeSlow(obj))
                    {
                        ++UsableCount;
                    }
                }
            }
        }

        private bool FreeSlow(T obj)
        {
            var items = _items;
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].value == null)
                {
                    // Intentionally not using interlocked here.
                    // In a worst case scenario two objects may be stored into same slot.
                    // It is very unlikely to happen and will only mean that one of the objects will get collected.
                    items[i].value = obj;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Removes an object from leak tracking.
        ///
        /// This is called when an object is returned to the pool.  It may also be explicitly
        /// called if an object allocated from the pool is intentionally not being returned
        /// to the pool.  This can be of use with pooled arrays if the consumer wants to
        /// return a larger array to the pool than was originally allocated.
        /// </summary>
        [Conditional("DEBUG")]
        private void ForgetTrackedObject(T old, T replacement = null)
        {
#if DETECT_LEAKS
            LeakTracker tracker;
            if (leakTrackers.TryGetValue(old, out tracker))
            {
                tracker.Dispose();
                leakTrackers.Remove(old);
            }
            else
            {
                var trace = CaptureStackTrace();
                IGG.Debug.LogFormat("TRACEOBJECTPOOLLEAKS_BEGIN\nObject of type {0} was freed, but was not from pool. \n Callstack: \n {1} TRACEOBJECTPOOLLEAKS_END",
                    typeof(T), trace);
            }

            if (replacement != null)
            {
                tracker = new LeakTracker();
                leakTrackers.Add(replacement, tracker);
            }
#endif
        }

#if DETECT_LEAKS
        private static object CaptureStackTrace()
        {
            return new StackTrace(2, true);
        }
#endif

        [Conditional("DEBUG")]
        private void Validate(object obj)
        {
#if (UNITY_5_4_OR_NEWER)
            Debug.Assert(obj != null, "freeing null?");
#endif
            var items = _items;
            for (int i = 0; i < items.Length; i++)
            {
                var value = items[i].value;
                if (value == null)
                {
                    return;
                }
#if (UNITY_5_4_OR_NEWER)
                Debug.Assert(value != obj, "freeing twice?");
#endif
            }
        }
    }
}