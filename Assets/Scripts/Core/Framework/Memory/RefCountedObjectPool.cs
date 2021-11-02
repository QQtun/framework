using LogUtil;
using System;
using System.Text;

namespace Core.Framework.Memory
{
    /// <summary>
    /// Author: chengdundeng
    /// Date: 2019/11/12
    /// Desc: Reference Count Object Pool 泛型版本
    /// </summary>
    public class RefCountedObjectPool<T> : IRefCountedObjectPool
        where T : class, IRefCountedObject
    {
        private ObjectPool<T> mObjectPoolImpl;

        public RefCountedObjectPool(ObjectPool<T>.Factory factoryFunc, int size)
        {
            Type t = typeof(T);

            StringBuilder builder = new StringBuilder();

            if (!string.IsNullOrEmpty(t.Namespace))
            {
                builder.Append(t.Namespace);
                builder.Append(".");
            }
            builder.Append(t.Name);

            if (t.IsGenericType)
            {
                Type[] genericArgs = t.GetGenericArguments();
                builder.Append("[");
                builder.Append(genericArgs[0].Name);
                for (int i = 1; i < genericArgs.Length; i++)
                {
                    builder.Append(",");
                    builder.Append(genericArgs[i].Name);
                }
                builder.Append("]");
            }

            TypeName = builder.ToString();

            mObjectPoolImpl = new ObjectPool<T>(factoryFunc, size);
        }

        /// <summary>
        /// Pool 中被配置的數量
        /// </summary>
        public int AllocateCount
        {
            get { return mObjectPoolImpl.AllocateCount; }
        }

        /// <summary>
        /// Pool 中已經建立的物件數量
        /// </summary>
        public int CreatedObjectCount
        {
            get { return mObjectPoolImpl.CreatedObjectCount; }
        }

        /// <summary>
        /// Pool 中可釋放的數量
        /// </summary>
        public int FreeCount
        {
            get { return mObjectPoolImpl.FreeCount; }
        }

        /// <summary>
        /// Pool 中保留數量
        /// </summary>
        public int ReservedCount
        {
            get { return mObjectPoolImpl.Size; }
            set { mObjectPoolImpl.Size = value; }
        }

        public string TypeName { get; private set; }

        /// <summary>
        /// Pool 中可用的數量
        /// </summary>
        public int UsableCount
        {
            get { return mObjectPoolImpl.UsableCount; }
        }

        /// <summary>
        /// 配置
        /// </summary>
        /// <returns></returns>
        public T Allocate()
        {
            return mObjectPoolImpl.Allocate();
        }

        /// <summary>
        /// 釋放
        /// </summary>
        /// <param name="refCountedObject"></param>
        public void Free(IRefCountedObject refCountedObject)
        {
            Free(refCountedObject as T);
        }

        /// <summary>
        /// 釋放
        /// </summary>
        /// <param name="obj"></param>
        public void Free(T obj)
        {
            if (obj == null)
            {
                return;
            }

            if (obj.UseCount != 0)
            {
                Debug.LogWarning("you should not free a object that useCount != 0 !!!!!");
                return;
            }

            mObjectPoolImpl.Free(obj);
        }
    }
}