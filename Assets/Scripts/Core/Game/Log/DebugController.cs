using System;
using System.Collections.Generic;
using UnityEngine;
using LogUtil;

namespace Core.Game.Log
{
    //[DebugInfoClass("DebugController", typeof(DebugControllerPanel))]
    public class DebugController : MonoBehaviour
    {
        [Serializable]
        public class LogSetting
        {
            public string tagName;
            public bool tagColor;
            public VisiblePack visible;

            [SerializeField]
            public Color32 color;
        }

        [SerializeField]
        private List<LogSetting> mList = new List<LogSetting>();

        [SerializeField]
        private VisiblePack mIsShowLog = VisiblePack.All;

        [SerializeField]
        private bool mIsShowTime = false;

        [SerializeField]
        private bool mIsShowTagString = true;

        public bool IsShowTagString
        {
            get => mIsShowTagString;
            set
            {
                mIsShowTagString = value;
            }
        }

        public bool IsShowTime
        {
            get => mIsShowTime;
            set
            {
                mIsShowTime = value;
            }
        }

        public VisiblePack IsShowLog => mIsShowLog;

        public List<LogSetting> TagList { get { return mList; } }

        public void SetVisibility()
        {
            if (mList == null || mList.Count == 0)
            {
                return;
            }

            Dictionary<string, VisiblePack> mapping = new Dictionary<string, VisiblePack>();
            foreach (LogSetting setting in mList)
            {
                mapping.Add(setting.tagName, setting.visible);
            }
            LogUtil.Debug.SetTagVisible(mapping);
        }

        public void ShowLogChange()
        {
            LogUtil.Debug.SetAllLogVisible(mIsShowLog);
        }

        public void SetShowLogs(VisiblePack show)
        {
            if (!show.Info)
                LogUtil.Debug.LogFormat("Show Log OFF", LogTag.System);

            mIsShowLog = show;
            ShowLogChange();

            if (show.Info)
                LogUtil.Debug.LogFormat("Show Log ON", LogTag.System);
        }

        public void SetTagColor()
        {
            if (mList == null || mList.Count == 0)
            {
                return;
            }

            Dictionary<string, LogColor> tagColors = new Dictionary<string, LogColor>();
            foreach (LogSetting setting in mList)
            {
                if (setting.tagColor)
                {
                    tagColors.Add(setting.tagName, new LogColor()
                    {
                        R = setting.color.r,
                        G = setting.color.g,
                        B = setting.color.b,
                        A = setting.color.a
                    });
                }
            }

            if (tagColors.Count > 0)
            {
                LogUtil.Debug.SetTagColor(tagColors);
            }
        }

        public void SetShowTime()
        {
            LogUtil.Debug.SetTimeVisible(mIsShowTime);
        }

        public void SetShowTagString()
        {
            LogUtil.Debug.SetTagStringVisible(mIsShowTagString);
        }

        private void OnEnable()
        {
            ShowLogChange();
            SetVisibility();
            SetTagColor();
            SetShowTime();
            SetShowTagString();

            //if(DebugUtility.ui != null)
            //    DebugUtility.AddCustomInfo(this);
            //else
            //    DebugUtility.OnUILoaded += OnDebugUILoaded;
        }

        private void OnDebugUILoaded()
        {
            //DebugUtility.AddCustomInfo(this);
        }
    }
}