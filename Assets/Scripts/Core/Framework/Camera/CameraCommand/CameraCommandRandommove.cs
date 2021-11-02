using UnityEngine;

/// <summary>
/// 攝影機震動
/// 
/// 數據格式
/// randommove 起始时间 持续时间 X震动 Y震动 间隔
/// </summary>
namespace Core.Framework.Camera.CameraCommand
{

    public class CameraCommandRandommove : CommandStruct
    {
        private float mXShakeOffectData;//X震動幅度原始資料
        private float mYShakeOffectData;//Y震動幅度原始資料
        private float mXShakeOffect;//X震動幅度
        private float mYShakeOffect;//Y震動幅度
        private float mShakeInterval;//震動間隔
        private float mShakeTime; //搖動時間
        private Vector3 mShakeOffset;//攝影機搖動位移
        private Vector3 mCameraOriginPosition; //攝影機座標

        //為了想是到外部Editor所以公開
        public float XShakeOffect { get => mXShakeOffect; }
        public float YShakeOffect { get => mYShakeOffect; }
        public Vector3 ShakeOffset { get => mShakeOffset; }

        protected override void Init()
        {
            //拿取資料
            mXShakeOffectData = data[0] / 100.0f;
            mYShakeOffectData = data[1] / 100.0f;
            mShakeInterval = data[2] / 1000.0f;

            //拿攝影機初始位置
            mCameraOriginPosition = cameraCommandManager.mainTransform.position;
        }

        protected override void UpdateDate()
        {
            if (isInit == false)
            { return; }


            //更新搖動時間
            mShakeTime += Time.deltaTime;

            mXShakeOffect = 0;
            mYShakeOffect = 0;

            //當時間超過間隔就震動攝影機
            if (mShakeTime > mShakeInterval)
            {
                mShakeTime -= mShakeInterval;
                mXShakeOffect = Random.Range(-mXShakeOffectData, +mXShakeOffectData);
                mYShakeOffect = Random.Range(-mYShakeOffectData, +mYShakeOffectData);
            }

            mShakeOffset = new Vector3(mXShakeOffect, mYShakeOffect, 0);
        }

        public override void Execute()
        {
            if (isInit == false)
            { return; }

            //震動
            cameraCommandManager.mainTransform.position = (mCameraOriginPosition + mShakeOffset);
        }
    }

}