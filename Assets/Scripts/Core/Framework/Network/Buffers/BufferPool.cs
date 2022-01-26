using LogUtil;
using System;
using System.Collections.Generic;

namespace Core.Framework.Network.Buffers
{
    public class BufferBasePool
    {
        public const int MaxBufferAllocCount = 65536;
        public static readonly BufferBasePool Default = new BufferBasePool(256);

        private Queue<BufferBase> _pool = new Queue<BufferBase>();

        public int BufferSize { get; }

        public BufferBasePool(int size)
        {
            BufferSize = size;
        }

        public void PreAlloc(int count)
        {
            count = Math.Min(count, MaxBufferAllocCount - _allocCount);
            for (int i = 0; i < count; i++)
            {
                _pool.Enqueue(new BufferBase(BufferSize));
            }
        }

        private int _allocCount = 0;
        public BufferBase Alloc()
        {
            lock (_pool)
            {
                if (_pool.Count > 0)
                    return _pool.Dequeue();
                if(_allocCount >= MaxBufferAllocCount)
                {
                    return null;
                }
                _allocCount++;
                //Debug.Log($"BufferBase AllocCount={_allocCount}");
                return new BufferBase(BufferSize);
            }
        }

        //private int _deallocCount = 0;
        public void Dealloc(BufferBase buffer)
        {
            buffer.Reset();
            lock (_pool)
            {
                //Debug.Log($"BufferBasePool DeallocCount={++_deallocCount}");
                _pool.Enqueue(buffer);
            }
        }
    }

    public class BufferStreamPool
    {
        public static readonly BufferStreamPool Default = new BufferStreamPool(BufferBasePool.Default);

        private Queue<BufferStream> _pool = new Queue<BufferStream>();

        public BufferBasePool BufferBasePool { get; }

        public BufferStreamPool(BufferBasePool bufferPool)
        {
            BufferBasePool = bufferPool;
        }

        //private int _allocCount = 0;
        public BufferStream Alloc()
        {
            lock (_pool)
            {
                if (_pool.Count > 0)
                    return _pool.Dequeue();
                //Debug.Log($"BufferStreamPool AllocCount={++_allocCount}");
                return new BufferStream(BufferBasePool);
            }
        }

        //private int _deallocCount = 0;
        public void Dealloc(BufferStream stream)
        {
            stream.Clear();
            lock (_pool)
            {
                //Debug.Log($"BufferStreamPool DeallocCount={++_deallocCount}");
                _pool.Enqueue(stream);
            }
        }
    }

    public class BufferPoolProvider
    {
        public static readonly BufferPoolProvider Default = new BufferPoolProvider(BufferBasePool.Default);

        public BufferBasePool BufferBasePool { get; private set; }
        public BufferStreamPool BufferStreamPool { get; private set; }

        public BufferPoolProvider(BufferBasePool bufferPool)
        {
            BufferBasePool = bufferPool;
            BufferStreamPool = new BufferStreamPool(BufferBasePool);
        }
    }
}
