using Core.Framework.Memory;
using System;

namespace Core.Framework.Event
{
    /// <summary>
    /// Author: chengdundeng
    /// Date: 2019/11/27
    /// Desc: 事件監聽器介面
    /// </summary>
    public interface IEventListener : IRefCountedObject
    {
        /// <summary>
        ///     是否只監聽一次事件
        /// </summary>
        bool IsOnce { get; set; }

        /// <summary>
        ///     觸發事件
        /// </summary>
        /// <param name="evt">事件物件</param>
        void Invoke(IEvent evt);

        /// <summary>
        ///     取消監聽
        /// </summary>
        void Unregister();

        /// <summary>
        ///     獲取事件型別
        /// </summary>
        /// <returns></returns>
        Type GetEventType();

        /// <summary>
        /// 綁定的物件對象
        /// </summary>
        object AttachObject { get; }

        /// <summary>
        /// 監聽器的識別項
        /// </summary>
        EventTagData EventTag { get; }

        /// <summary>
        /// 物件名稱 (識別用)
        /// </summary>
        string CacheName { get; }
    }

    /// <summary>
    /// Author: chengdundeng
    /// Date: 2019/11/27
    /// Desc: 事件監聽器介面
    /// </summary>
    public interface IEventListener<TEventType> : IEventListener
        where TEventType : class, IEvent
    {
        /// <summary>
        ///     新增事件觸發條件
        /// </summary>
        /// <typeparam name="TEventType">事件型別條件</typeparam>
        /// <param name="conditionFunc">條件判斷式</param>
        void AddCondition(Func<TEventType, bool> conditionFunc);
    }
}