#if !SERVERCONSOLE

using UnityEngine;

namespace Core.Framework.Memory
{
    /// <summary>
    /// Author: chengdundeng
    /// Date: 2019/11/12
    /// Desc: Yieldable Reference Count Object
    /// </summary>
    public abstract class YieldableRefCountedObject : CustomYieldInstruction, IRefCountedObject
    {
        private IRefCountedObjectPool mPoolObj;

        protected YieldableRefCountedObject(IRefCountedObjectPool pool)
        {
            UseCount = 0;
            mPoolObj = pool;
        }

        /// <summary>
        /// InstanceID 用來識別物件編號
        /// </summary>
        public virtual int InstanceId { get; protected set; }

        /// <summary>
        /// 引用次數
        /// </summary>
        public int UseCount { get; private set; }

        /// <summary>
        ///     減少引用次數, 當物件不使用時呼叫, 當 UseCount從1減為0時發生回收
        /// </summary>
        public virtual void Release()
        {
            if (UseCount > 0)
            {
                --UseCount;
            }
            else
            {
                return;
            }

            if (UseCount == 0 && mPoolObj != null)
            {
                mPoolObj.Free(this);
            }
        }

        /// <summary>
        ///     增加引用次數, 避免物件被回收
        /// </summary>
        public virtual void Retain()
        {
            ++UseCount;
        }
    }

    /// <summary>
    /// Author: chengdundeng
    /// Date: 2019/11/12
    /// Desc: Generic Yieldable Reference Count Object
    /// </summary>
    public abstract class YieldableRefCountedObject<T> : YieldableRefCountedObject
        where T : class, IRefCountedObject
    {
        protected static ObjectPool<T>.Factory factoryFunc;
        private static RefCountedObjectPool<T> s_poolRefCountedObject;
        private static int s_poolSize = 10;

        protected YieldableRefCountedObject() : base(s_poolRefCountedObject)
        {
        }

        /// <summary>
        ///     改變保存物件最大數量
        /// </summary>
        /// <param name="size"></param>
        public static void SetReserveSize(int size)
        {
            s_poolSize = size;
            if (s_poolRefCountedObject != null)
            {
                s_poolRefCountedObject.ReservedCount = s_poolSize;
            }
        }

        /// <summary>
        ///     要求配置物件 ( 取代new功能 )
        /// </summary>
        /// <returns></returns>
        protected static T Allocate()
        {
            if (s_poolRefCountedObject == null)
            {
                if (factoryFunc == null)
                {
                    Debug.LogError(string.Format("Need initialize for factoryFunc of {0}", typeof(T).Name));
                }

                s_poolRefCountedObject = ObjectPoolManager.Instance.GetPool<T>() ??
                                        ObjectPoolManager.Instance.CreatePool(s_poolSize, factoryFunc);
            }

            T obj = s_poolRefCountedObject.Allocate();
            obj.Retain();
            return obj;
        }
    }
}

#endif