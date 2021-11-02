using UnityEngine;

/// <summary>
/// 後製效果-動態模糊
/// 
/// 數據格式
/// motion 起始时间 持续时间
/// </summary>
namespace Core.Framework.Camera.CameraCommand
{
    public class CameraCommandMotion : CommandStruct
    {

        //是否啟動 動態模糊
        private bool mEnableMotionBlur = false;
        private float mMotionUsedTime = 0f;//現在運行時間
        private float mMotionDelayUsedTime = 0f;//現在延遲時間

        protected override void Init()
        {
            mEnableMotionBlur = true;
            mMotionUsedTime = 0;
            mMotionDelayUsedTime = 0;
        }

        protected override void UpdateDate()
        {
            if (isInit == false)
            { return; }

            //計算時間
            if (mEnableMotionBlur)
            {
                mMotionDelayUsedTime += Time.deltaTime;
                if (mMotionDelayUsedTime >= begintime)
                {
                    mMotionUsedTime += Time.deltaTime;
                    if (mMotionUsedTime >= durationtime)
                    {
                        mEnableMotionBlur = false;
                    }
                }
            }
        }

        public override void Execute() { }

        public override void RenderEffect(RenderTexture source, RenderTexture destination, ref Material material)
        {
            //啟動動態模糊
            //EnableKeyword意思是，Shader裡 #if MOTIONBLUR 為True 讓Shader能啟動
            if (mEnableMotionBlur && mMotionUsedTime > 0)
            {
                Debug.Log("mMotionMaterial.EnableKeyword(MOTIONBLUR)");
                material.EnableKeyword("MOTIONBLUR");
            }
            else
            {
                material.DisableKeyword("MOTIONBLUR");
            }
        }
    }

}