using System;

namespace Core.Game.Utility
{
    public static class ServerTimeUtil
    {
        // 以下單位ms
        private static long mAccLatency = 0;
        private static long mMaxLatency = long.MinValue;
        private static long mMinLatency = long.MaxValue;

        private static long mCount = 0;

        public static long ClientSubServerNowTick { get; set; } // 單位ticks

        public static long MillisecondNow
        {
            get
            {
                long num = DateTime.Now.Ticks - ClientSubServerNowTick;
                return num / TimeSpan.TicksPerMillisecond;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="latency">單位ms</param>
        public static void AddLatency(long latency)
        {
            mCount++;
            mAccLatency += latency;
            if (latency > mMaxLatency)
            {
                mMaxLatency = latency;
            }
            if (latency < mMinLatency)
            {
                mMinLatency = latency;
            }
        }

        public static void LogLatency()
        {
            if (mCount <= 0)
                return;

            LogUtil.Debug.LogError($"執行{mCount}次, 平均延遲{(int)(mAccLatency / mCount)}ms, 最大延遲{mMaxLatency}ms, 最小延遲{mMinLatency}ms");
            mCount = 0;
            mAccLatency = 0;
            mMaxLatency = long.MinValue;
            mMinLatency = long.MaxValue;
        }
    }
}