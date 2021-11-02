using LogUtil;

namespace Core.Framework.Memory
{
    /// <summary>
    /// Author: chengdundeng
    /// Date: 2019/11/12
    /// Desc: Reference Count Object 抽象層
    /// </summary>
    public abstract class RefCountedObject : IRefCountedObject
    {
        private IRefCountedObjectPool mPoolObj;

        protected RefCountedObject(IRefCountedObjectPool pool)
        {
            UseCount = 0;
            mPoolObj = pool;
        }

        public virtual int InstanceId { get; protected set; }
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

            OnRelease();
        }

        /// <summary>
        ///     增加引用次數, 避免物件被回收
        /// </summary>
        public virtual void Retain()
        {
            ++UseCount;
        }

        protected virtual void OnRelease()
        {
            if (UseCount == 0 && mPoolObj != null)
            {
                InstanceId = 0;
                mPoolObj.Free(this);
            }
        }
    }

    /// <summary>
    /// Author: chengdundeng
    /// Date: 2019/11/12
    /// Desc: Reference Count Object 泛型抽象層
    /// </summary>
    public abstract class RefCountedObject<T> : RefCountedObject
        where T : RefCountedObject<T>, new()
    {
        protected static ObjectPool<T>.Factory factoryFunc = () => new T();
        private static int s_instanceId = 0;
        private static object s_instanceIdLock = new object();
        private static volatile RefCountedObjectPool<T> s_poolRefCountedObject;
        private static int s_poolSize = 10;
        private static object s_syncRoot = new object();

        protected RefCountedObject() : base(s_poolRefCountedObject)
        {
        }

        public sealed override int InstanceId { get; protected set; }

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
            var type = typeof(T);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(type.TypeHandle);

            if (s_poolRefCountedObject == null)
            {
                if (factoryFunc == null)
                {
                    Debug.LogError(string.Format("Need initialize for factoryFunc of {0}", typeof(T).Name));
                }

                lock (s_syncRoot)
                {
                    if (s_poolRefCountedObject == null)
                        s_poolRefCountedObject = ObjectPoolManager.Instance.GetPool<T>() ??
                                                 ObjectPoolManager.Instance.CreatePool(s_poolSize, factoryFunc);
                }
            }

            T obj = s_poolRefCountedObject.Allocate();
            obj.Retain();
            lock (s_instanceIdLock)
            {
                obj.InstanceId = ++s_instanceId;
            }
            return obj;
        }
    }
}