using Core.Framework.Memory;
using System;
using System.Collections.Generic;

namespace Core.Framework.Event
{
    public partial class EventSystem
    {
        /// <summary>
        /// Author: chengdundeng
        /// Date: 2019/11/27
        /// Desc: 實作IEventListener
        /// </summary>
        private class EventListener<TListen> : RefCountedObject<EventListener<TListen>>,
            IEventListener<TListen>
            where TListen : class, IEvent
        {
            private List<Func<TListen, bool>> mConditions = new List<Func<TListen, bool>>();
            private Action<TListen> mListener;
            private EventSystem mEventSystem;

            public bool IsOnce { get; set; }

            public object AttachObject { get; private set; }
            public EventTagData EventTag { get; private set; }

            public string CacheName { get; set; }

            private Type EventType { get; set; }

            private bool IsRegistered { get; set; }

            static EventListener()
            {
                SetReserveSize(50);
            }

            public static EventListener<TListen> Allocate(EventSystem eventSystem, object obj, EventTagData tag, Action<TListen> l)
            {
                EventListener<TListen> listener = Allocate();
                listener.mEventSystem = eventSystem;
                listener.mListener = l;
                listener.IsOnce = false;
                listener.AttachObject = obj;
                listener.EventTag = tag;
                listener.mConditions.Clear();
                listener.IsRegistered = true;
                listener.CacheName = obj != null ? obj.ToString() : null;
                listener.EventType = typeof(TListen);

                return listener;
            }

            protected override void OnRelease()
            {
                base.OnRelease();

                if (IsRegistered)
                {
                    throw new InvalidOperationException("should not call Release() before call Unregister() !!!");
                }
            }

            public void AddCondition(Func<TListen, bool> conditionFunc)
            {
                mConditions.Add(conditionFunc);
            }

            public void Invoke(IEvent evt)
            {
                TListen realEvent = evt as TListen;
                if (realEvent == null)
                {
                    return;
                }

                for (int i = 0; i < mConditions.Count; i++)
                {
                    Func<TListen, bool> condition = mConditions[i];
                    if (!condition.Invoke(realEvent))
                    {
                        return;
                    }
                }

                mListener.Invoke(realEvent);

                if (IsOnce)
                    Unregister();
            }

            public void Unregister()
            {
                if (IsRegistered)
                {
                    IsRegistered = false;
                    mEventSystem.Unregister(this);
                }
            }

            public Type GetEventType()
            {
                return EventType;
            }
        }
    }
}