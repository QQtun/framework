using System.Diagnostics;

namespace Core.Framework.Utility
{
    /// <summary>
    ///     有限狀態機
    /// </summary>
    /// <typeparam name="T">狀態型別</typeparam>
    public class FiniteState<T>
    {
        private readonly object mLock = new object();

        /// <summary>
        ///     當前狀態
        /// </summary>
        private T mCurrentState;

        /// <summary>
        ///     初始狀態
        /// </summary>
        private T mInitialState;

        /// <summary>
        ///     是否剛進入目前狀態
        /// </summary>
        private bool mIsEntering;

        /// <summary>
        ///     下一個狀態
        /// </summary>
        private T mNextState;

        private T mPrevState;

        /// <summary>
        ///     是否準備轉移
        /// </summary>
        private bool mToTransit;

        /// <summary>
        ///     轉移時間
        /// </summary>
        private Stopwatch mTransitTime = Stopwatch.StartNew();

        private bool mTransitWhenTick;

        /// <summary>
        ///     當前狀態
        /// </summary>
        public T Current
        {
            get
            {
                lock (mLock)
                {
                    return mCurrentState;
                }
            }
        }

        public T Previous
        {
            get
            {
                lock (mLock)
                {
                    return mPrevState;
                }
            }
        }

        /// <summary>
        ///     在這次 Tick() 是否為剛剛進入目前的狀態，可用來引發進入事件。
        /// </summary>
        public bool Entering
        {
            get
            {
                lock (mLock)
                {
                    return mIsEntering;
                }
            }
        }

        /// <summary>
        ///     從進入這個狀態開始到現在過了多少秒。
        /// </summary>
        public double Elapsed
        {
            get
            {
                lock (mLock)
                {
                    return mTransitTime.Elapsed.TotalSeconds;
                }
            }
        }

        /// <summary>
        ///     建構式
        /// </summary>
        /// <param name="initialState">初始狀態</param>
        public FiniteState(T initialState)
        {
            mInitialState = initialState;
            Init();
        }

        /// <summary>
        ///     要求轉移到新狀態，實際轉移會在下一次呼叫 Tick() 時執行
        /// </summary>
        /// <param name="newState">要轉移過去的新狀態</param>
        public void Transit(T newState, bool immediately = false)
        {
            lock (mLock)
            {
                if(immediately)
                {
                    mPrevState = mCurrentState;
                    mCurrentState = newState;
                    mTransitTime.Reset();
                    mTransitTime.Start();
                    mToTransit = true;
                    mTransitWhenTick = false;
                }
                else
                {
                    mNextState = newState;
                    mToTransit = true;
                    mTransitWhenTick = true;
                }
            }
        }

        /// <summary>
        ///     通常被 timer 或 main loop 所呼叫, 表示進入下一個狀態
        /// </summary>
        /// <example>
        ///     <code><![CDATA[
        ///      switch ( _state.Tick() )
        ///      {
        ///      case STATE_INITIAL:
        ///          ....
        ///
        ///  ]]></code>
        /// </example>
        /// <returns>目前狀態</returns>
        public T Tick()
        {
            lock (mLock)
            {
                if (mToTransit)
                {
                    mToTransit = false;
                    if(mTransitWhenTick)
                    {
                        mTransitTime.Reset();
                        mTransitTime.Start();
                        mPrevState = mCurrentState;
                        mCurrentState = mNextState;
                    }
                    mIsEntering = true;
                }
                else
                {
                    mIsEntering = false;
                }

                return mCurrentState;
            }
        }

        /// <summary>
        ///     初始有限狀態機
        /// </summary>
        public void Init()
        {
            mNextState = mInitialState;
            mPrevState = mInitialState;
            mTransitTime.Reset();
            mTransitTime.Start();
            mToTransit = true;
            mCurrentState = mInitialState;
            mIsEntering = false;
            mTransitWhenTick = false;
        }
    }
}