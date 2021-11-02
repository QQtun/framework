using UnityEngine;

/// <summary>
/// 畫面變黑，
/// 
/// 數據格式
/// reduce 起始时间 持续时间 灰度 淡入百分比 持续百分比 顏色(255,255,255格式)
/// 
/// 作法:
/// 透過SetGlobalColor方式 調整場景內所以Shader數據
/// 讓所有物件變黑
/// 
/// 參數:
/// 起始时间 單位 毫秒
/// 持续时间 單位 毫秒
/// 灰度 0~100 0為設定顏色 100為原色
/// 淡入百分比  持续百分比  設定在時間軸多少百分比時完成
/// 
/// 假定現在有個變黑時間軸 
/// 起始 0 持续 500 灰度 50 淡入 30 持续 30
/// 意思就是，
/// 在 500ms * 30% 的 "時間點" 要將灰度數據降到 0.5
/// 然後 500ms * (30% + 30%) 的 "時間點" 灰度依然是 0.5
/// 最後當過了 500ms * (30% + 30%) 的 "時間點"後，剩下的時間讓 灰度回到 1
/// </summary>
namespace Core.Framework.Camera.CameraCommand
{

    public class CameraCommandReduce : CommandStruct
    {

        //是否啟動畫面淡入
        private bool mEnableScreenreduce = false;

        private float mScreenreduceMin = 0.2f;//灰度值
        private float mScreenreduceDown = 0.75f;//淡入的時間點(百分比)
        private float mScreenreduceDuration = 0.0f;//持續的時間點(百分比)
        private float mScreenreduceUsedTime = 0f;//目前使用時間
        private float mScreenreduceDelayUsedTime = 0f;//目前延遲時間
        private float mScreenreduce = 1.0f;//灰度值

        private Color32 mScreenreduceTargetColor = Color.black;//設定目標顏色
        private Color32 mScreenreduceNowColor = Color.black;//現在顏色

        //Shader號碼
        private int mScreenReduceColor;
        private int mScreenReduce;

        //為了想是到外部Editor所以公開
        public float Screenreduce { get => mScreenreduce; }
        public float ScreenreduceDown { get => mScreenreduceDown; }
        public float ScreenreduceDuration { get => mScreenreduceDuration; }
        public Color32 ScreenreduceNowColor { get => mScreenreduceNowColor; }
        public Color32 ScreenreduceTargetColor { get => mScreenreduceTargetColor; }

        protected override void Init()
        {
            //那Shader號碼 //TODO:可以改成ShaderPropertyID下的數據，但ShaderPropertyID沒有初始化所以目前拿不到
            mScreenReduce = Shader.PropertyToID("_screenReduce");
            mScreenReduceColor = Shader.PropertyToID("_screenReduceColor");

            //初始化顏色，避免問題
            Shader.SetGlobalFloat(mScreenReduce, 1);
            Shader.SetGlobalColor(mScreenReduceColor, Color.black);

            //輸入數據
            mScreenreduceMin = data[0] / 100.0f; //灰度 0為設定顏色 100為白
            mScreenreduceDown = data[1] / 100.0f;//淡入百分比
            mScreenreduceDuration = data[2] / 100.0f;//持续百分比
            mScreenreduceTargetColor = new Color32(((byte)data[3]), ((byte)data[4]), ((byte)data[5]), 255);
            mEnableScreenreduce = true;
        }

        protected override void UpdateDate()
        {
            if (isInit == false)
            { return; }

            #region  計算淡入淡出所需數據

            //計算淡入淡出所需數據
            if (mEnableScreenreduce)
            {
                mScreenreduceDelayUsedTime += Time.deltaTime;
                if (mScreenreduceDelayUsedTime >= begintime)
                {
                    mScreenreduceUsedTime += Time.deltaTime;
                    if (mScreenreduceUsedTime >= (durationtime - begintime))
                    {
                        //終止
                        mEnableScreenreduce = false;
                    }
                    else if (mScreenreduceUsedTime >= (durationtime - begintime) * (mScreenreduceDown + mScreenreduceDuration))
                    {
                        //淡出 F 1=>0
                        float f = (mScreenreduceUsedTime - (durationtime - begintime) *
                         (mScreenreduceDown + mScreenreduceDuration)) /
                         ((durationtime - begintime) - (durationtime - begintime) *
                         (mScreenreduceDown + mScreenreduceDuration));

                        mScreenreduce = mScreenreduceMin + (1 - mScreenreduceMin) * f;
                        mScreenreduceNowColor = Color.Lerp(mScreenreduceTargetColor, Color.black, f);
                    }
                    else
                    {
                        //淡入 F 0=>1
                        float f = mScreenreduceUsedTime /
                        ((durationtime - begintime) * mScreenreduceDown);

                        f = f >= 1 ? 1 : f;

                        mScreenreduce = 1 - (1 - mScreenreduceMin) * f;
                        mScreenreduceNowColor = Color.Lerp(Color.black, mScreenreduceTargetColor, f);
                    }
                }
            }

            #endregion

            #region  更改場上所有物件的顏色

            //更改場上所有物件的顏色
            if (mEnableScreenreduce)
            {
                if (mScreenreduceUsedTime > 0)
                {
                    Shader.SetGlobalFloat(mScreenReduce, mScreenreduce);
                    Shader.SetGlobalColor(mScreenReduceColor, mScreenreduceNowColor);
                }
            }
            else
            {
                mScreenreduceUsedTime = 0;
                mScreenreduceDelayUsedTime = 0;
                mScreenreduce = 1.0f;
                Shader.SetGlobalFloat(mScreenReduce, mScreenreduce);
                Shader.SetGlobalColor(mScreenReduceColor, Color.black);
            }
            #endregion
        }

        public override void Execute() { }
    }


}