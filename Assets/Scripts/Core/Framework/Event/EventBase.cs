using Core.Framework.Memory;
using System;

namespace Core.Framework.Event
{
    /// <summary>
    /// Author: chengdundeng
    /// Date: 2019/11/27
    /// Desc: 基礎事件類型，使用者必須繼承此物件來客製化事件類型
    /// </summary>
    public abstract class EventBase<T> : RefCountedObject<T>, IEvent
        where T : EventBase<T>, new()
    {
        private static Type sType = typeof(T);

        /// <summary>
        ///     針對特定tag發送事件
        /// </summary>
        public EventTagData EventTag { get; set; }

        /// <summary>
        ///     事件的實體型別
        /// </summary>
        public Type EventType
        {
            get { return sType; }
        }

        /// <summary>
        ///     事件的實體型別
        /// </summary>
        public new Type GetType()
        {
            return sType;
        }

        public T SetEventTag(EventTagData tag)
        {
            EventTag = tag;
            return this as T;
        }
    }
}