using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Framework.Audio
{

    [RequireComponent(typeof(AudioSource))]
    public class AudioContructor : MonoBehaviour
    {
        [SerializeField]
        private AudioSource mAudioSource;//撥放器

        public AudioSource audioSource
        {
            get
            {
                //防止因mAudioSource無故消失
                if (mAudioSource == null)
                {
                    mAudioSource = this.gameObject.GetComponent<AudioSource>();
                }

                if (mAudioSource == null)
                {
                    mAudioSource = this.gameObject.AddComponent<AudioSource>();
                }

                return mAudioSource;
            }
        }

        [SerializeField]
        private AudioData mAudioData;//Excel設定資料
        public AudioData audioData
        {
            get
            {
                //防錯誤
                if (mAudioData == null)
                {
                    mAudioData = new AudioData();
                }

                return mAudioData;
            }
        }

        public AudioClip audioClip;//音樂文件

        //Audio資料
        public bool isLoop = false;//是否有重複
        public int loopNum = 0;//重複次數 -1為一直重複
        private int mLoopNumNow = 0;//當前重複次數
        public float delayTime = 0;//延遲時間

        [SerializeField]
        private bool isInit = false;//是否初始化完
        [SerializeField]
        private float mClipTime;//音樂文件時間
        [SerializeField]
        private float mTime;//撥放執行時間
        public bool isPause;//是否在暫停 
        private bool isPausePlayInit;//是否有設定Play過 

        #region  Editor用
        public bool IsPause { get => isPause; }
        public float ClipTime { get => mClipTime; }
        public float MTime { get => mTime; }

        #endregion

        /// <summary>
        /// Audio的Update由AudioManager控制
        /// </summary>
        public void AudioUpdate()
        {
            //偵測PlayAgain或是音樂玩了
            if (audioSource.loop != true && isInit == true)
            {
                //撥放完成
                if (audioSource.isPlaying == false
                && (mTime >= mClipTime && mClipTime != 0))
                {
                    if (loopNum > mLoopNumNow && isLoop == true)
                    {
                        PlayAgain();
                        mLoopNumNow++;
                        mTime = 0;
                    }
                    else
                    {
                        audioSource.Stop();
                        AudioRecycle();
                    }
                }
                else if (isPause == true)//暫停
                {
                    //暫停中什麼都不會做
                }
                else
                {
                    //撥放加時間，用於計算是否音樂有完成
                    mTime += Time.deltaTime;
                }
            }
        }

        /// <summary>
        /// 啟動Audio入口
        /// </summary>
        public void AudioStart()
        {
            if (null == audioSource)
            {
                LogUtil.Debug.LogError("AudioContructor 錯誤 無AudioSource");
                AudioRecycle();
                return;
            }

            #region  初始化
            audioSource.clip = audioClip;
            audioSource.minDistance = audioData.MinDistance;
            audioSource.maxDistance = audioData.MaxDistance;
            audioSource.spatialBlend = audioData.SpatialBlend;
            mClipTime = audioClip.length;
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            isLoop = false;
            isPausePlayInit = false;
            mLoopNumNow = 0;
            mTime = 0;

            if (loopNum == -1)
            {
                audioSource.loop = true;
            }
            else if (loopNum > 0)
            {
                isLoop = true;
            }
            #endregion

            //執行
            isInit = true;
            if (isPause == false)
            {
                if (delayTime > 0)
                {
                    audioSource.PlayDelayed(delayTime);
                    isPausePlayInit = true;
                }
                else
                {
                    audioSource.Play();
                    isPausePlayInit = true;
                }
            }
        }

        private void PlayAgain()
        {
            if (null == audioSource || null == audioSource.clip)
            {
                return;
            }

            if (!this.enabled
            || !gameObject.activeInHierarchy
            || !gameObject.activeSelf
            || !audioSource.enabled)
            {
                return;
            }
            audioSource.Stop();
            audioSource.Play();
        }

        /// <summary>
        /// 回收Audio物件放回到AudioPool
        /// </summary>
        public void AudioRecycle()
        {
            isInit = false;
            AudioManager.Instance.AudioContructorRecycle(audioData.groupName, this);
            mAudioData.RestData();
        }

        /// <summary>
        /// 暫停
        /// </summary>
        public void AudioPause()
        {
            if (audioSource != null)
            {
                isPause = true;
                audioSource.Pause();
            }
        }

        /// <summary>
        /// 重新撥放(暫停用)
        /// </summary>
        public void AudioUnPause()
        {
            if (audioSource != null)
            {
                isPause = false;
                if (isPausePlayInit == false)//是否有Play過才停，無則要重新Play
                {
                    if (delayTime > 0)
                    {
                        audioSource.PlayDelayed(delayTime);
                        isPausePlayInit = true;
                    }
                    else
                    {
                        audioSource.Play();
                        isPausePlayInit = true;
                    }
                }
                else
                {
                    audioSource.UnPause();
                }
            }
        }

        /// <summary>
        /// 停止
        /// </summary>
        public void AudioStop()
        {
            if (audioSource != null)
            {
                audioSource.Stop();
                AudioRecycle();
            }
        }

        /// <summary>
        /// 音樂靜音
        /// </summary>
        public void AudioMute(bool mute)
        {
            if (audioSource != null)
            {
                audioSource.mute = mute;
            }
        }

        /// <summary>
        /// 改音樂音量
        /// </summary>
        public void AudioVolume(float volume)
        {
            if (audioSource != null)
            {
                audioSource.volume = volume;
            }
        }

    }

}
