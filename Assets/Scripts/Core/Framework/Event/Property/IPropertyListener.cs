namespace Core.Framework.Event.Property
{
    /// <summary>
    /// Author: chengdundeng
    /// Date: 2019/11/27
    /// Desc: 屬性監聽介面
    /// </summary>
    public interface IPropertyListener
    {
        /// <summary>
        /// 是否處於訂閱狀態
        /// </summary>
        bool IsUnsubscribed { get; }

        /// <summary>
        /// 訂閱的屬性名稱
        /// </summary>
        string PropertyName { get; }

        /// <summary>
        /// 取消訂閱
        /// </summary>
        void Unsubscribe();

        /// <summary>
        /// 給PropertyManager使用的函式
        /// 用以在屬性提供者出現時跟listener告知
        /// </summary>
        /// <param name="field"></param>
        void Subscribe(ISubscribableField field);
    }

    public interface IPropertyListener<T> : IPropertyListener
    {
        /// <summary>
        /// 取得當前屬性值
        /// </summary>
        T LastValue { get; }

        /// <summary>
        /// 取得前一次屬性值
        /// </summary>
        T Value { get; }

        /// <summary>
        /// 給PropertyManager使用的函式
        /// 用以在屬性提供者出現時跟listener告知
        /// </summary>
        /// <param name="field"></param>
        void Subscribe(ISubscribableField<T> field);
    }
}