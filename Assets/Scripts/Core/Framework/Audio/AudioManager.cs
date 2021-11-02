using System.Collections.Generic;
using UnityEngine;
using Core.Game.Table;
using Core.Framework.Res;
using System.Collections;

namespace Core.Framework.Audio
{
    /// <summary>
    /// 這裡AudioManager的基本核心功能
    /// 命令函式會太多，分開處理
    /// </summary>
    public partial class AudioManager : Singleton<AudioManager>
    {
        //物件池裡待命的
        private Queue<AudioContructor> mAudioContructorPool = new Queue<AudioContructor>();

        //場景上有的物件
        private Dictionary<string, List<AudioContructor>> mAudioNowList = new Dictionary<string, List<AudioContructor>>();

        //是否全體靜音
        private bool isAllMute = false;
        //是否全體暫停
        private bool isAllPause = false;

        public class AudioSetting//存放設定群組資料
        {
            public bool isMute = false;
            public bool isPause = false;
            public float volume = 1;
        }

        //設定的群組資料
        private Dictionary<string, AudioSetting> mAudioSetting = new Dictionary<string, AudioSetting>();


        #region  Editor用
        public Queue<AudioContructor> AudioContructorPool { get => mAudioContructorPool; }
        public Dictionary<string, List<AudioContructor>> AudioNowList { get => mAudioNowList; }
        public bool IsAllMute { get => isAllMute; }
        public bool IsAllPause { get => isAllPause; }
        public Dictionary<string, AudioSetting> MAudioSetting { get => mAudioSetting; }

        #endregion

