using System;

namespace Core.Framework.Event.Property
{
    /// <summary>
    /// Author: chengdundeng
    /// Date: 2019/11/27
    /// Desc: 可訂閱Field介面
    /// </summary>
    public interface ISubscribableField
    {
    }

    /// <summary>
    /// Author: chengdundeng
    /// Date: 2019/11/27
    /// Desc: 可訂閱Field介面
    /// </summary>
    public interface ISubscribableField<T> : ISubscribableField
    {
        /// <summary>
        /// 讀取屬性
        /// </summary>
        T Value { get; }

        /// <summary>
        /// 取得上一次屬性值
        /// </summary>
        T LastValue { get; }

        /// <summary>
        /// 監聽屬性變化, 第一個是LastValue, 第二個指當前Value, 監聽者必須自行轉型
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        IEventListener<SubscribableFieldEvent<T>> AddValueChangeListener(Action<T, T> callback);
    }
}