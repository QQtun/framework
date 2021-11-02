using Core.Framework.Memory;
using System;
using System.Collections.Generic;

namespace Core.Framework.Event
{
    /// <summary>
    /// Author: chengdundeng
    /// Date: 2019/11/27
    /// Desc: 基礎註冊、配發的管理器
    /// partial 是因為 EventListener 比較複雜不想寫在 EventSystem 內, 實作拉到EventListener.cs完成
    /// </summary>
    public partial class EventSystem : Singleton<EventSystem>
    {
        private Queue<IEvent> mEventQueue = new Queue<IEvent>();

        private Dictionary<Type, HashSet<IEventListener>> mListenersMap =
            new Dictionary<Type, HashSet<IEventListener>>();

        private Dictionary<object, HashSet<IEventListener>> mObjectToListeners =
            new Dictionary<object, HashSet<IEventListener>>();

        private Dictionary<Type, Dictionary<int, HashSet<IEventListener>>> mTypeToTagListeners =
            new Dictionary<Type, Dictionary<int, HashSet<IEventListener>>>();

        private IEventListener[] mBuffer;

        private void Update()
        {
            ExecuteProcess();
        }

        /// <summary>
        ///     實際執行事件發送的函式
        /// </summary>
        private void ExecuteProcess()
        {
            while (mEventQueue.Count > 0)
            {
                IEvent evt = mEventQueue.Dequeue();

                InvokeEvent(evt);
            }
        }

        /// <summary>
        ///     發送事件訊息
        /// </summary>
        /// <param name="evt"></param>
        /// <param name="forceNow">是否立刻發送事件</param>
        public void SendEvent(IEvent evt, bool forceNow = false)
        {
            if (forceNow)
            {
                InvokeEvent(evt);
                return;
            }

            mEventQueue.Enqueue(evt);
        }

        /// <summary>
        ///     發送字串為KEY的事件訊息
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        /// <param name="forceNow">是否立刻發送事件</param>
        public void SendStringKeyEvent(string key, object data = null, bool forceNow = false)
        {
            var evt = StringKeyEvent.Allocate(key, data);
            SendEvent(evt, forceNow);
        }

        /// <summary>
        ///     註冊監聽事件訊息
        /// </summary>
        /// <typeparam name="TListen">欲監聽的事件類別</typeparam>
        /// <param name="listener">已經建立的監聽器物件, 反註冊, 或新增條件時使用</param>
        public void RegisterListener<TListen>(IEventListener<TListen> listener) where TListen : class, IEvent
        {
            HashSet<IEventListener> listeners;
            Type eventType = typeof(TListen);

            mListenersMap.TryGetValue(eventType, out listeners);
            if (listeners == null)
            {
                listeners = new HashSet<IEventListener>();
                mListenersMap.Add(eventType, listeners);
            }

            if (!listeners.Contains(listener))
            {
                listeners.Add(listener);
            }

            if (listener.EventTag.type != EventTagData.Type.None)
            {
                AddToTagDictionary<TListen>(listener, listener.EventTag);
            }
            if (listener.AttachObject != null)
            {
                AddToObjectDictionary(listener, listener.AttachObject);
            }
        }

        /// <summary>
        ///     註冊監聽事件訊息
        /// </summary>
        /// <typeparam name="TListen">欲監聽的事件類別</typeparam>
        /// <param name="callback">事件發生的callback</param>
        /// <returns>監聽器物件, 反註冊, 或新增條件時使用</returns>
        public IEventListener<TListen> RegisterListener<TListen>(Action<TListen> callback)
            where TListen : class, IEvent
        {
            return RegisterListener(callback, null, EventTagData.None);
        }

        /// <summary>
        ///     註冊監聽事件訊息
        /// </summary>
        /// <typeparam name="TListen">欲監聽的事件類別</typeparam>
        /// <param name="callback">事件發生的callback</param>
        /// <param name="tag">給監聽器的標記, 可以方便發送者指定特定標記發送事件</param>
        /// <returns></returns>
        public IEventListener<TListen> RegisterListenerWithTag<TListen>(Action<TListen> callback, EventTagData tag)
            where TListen : class, IEvent
        {
            return RegisterListener(callback, null, tag);
        }

