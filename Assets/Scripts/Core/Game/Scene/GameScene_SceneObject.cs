using Core.Framework.Event;
using Core.Framework.Utility;
using Core.Game.Event;
using Core.Game.Logic;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Game.Scene
{
    public partial class GameScene
    {
        private Dictionary<string, ISceneObject> mNameToObjectDic = new Dictionary<string, ISceneObject>();
        private Dictionary<int, ISceneObject> mIdToObjectDic = new Dictionary<int, ISceneObject>();
        // 下面兩個是lock時 暫時的List
        private List<ISceneObject> mWaittingForAddObjects = new List<ISceneObject>();
        private List<ISceneObject> mWaittingForDelObjects = new List<ISceneObject>();
        // 
        private Dictionary<int, ISceneObject> mPreparingObject = new Dictionary<int, ISceneObject>();

        private int mLockingCount = 0;

        public event Action<ISceneObject> OnObjectAdded;
        public event Action<ISceneObject> OnObjectRemoved;

        private int LockObjectDic
        {
            get { return mLockingCount; }
            set
            {
                mLockingCount = value;
                if (value == 0)
                {
                    for (int i = 0; i < mWaittingForAddObjects.Count; i++)
                    {
                        Add(mWaittingForAddObjects[i]);
                    }
                    mWaittingForAddObjects.Clear();
                    for (int i = 0; i < mWaittingForDelObjects.Count; i++)
                    {
                        Remove(mWaittingForDelObjects[i]);
                    }
                    mWaittingForDelObjects.Clear();
                }
            }
        }

        public void Add(ISceneObject obj)
        {
            try
            {
                if (string.IsNullOrEmpty(obj.Name))
                {
                    Debug.LogError("obj name can't be null or empty !!");
                    return;
                }
                if (mNameToObjectDic.ContainsKey(obj.Name))
                {
                    return;
                }
                if (LockObjectDic > 0)
                {
                    mWaittingForAddObjects.Add(obj);
                    return;
                }

                mNameToObjectDic.Add(obj.Name, obj);
                if (obj.Id > 0)
                    mIdToObjectDic.Add(obj.Id, obj);

                OnObjectAdded?.Invoke(obj);
                EventSystem.Instance.SendStringKeyEvent(EventKey.OnObjectAdded, obj);
            }
            catch (System.Exception e)
            {
                LogUtil.Debug.LogError(e);
            }
        }

        public void Remove(ISceneObject obj)
        {
            if (obj == null)
            {
                return;
            }
            try
            {
                for (int i = 0; i < mWaittingForAddObjects.Count; i++)
                {
                    if (obj.Name == mWaittingForAddObjects[i].Name)
                    {
                        mWaittingForAddObjects.RemoveAt(i);
                        return;
                    }
                }
                if (LockObjectDic > 0)
                {
                    mWaittingForDelObjects.Add(obj);
                    return;
                }

                mNameToObjectDic.Remove(obj.Name);
                mIdToObjectDic.Remove(obj.Id);
                EventSystem.Instance.SendStringKeyEvent(EventKey.OnObjectRemoved, obj);
                //ObjectPool.Recycle(obj);
            }
            catch (Exception e)
            {
                LogUtil.Debug.LogError(e);
            }
        }

        public ISceneObject FindByName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            for (int i = 0; i < mWaittingForDelObjects.Count; i++)
            {
                if (mWaittingForDelObjects[i].Name == name)
                {
                    return null;
                }
            }

            mNameToObjectDic.TryGetValue(name, out var ret);
            return ret;
        }

        public ISceneObject FindById(int id)
        {
            if (id == 0 || id < 0)
                return null;

            for (int i = 0; i < mWaittingForDelObjects.Count; i++)
            {
                if (mWaittingForDelObjects[i].Id == id)
                {
                    return null;
                }
            }

            mIdToObjectDic.TryGetValue(id, out var ret);
            return ret;
        }

        public ISceneObject Find(Func<ISceneObject, bool> handler)
        {
            ISceneObject ret = null;
            var iter = mIdToObjectDic.GetEnumerator();
            LockObjectDic++;
            while (iter.MoveNext())
            {
                ISceneObject obj = iter.Current.Value;
                if (handler(ret))
                {
                    ret = obj;
                    break;
                }
            }
            iter.Dispose();
            LockObjectDic--;
            return ret;
        }

        private void AddPreparingObj(ISceneObject obj)
        {
            if(mPreparingObject.ContainsKey(obj.Id))
            {
                LogUtil.Debug.Log("AddPreparingObj obj exist id=" + obj.Id);
            }
            mPreparingObject[obj.Id] = obj;
        }

        private bool TryGetPreparingObj(int id, out ISceneObject obj)
        {
            return mPreparingObject.TryGetValue(id, out obj);
        }

        private ISceneObject PopPreparingObj(int id)
        {
            mPreparingObject.TryGetValue(id, out var ret);
            return ret;
        }

        public ISceneObject FindTargetNear(Vector2Int center, Table.Magics skill)
        {
            // TODO
            switch (skill.TargetType[0])
            {
                case 3:
                {
                    // monster
                    return FindTargetNear(center, SpriteType.Monster);
                }
            }
            return null;
        }

        public ISceneObject FindTargetNear(Vector2Int center, SpriteType spriteType)
        {
            for (int i = 500; i <= 2000; i += 500)
            {
                var found = FindTargetInRange(center, i, Vector2.right, 180, spriteType);
                if (found != null)
                    return found;
            }
            return null;
        }

        public ISceneObject FindTargetInRange(Vector2Int center, float radius, Vector2 direction, float angle, SpriteType spriteType)
        {
            var dirV3 = direction.ToVector3();
            var radiusSqr = radius * radius;
            var iter = mIdToObjectDic.GetEnumerator();
            while(iter.MoveNext())
            {
                var obj = iter.Current.Value;
                if (obj == null)
                    continue;
                var pos = obj.Coordinate;
                if ((pos - center).sqrMagnitude > radiusSqr)
                    continue;
                var tarDir = (pos - center).ToVector3f();
                if (Vector3.Angle(dirV3, tarDir) > angle)
                    continue;
                if (obj.SpriteType != spriteType)
                    continue;
                return obj;
            }
            iter.Dispose();
            return null;
        }
    }
}