        private void Update()
        {
            //跑AudioContructor的Update
            if (mAudioNowList.Count != 0)
            {
                Dictionary<string, List<AudioContructor>>.Enumerator enumerator = mAudioNowList.GetEnumerator();

                while (enumerator.MoveNext())
                {
                    for (int i = enumerator.Current.Value.Count - 1; i >= 0; i--)
                    {
                        if (enumerator.Current.Value[i] == null)
                        {
                            LogUtil.Debug.LogWarning("發生AudioNowList遭刪除，請檢查");
                            enumerator.Current.Value.RemoveAt(i);
                        }
                        else
                        {
                            enumerator.Current.Value[i].AudioUpdate();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 撥放音樂入口
        /// </summary>
        /// <param name="url">音樂位置</param>
        /// <param name="objParent">放置在哪個物件下</param>
        /// <param name="volume">音量大小</param>
        /// <param name="delayTime">延遲時間 (單位 秒)</param>
        /// <param name="loopNum">重複次數 (-1為無限 0為1次)</param>
        /// <param name="isPause">是否暫停</param>
        public void PlayAudio(string url, Transform objParent,
        float volume = 1, float delayTime = 0, int loopNum = 0, bool pause = false)
        {
            #region  檢查

            if (isAllMute == true)
            {
                LogUtil.Debug.LogError("已被設定成靜音");
            }

            if (isAllPause == true)
            {
                LogUtil.Debug.LogError("全體音樂暫停中");
            }

            //抓Excel資料
            Core.Game.Table.Audios data = TableGroup.AudiosTable.Get(url);
            //查Excel資料在不在
            if (data == null)
            {
                LogUtil.Debug.LogError("找不到" + url + "的Excel資料");
                return;
            }

            //查群組設定
            AudioSetting audioSetting;
            if (mAudioSetting.TryGetValue(data.GroupName, out audioSetting) == false)
            {
                mAudioSetting.Add(data.GroupName, new AudioSetting());
                audioSetting = mAudioSetting[data.GroupName];
            }
            else
            {
                //TODO:新增群組資料時這邊也要新增
                //查該群組音樂是否是被設定成不能撥放
                if (audioSetting.isMute == true)
                {
                    LogUtil.Debug.LogError(data.GroupName + "該群組被設定成靜音");
                }

                if (audioSetting.isPause == true)
                {
                    LogUtil.Debug.LogError(data.GroupName + "該群組被設定成暫停撥放");

                }

            }

            #endregion

            #region 設定音樂物件

            //準備音樂物件
            AudioContructor audioContructor = PopAudioContructorPool();
            AudioData audioData = audioContructor.audioData;

            if (mAudioNowList.ContainsKey(data.GroupName) == false)//檢查是否有Key值
            {
                mAudioNowList.Add(data.GroupName, new List<AudioContructor>());
            }

            mAudioNowList[data.GroupName].Add(audioContructor);//添加到場景List裡

            //遷移到目標
            audioContructor.gameObject.SetActive(true);
            audioContructor.gameObject.transform.parent = objParent;

            #region 讀取Excel設定 //TODO:Excel改動 就需要改這邊

            audioData.url = data.UrlPath;//URL
            audioData.groupName = data.GroupName;//組別
            audioData.audioName = data.AudioName;//音樂名稱

            //AudioSource資料
            audioData.SpatialBlend = data.SpatialBlend;//混和設定
            audioData.MinDistance = data.MinDistance;//音量最大距離
            audioData.MaxDistance = data.MaxDistance;//音量最小距離

            audioContructor.delayTime = delayTime;//延遲時間 單位 毫秒
            audioContructor.loopNum = loopNum;//Loop次數

            audioContructor.audioSource.volume = volume;//設定音量

            audioContructor.audioSource.mute = isAllMute;//是否靜音

            audioContructor.isPause = isAllPause;//是否暫停

            if (pause == true)
            {
                audioContructor.isPause = true;
            }

            #region  設定群組資料

            if (isAllMute == false)//是否有群組靜音
            {
                if (audioSetting.isMute == true)
                {
                    audioContructor.audioSource.mute = true;
                }
                else
                {
                    audioContructor.audioSource.mute = false;
                }
            }

            if (volume == 1)//如果函式有設定音量，以函式為準
            {
                audioContructor.audioSource.volume = audioSetting.volume;
            }

            if (pause == false && isAllPause == false)//如果函式有設定，以函式為準
            {
                if (audioSetting.isPause == true)
                {
                    audioContructor.isPause = true;
                }
                else
                {
                    audioContructor.isPause = false;
                }
            }

            #endregion

            #endregion

            //拿取音樂Clip + 執行
            ResourceManager.Instance.LoadAudioAssetAsync(url, (audio) =>
            {
                audioContructor.audioClip = audio;

                //設定完執行
                audioContructor.AudioStart();
            });

            #endregion

        }

        /// <summary>
        /// 拿音樂物件
        /// </summary>
        private AudioContructor PopAudioContructorPool()
        {
            AudioContructor audioContructorObj = null;

            //進行檢查
            if (mAudioContructorPool.Count > 0)
            {
                while (audioContructorObj == null)
                {
                    audioContructorObj = mAudioContructorPool.Dequeue();
                }
            }

            //搜尋無則新增一個
            if (audioContructorObj == null)
            {
                GameObject obj = new GameObject();
                obj.name = "AudioContructor";
                obj.AddComponent<AudioSource>();
                audioContructorObj = obj.AddComponent<AudioContructor>();
            }

            return audioContructorObj;
        }

        /// <summary>
        /// 回收物件
        /// </summary>
        /// <param name="obj">Audio物件</param>
        /// <param name="groupName">群組名</param>
        /// <param name="contructor">Contructor組件</param>
        public void AudioContructorRecycle(string groupName, AudioContructor contructor)
        {
            contructor.gameObject.transform.parent = this.transform;
            contructor.gameObject.SetActive(false);
            mAudioContructorPool.Enqueue(contructor);
            mAudioNowList[groupName].Remove(contructor);
        }


        /// <summary>
        /// 設定群組資料
        /// </summary>
        /// <param name="groupName">群祖名</param>
        /// <param name="setting">設定</param>
        public void SetGroupSetting(string groupName, AudioSetting setting)
        {
            mAudioSetting[groupName] = setting;
        }


    }
}