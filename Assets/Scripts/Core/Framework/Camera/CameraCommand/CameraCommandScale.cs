using UnityEngine;

/// <summary>
/// 攝影機縮放(以百分比計算)
/// 例 : 50指的是攝影機會向前縮短距離到原來的50%
/// 
/// 數據格式
/// scale 起始时间 持续时间 拉近距离 淡入时间
/// </summary>
namespace Core.Framework.Camera.CameraCommand
{

    public class CameraCommandScale : CommandStruct
    {
        private float mShortenData; //拉近距離原始資料

        private float mShorten;//拉近距離 百分比

        private float mDurationTime;//拉近的持續時間

        private Vector3 mScaleTarget;//攝影機縮放目標物

        private float mDistance;//攝影機與目標物的距離

        private Vector3 mCameraOriginPosition; //攝影機座標

        //為了顯示到外部Editor所以公開
        public Vector3 ScaleTarget { get => mScaleTarget; }
        public float Shorten { get => mShorten; }
        private float mDis;
        public float Dis { get => mDis; }


        protected override void Init()
        {
            mShortenData = data[0] / 100.0f;
            //如果攝影機縮放百分比超過 100% 強制轉成 100%
            if (mShortenData > 1)
            {
                mShortenData = 1;
            }

            mDurationTime = data[1] / 1000.0f;

            mScaleTarget = cameraCommandManager.cameraTarget.position;

            mCameraOriginPosition = cameraCommandManager.mainCamera.transform.position;

            mDistance = Vector3.Distance(mCameraOriginPosition, mScaleTarget);
        }

        protected override void UpdateDate()
        {
            if (isInit == false)
            { return; }

            //如果再時間範圍內 拉近距離(以百分比計算)
            if (time - begintime <= mDurationTime)
            {
                mShorten = (1 - mShortenData * (time - begintime) / mDurationTime);
            }
            else
            {
                mShorten = 1;
            }

            //新的距離
            mDis = mDistance * mShorten;

            Vector3 newPos = (mCameraOriginPosition - mScaleTarget).normalized * mDis; //計算距離多遠

            mCameraOriginPosition = mScaleTarget + newPos; //新座標

        }
        public override void Execute()
        {
            if (isInit == false)
            { return; }

            cameraCommandManager.mainTransform.position = mCameraOriginPosition;
        }
    }

}