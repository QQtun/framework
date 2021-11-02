namespace Core.Framework.Memory
{
    /// <summary>
    /// Author: chengdundeng
    /// Date: 2019/11/12
    /// Desc: Reference Count Object Interface
    /// </summary>
    public interface IRefCountedObject
    {
        /// <summary>
        ///     物件引用次數
        /// </summary>
        int UseCount { get; }

        /// <summary>
        ///     物件編號, 方便判斷是不是由pool產生的物件
        /// </summary>
        int InstanceId { get; }

        /// <summary>
        ///     增加引用次數, 避免物件被回收
        /// </summary>
        void Retain();

        /// <summary>
        ///     減少引用次數, 當物件不使用時呼叫, 當 UseCount從1減為0時發生回收
        /// </summary>
        void Release();
    }
}