        /// <summary>
        ///     註冊監聽事件訊息
        /// </summary>
        /// <typeparam name="TListen">欲監聽的事件類別</typeparam>
        /// <param name="callback">事件發生的callback</param>
        /// <param name="obj">監聽器綁定於特定物件, 方便針對特定物件反註冊所有綁定於此物件的監聽器</param>
        /// <returns></returns>
        public IEventListener<TListen> RegisterListenerWithObject<TListen>(Action<TListen> callback, object obj)
            where TListen : class, IEvent
        {
            return RegisterListener(callback, obj, EventTagData.None);
        }

        /// <summary>
        ///     註冊監聽事件訊息
        /// </summary>
        /// <typeparam name="TListen">欲監聽的事件類別</typeparam>
        /// <param name="callback">事件發生的callback</param>
        /// <param name="obj">監聽器綁定於特定物件, 方便針對特定物件反註冊所有綁定於此物件的監聽器</param>
        /// <param name="tag">給監聽器的標記, 可以方便發送者指定特定標記發送事件</param>
        /// <returns></returns>
        public IEventListener<TListen> RegisterListenerWithTagAndObject<TListen>(Action<TListen> callback,
            object obj, EventTagData tag)
            where TListen : class, IEvent
        {
            return RegisterListener(callback, obj, tag);
        }

        /// <summary>
        ///     註冊監聽以字串為KEY的事件訊息
        /// </summary>
        /// <param name="key"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public IEventListener<StringKeyEvent> RegisterStringKeyListener(string key, Action<StringKeyEvent> callback)
        {
            return RegisterListenerWithTag(callback, key);
        }

        /// <summary>
        ///     註冊監聽以字串為KEY的事件訊息
        /// </summary>
        /// <param name="key"></param>
        /// <param name="callback"></param>
        /// <param name="obj">監聽器綁定於特定物件, 方便針對特定物件反註冊所有綁定於此物件的監聽器</param>
        /// <returns></returns>
        public IEventListener<StringKeyEvent> RegisterStringKeyListenerWithObject(string key, Action<StringKeyEvent> callback, object obj)
        {
            return RegisterListenerWithTagAndObject(callback, obj, key);
        }

        /// <summary>
        ///     針對特定物件反註冊所有監聽器
        /// </summary>
        /// <param name="obj">監聽器綁定的特定物件, 針對特定物件反註冊所有綁定於此物件的監聽器</param>
        public void UnregisterAll(object obj)
        {
            if (obj == null)
            {
                return;
            }

            HashSet<IEventListener> listeners;
            mObjectToListeners.TryGetValue(obj, out listeners);
            mObjectToListeners.Remove(obj);

            if (listeners == null)
            {
                return;
            }

            foreach (IEventListener listener in listeners)
            {
                listener.Unregister();
            }
        }

        /// <summary>
        ///     取消監聽 ( 這是給EventListener呼叫的函式 )
        /// </summary>
        /// <param name="listener"></param>
        private void Unregister(IEventListener listener)
        {
            HashSet<IEventListener> listeners = null;
            var type = listener.GetEventType();
            if (type != null)
            {
                mListenersMap.TryGetValue(type, out listeners);
            }
            if (listeners != null)
                listeners.Remove(listener);
            object obj = listener.AttachObject;
            if (obj != null)
            {
                mObjectToListeners.TryGetValue(obj, out listeners);
                if (listeners != null)
                    listeners.Remove(listener);
            }

            EventTagData tag = listener.EventTag;
            if (tag.type != EventTagData.Type.None)
            {
                Type eventType = listener.GetEventType();
                Dictionary<int, HashSet<IEventListener>> tagToListeners;
                mTypeToTagListeners.TryGetValue(eventType, out tagToListeners);
                if (tagToListeners != null)
                    tagToListeners.TryGetValue(tag.hashCode, out listeners);
                if (listeners != null)
                    listeners.Remove(listener);
            }

            listener.Release();
        }

