using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Framework.Memory
{
    /// <summary>
    /// Author: chengdundeng
    /// Date: 2019/11/12
    /// Desc: Object Pool Manager
    /// </summary>
    public class ObjectPoolManager
    {
        private static volatile ObjectPoolManager sInstance;
        private static object sSyncRoot = new Object();

        private Dictionary<Type, IRefCountedObjectPool> mObjectPools;

        private ObjectPoolManager()
        {
            mObjectPools = new Dictionary<Type, IRefCountedObjectPool>();
        }

        /// <summary>
        /// 取出實體
        /// </summary>
        public static ObjectPoolManager Instance
        {
            get
            {
                if (sInstance == null)
                {
                    lock (sSyncRoot)
                    {
                        if (sInstance == null)
                            sInstance = new ObjectPoolManager();
                    }
                }
                return sInstance;
            }
        }

        /// <summary>
        ///     建立Pool物件
        /// </summary>
        /// <typeparam name="T">Pool內物件類別</typeparam>
        /// <param name="poolSize">Pool保存物件最大數量</param>
        /// <param name="factoryFunc"></param>
        /// <returns></returns>
        public RefCountedObjectPool<T> CreatePool<T>(int poolSize, ObjectPool<T>.Factory factoryFunc)
            where T : class, IRefCountedObject
        {
            Type t = typeof(T);
            IRefCountedObjectPool pool;
            lock (mObjectPools)
            {
                if (!mObjectPools.TryGetValue(t, out pool))
                {
                    pool = new RefCountedObjectPool<T>(factoryFunc, poolSize);
                    mObjectPools.Add(t, pool);
                }
            }

            return pool as RefCountedObjectPool<T>;
        }

        /// <summary>
        ///     取得Pool物件
        /// </summary>
        /// <typeparam name="T">Pool內物件類別</typeparam>
        /// <returns></returns>
        public RefCountedObjectPool<T> GetPool<T>()
            where T : class, IRefCountedObject
        {
            Type t = typeof(T);
            IRefCountedObjectPool pool;
            lock (mObjectPools)
            {
                if (mObjectPools.TryGetValue(t, out pool))
                {
                    return pool as RefCountedObjectPool<T>;
                }
            }
            return null;
        }

        /// <summary>
        ///     取得所有Pool物件
        /// </summary>
        /// <returns></returns>
        public List<IRefCountedObjectPool> GetPools()
        {
            lock (mObjectPools)
            {
                return mObjectPools.Values.ToList();
            }
        }
    }
}