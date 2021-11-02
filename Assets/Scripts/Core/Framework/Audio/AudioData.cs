using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Framework.Audio
{

    [System.Serializable]
    public class AudioData
    {
        //音頻資料
        public string url;//音樂檔位置
        public string groupName;//組別
        public string audioName;//音樂名稱

        //AudioSource資料
        public float SpatialBlend = 0;//混和設定
        public float MinDistance = 0;//音量最大距離
        public float MaxDistance = 0;//音量最小距離

        public void RestData() //TODO:重製數據更新
        {
            url = "";
            groupName = "";
            audioName = "";
            SpatialBlend = 0;
            MinDistance = 0;
            MaxDistance = 0;
        }
    }

}