using System.Collections.Generic;
using UnityEngine;
using Core.Game.Table;
using Core.Framework.Res;

namespace Core.Framework.Audio
{
    /// <summary>
    /// AudioManager命令函式
    /// </summary>
    public partial class AudioManager : Singleton<AudioManager>
    {
        #region 靜音

        /// <summary>
        /// 新增群組靜音
        /// </summary>
        /// <param name="groupName">組名</param>
        public void AddMuteAudioGroup(string groupName)
        {
            AudioSetting setting;

            if (mAudioSetting.TryGetValue(groupName, out setting) == true)
            {
                setting.isMute = true;
            }
            else
            {
                mAudioSetting.Add(groupName, new AudioSetting() { isMute = true });
            }

            AudioMute(groupName, true);
        }

        /// <summary>
        /// 刪除群組靜音
        /// </summary>
        /// <param name="groupName">組名</param>
        public void RemoveMuteAudioGroup(string groupName)
        {
            AudioSetting setting;

            if (mAudioSetting.TryGetValue(groupName, out setting) == true)
            {
                setting.isMute = false;
            }
            else
            {
                mAudioSetting.Add(groupName, new AudioSetting() { isMute = false });
            }
            AudioMute(groupName, false);
        }

        /// <summary>
        /// 全部靜音
        /// </summary>
        public void SetMute(bool mute)
        {
            isAllMute = mute;
            if (mute == true)
            {
                AudioMute(true);
            }
            else
            {
                AudioMute(false);
            }
        }

        /// <summary>
        /// 全體靜音
        /// </summary>
        private void AudioMute(bool mute)
        {
            Dictionary<string, List<AudioContructor>>.Enumerator enumerator = mAudioNowList.GetEnumerator();

            while (enumerator.MoveNext())
            {
                for (int i = enumerator.Current.Value.Count - 1; i >= 0; i--)
                {
                    enumerator.Current.Value[i].AudioMute(mute);
                }
            }
        }

        /// <summary>
        /// 特定群組靜音
        /// </summary>
        private void AudioMute(string groupName, bool mute)
        {
            List<AudioContructor> group;
            mAudioNowList.TryGetValue(groupName, out group);

            if (group != null)
            {
                for (int i = group.Count - 1; i >= 0; i--)
                {
                    group[i].AudioMute(mute);
                }
            }

        }


        #endregion

        #region 暫停音樂 

        /// <summary>
        /// 暫停場景上的音樂
        /// </summary>
        public void AudioPause()
        {
            Dictionary<string, List<AudioContructor>>.Enumerator enumerator = mAudioNowList.GetEnumerator();

            while (enumerator.MoveNext())
            {
                for (int i = enumerator.Current.Value.Count - 1; i >= 0; i--)
                {
                    enumerator.Current.Value[i].AudioPause();
                }
            }

            isAllPause = true;
        }

        /// <summary>
        /// 暫停特定群組上的音樂
        /// </summary>
        /// <param name="groupName">名字</param>
        /// <param name="isAll">是否暫停名字</param>
        public void AudioPause(string groupName)
        {
            AudioSetting setting;

            if (mAudioSetting.TryGetValue(groupName, out setting) == true)
            {
                mAudioSetting[groupName].isPause = true;
            }
            else
            {
                mAudioSetting.Add(groupName, new AudioSetting() { isPause = true });
            }

            List<AudioContructor> group;
            mAudioNowList.TryGetValue(groupName, out group);

            if (group != null)
            {
                for (int i = group.Count - 1; i >= 0; i--)
                {
                    group[i].AudioPause();
                }
            }
        }

        /// <summary>
        /// 撥放場景上暫停的音樂
        /// </summary>
        public void AudioUnPause()
        {
            Dictionary<string, List<AudioContructor>>.Enumerator enumerator = mAudioNowList.GetEnumerator();

            while (enumerator.MoveNext())
            {
                for (int i = enumerator.Current.Value.Count - 1; i >= 0; i--)
                {
                    enumerator.Current.Value[i].AudioUnPause();
                }
            }

            isAllPause = false;
        }

        /// <summary>
        /// 撥放特定群組上的音樂
        /// </summary>
        public void AudioUnPause(string groupName)
        {
            AudioSetting setting;

            if (mAudioSetting.TryGetValue(groupName, out setting) == true)
            {
                mAudioSetting[groupName].isPause = false;
            }
            else
            {
                mAudioSetting.Add(groupName, new AudioSetting() { isPause = false });
            }

            List<AudioContructor> group;
            mAudioNowList.TryGetValue(groupName, out group);

            if (group != null)
            {
                for (int i = group.Count - 1; i >= 0; i--)
                {
                    group[i].AudioUnPause();
                }
            }
        }

        #endregion

        #region 停止音樂 

        /// <summary>
        /// 暫停場景上的音樂
        /// </summary>
        public void AudioStop()
        {
            Dictionary<string, List<AudioContructor>>.Enumerator enumerator = mAudioNowList.GetEnumerator();

            while (enumerator.MoveNext())
            {
                for (int i = enumerator.Current.Value.Count - 1; i >= 0; i--)
                {
                    enumerator.Current.Value[i].AudioStop();
                }
            }
        }

        /// <summary>
        /// 暫停特定群組上的音樂
        /// </summary>
        /// <param name="groupName">名字</param>
        /// <param name="isAll">是否暫停名字</param>
        public void AudioStop(string groupName)
        {
            List<AudioContructor> group;
            mAudioNowList.TryGetValue(groupName, out group);

            if (group != null)
            {
                while (group.Count > 0 && group[0] != null)
                {
                    group[0].AudioStop();
                }
            }
        }

        #endregion

        #region 改音量
        /// <summary>
        /// 改場景上的音樂音量
        /// </summary>
        public void AudioVolume(float volume)
        {
            Dictionary<string, List<AudioContructor>>.Enumerator enumerator = mAudioNowList.GetEnumerator();

            while (enumerator.MoveNext())
            {
                for (int i = enumerator.Current.Value.Count - 1; i >= 0; i--)
                {
                    enumerator.Current.Value[i].AudioVolume(volume);
                }
            }
        }

        /// <summary>
        /// 改特定群組上的音樂音量
        /// </summary>
        /// <param name="groupName">名字</param>
        /// <param name="isAll">是否暫停名字</param>
        public void AudioVolume(string groupName, float volume)
        {
            AudioSetting setting;

            if (mAudioSetting.TryGetValue(groupName, out setting) == true)
            {
                mAudioSetting[groupName].volume = volume;
            }
            else
            {
                mAudioSetting.Add(groupName, new AudioSetting() { volume = volume });
            }

            List<AudioContructor> group;
            mAudioNowList.TryGetValue(groupName, out group);

            if (group != null)
            {
                for (int i = group.Count - 1; i >= 0; i--)
                {
                    group[i].AudioVolume(volume);
                }
            }
        }
        #endregion

    }
}