        private IEventListener<TListen> RegisterListener<TListen>(Action<TListen> callback, object obj,
            EventTagData tag)
            where TListen : class, IEvent
        {
            if (callback == null)
            {
                return null;
            }

            IEventListener<TListen> listener = AddToTypeDictionary(callback, obj, tag);

            if (tag.type != EventTagData.Type.None)
            {
                AddToTagDictionary<TListen>(listener, tag);
            }
            if (obj != null)
            {
                AddToObjectDictionary(listener, obj);
            }

            return listener;
        }

        private IEventListener<TListen> AddToTypeDictionary<TListen>(Action<TListen> callback, object obj,
            EventTagData tag) where TListen : class, IEvent
        {
            HashSet<IEventListener> listeners;
            Type eventType = typeof(TListen);

            mListenersMap.TryGetValue(eventType, out listeners);
            if (listeners == null)
            {
                listeners = new HashSet<IEventListener>();
                mListenersMap.Add(eventType, listeners);
            }

            EventListener<TListen> listener = EventListener<TListen>.Allocate(this, obj, tag, callback);
            listeners.Add(listener);

            return listener;
        }

        private void AddToTagDictionary<TListen>(IEventListener listener, EventTagData tag)
            where TListen : class, IEvent
        {
            Type listenType = typeof(TListen);

            Dictionary<int, HashSet<IEventListener>> tagToListeners;
            mTypeToTagListeners.TryGetValue(listenType, out tagToListeners);
            if (tagToListeners == null)
            {
                tagToListeners = new Dictionary<int, HashSet<IEventListener>>();
                mTypeToTagListeners.Add(listenType, tagToListeners);
            }

            HashSet<IEventListener> listeners;
            tagToListeners.TryGetValue(tag.hashCode, out listeners);
            if (listeners == null)
            {
                listeners = new HashSet<IEventListener>();
                tagToListeners.Add(tag.hashCode, listeners);
            }

            if (!listeners.Contains(listener))
            {
                listeners.Add(listener);
            }
        }

        private void AddToObjectDictionary(IEventListener listener, object obj)
        {
            HashSet<IEventListener> listeners;
            mObjectToListeners.TryGetValue(obj, out listeners);
            if (listeners == null)
            {
                listeners = new HashSet<IEventListener>();
                mObjectToListeners.Add(obj, listeners);
            }
            if (!listeners.Contains(listener))
            {
                listeners.Add(listener);
            }
        }

        private void InvokeEvent(IEvent evt)
        {
            do
            {
                HashSet<IEventListener> listeners;
                if (!mListenersMap.TryGetValue(evt.GetType(), out listeners))
                {
                    break;
                }

                EventTagData tag = evt.EventTag;

                if (tag.type == EventTagData.Type.None)
                {
                    InvokeAll(evt, listeners);
                }
                else
                {
                    // 針對特定tag發送
                    Type listenType = evt.EventType;

                    Dictionary<int, HashSet<IEventListener>> tagToListeners;
                    if (!mTypeToTagListeners.TryGetValue(listenType, out tagToListeners))
                    {
                        break;
                    }

                    if (!tagToListeners.TryGetValue(tag.hashCode, out listeners))
                    {
                        break;
                    }

                    InvokeAll(evt, listeners);
                }
            } while (false);

            IRefCountedObject refObj = evt as IRefCountedObject;
            if (refObj != null)
            {
                refObj.Release();
            }
        }

        private void InvokeAll(IEvent evt, HashSet<IEventListener> listeners)
        {
            if (mBuffer == null || mBuffer.Length < listeners.Count)
            {
                mBuffer = new IEventListener[listeners.Count + 100]; // 一次+100 減少產生垃圾
            }
            listeners.CopyTo(mBuffer);
            int size = listeners.Count;
            for (int i = 0; i < size; i++)
            {
                try
                {
                    mBuffer[i].Invoke(evt);
                }
                catch (Exception e)
                {
                    LogUtil.Debug.LogErrorFormat("Invoke Event happened exception e: {0}", e);
                }
            }
        }
    }
}