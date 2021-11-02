using System;

namespace Core.Framework.Event
{
    /// <summary>
    /// Author: chengdundeng
    /// Date: 2019/11/27
    /// Desc: 事件介面
    /// </summary>
    public interface IEvent
    {
        /// <summary>
        /// 針對特定tag發送事件
        /// </summary>
        EventTagData EventTag { get; }

        /// <summary>
        /// 事件的實體型別
        /// </summary>
        Type EventType { get; }

        /// <summary>
        /// 事件的實體型別
        /// </summary>
        Type GetType();
    }
}