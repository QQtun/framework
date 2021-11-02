using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Core.Framework.Schedule
{
    public class ScheduleManager : Singleton<ScheduleManager>
    {
        private class Scheduler
        {
            public string Name { get; private set; }
            private int Key { get; set; }
            private float Interval { get; set; }
            private Func<int, float, object, bool> Func { get; set; }

            private UnityEngine.Object BindObj
            {
                get
                {
                    return mBindObj;
                }
                set
                {
                    mBindObj = value;
                    mHasBindObj = value != null;
                }
            }

            private object Obj { get; set; }

            private float mElapsedTime;
            private UnityEngine.Object mBindObj;
            private bool mHasBindObj;

            public void Init(string name, int key, float interval, Func<int, float, object, bool> func,
                UnityEngine.Object bindObj, object obj)
            {
                Name = name;
                Key = key;
                Interval = interval;
                Func = func;
                BindObj = bindObj;
                Obj = obj;

                mElapsedTime = 0.0f;
            }

            public void Update(float dt)
            {
                mElapsedTime += dt;
                if (!(mElapsedTime >= Interval))
                {
                    return;
                }

                if (mHasBindObj && BindObj == null)
                {
                    Instance.Unschedule(Key);
                    return;
                }

                bool again = Func.Invoke(Key, mElapsedTime, Obj);

                while (mElapsedTime >= Interval)
                {
                    mElapsedTime -= Interval;
                }

                if ((!again || (mHasBindObj && BindObj == null)) && Instance != null)
                {
                    Instance.Unschedule(Key);
                }
            }
        }

        private Queue<Scheduler> mPool = new Queue<Scheduler>();

        private LinkedList<Scheduler> mSchedulers = new LinkedList<Scheduler>();

        private Dictionary<int, LinkedListNode<Scheduler>> mKeyToSchedulerDic =
            new Dictionary<int, LinkedListNode<Scheduler>>();

        private Dictionary<string, int> mNameToSchedulerIdDic = new Dictionary<string, int>();
        private int mIncreaseKey;
        private Stopwatch mUpdateTimeTimer = new Stopwatch();

        /// <summary>
        ///     周期性呼叫特定函式
        ///     注意如果lag, deltaTime有可能超過兩個interval, 所以使用者如果需要很精準, 請自行檢查deltaTime再做處理
        /// </summary>
        /// <param name="interval">時間間距, 單位:秒</param>
        /// <param name="func">欲呼叫的函式[key,deltaTime,obj,callAgain]</param>
        /// <param name="bindObj">跟著此物線的生命週期結束</param>
        /// <param name="obj">傳給func的第三個參數使用</param>
        /// <returns>key, Unschedule時要使用</returns>
        public int Schedule(float interval, Func<int, float, object, bool> func,
            UnityEngine.Object bindObj = null, object obj = null)
        {
            return Schedule(string.Empty, interval, func, bindObj, obj);
        }

        /// <summary>
        ///     周期性呼叫特定函式, 指定使用特定名稱
        ///     注意如果lag, deltaTime有可能超過兩個interval, 所以使用者如果需要很精準, 請自行檢查deltaTime再做處理
        /// </summary>
        /// <param name="scheduleName">特定名稱</param>
        /// <param name="interval">時間間距, 單位:秒</param>
        /// <param name="func">欲呼叫的函式[key,deltaTime,obj,callAgain]</param>
        /// <param name="bindObj">跟著此物線的生命週期結束</param>
        /// <param name="obj">傳給func的第三個參數使用</param>
        /// <returns>key, Unschedule時要使用, 如果回傳-1代表scheduleName已經被使用過, 無法啟動</returns>
        public int Schedule(string scheduleName, float interval, Func<int, float, object, bool> func,
            UnityEngine.Object bindObj = null, object obj = null)
        {
            if (!string.IsNullOrEmpty(scheduleName) && mNameToSchedulerIdDic.ContainsKey(scheduleName))
            {
                return -1;
            }

            int key = ++mIncreaseKey;
            Scheduler s = AllocScheduler(scheduleName, key, interval, func, bindObj, obj);
            LinkedListNode<Scheduler> node = mSchedulers.AddLast(s);
            mKeyToSchedulerDic.Add(key, node);

            if (!string.IsNullOrEmpty(scheduleName))
            {
                mNameToSchedulerIdDic.Add(scheduleName, key);
            }
            return key;
        }

        /// <summary>
        ///     取消持續呼叫特定函式
        /// </summary>
        /// <param name="key"></param>
        public void Unschedule(int key)
        {
            LinkedListNode<Scheduler> node;
            if (mKeyToSchedulerDic.TryGetValue(key, out node))
            {
                Scheduler schedule = node.Value;

                mSchedulers.Remove(node);
                mKeyToSchedulerDic.Remove(key);

                if (!string.IsNullOrEmpty(schedule.Name))
                {
                    mNameToSchedulerIdDic.Remove(schedule.Name);
                }

                mPool.Enqueue(schedule);
            }
        }

        /// <summary>
        ///     取消持續呼叫特定函式
        /// </summary>
        /// <param name="scheduleName"></param>
        /// <returns></returns>
        public int Unschedule(string scheduleName)
        {
            int schedulerId;
            if (!mNameToSchedulerIdDic.TryGetValue(scheduleName, out schedulerId))
            {
                return -1;
            }

            Unschedule(schedulerId);
            return schedulerId;
        }

        private void Update()
        {
            var t = UnityEngine.Time.deltaTime;

            if (mSchedulers.Count > 0)
            {
                mUpdateTimeTimer.Reset();
                mUpdateTimeTimer.Start();

                var node = mSchedulers.First;
                while (node != null)
                {
                    var next = node.Next; // 可能在 update 中 移除node 所以必須先記住 下一個是哪個node
                    node.Value.Update(t);
                    node = next;
                }

                if (mUpdateTimeTimer.Elapsed.TotalMilliseconds > 100)
                {
                    UnityEngine.Debug.LogWarning("schedulers spent too much time now !!!!");
                }
            }
        }

        private Scheduler AllocScheduler(
            string name, int key, float interval, Func<int, float, object, bool> func,
                UnityEngine.Object bindObj, object obj)
        {
            Scheduler ret = null;
            if (mPool.Count > 0)
                ret = mPool.Dequeue();
            else
                ret = new Scheduler();

            ret.Init(name, key, interval, func, bindObj, obj);
            return ret;
        }
    }
}