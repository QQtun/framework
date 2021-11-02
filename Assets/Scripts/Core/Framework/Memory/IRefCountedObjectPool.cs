namespace Core.Framework.Memory
{
    /// <summary>
    /// Author: chengdundeng
    /// Date: 2019/11/12
    /// Desc: Reference Count Object Pool Interface
    /// </summary>
    public interface IRefCountedObjectPool
    {
        /// <summary>
        ///     Pool內類別名稱
        /// </summary>
        string TypeName
        {
            get;
        }

        /// <summary>
        ///     還有幾個物件可以使用
        /// </summary>
        int UsableCount
        {
            get;
        }

        /// <summary>
        ///     Pool內最多可以保留多少物件
        /// </summary>
        int ReservedCount
        {
            get; set;
        }

        /// <summary>
        ///     產生過多少物件
        /// </summary>
        int CreatedObjectCount
        {
            get;
        }

        /// <summary>
        ///     Pool被要求配置物件次數
        /// </summary>
        int AllocateCount
        {
            get;
        }

        /// <summary>
        ///     物件被收回次數
        /// </summary>
        int FreeCount
        {
            get;
        }

        /// <summary>
        ///     釋放物件
        /// </summary>
        /// <param name="refCountedObject"></param>
        void Free(IRefCountedObject refCountedObject);
    }
}