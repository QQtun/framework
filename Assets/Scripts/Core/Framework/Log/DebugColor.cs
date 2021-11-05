using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RiverGames
{
    [CreateAssetMenu(fileName = "DebugColor", menuName = "DebugColor")]
    public class DebugColor : ScriptableObject
    {
        [Flags]
        public enum Severity
        {
            Info = 1 << 1,
            Warning = 1 << 2,
            Error = 1 << 3,
            All = Info | Warning | Error,
        }

        [Serializable]
        public class TagColor
        {
            [OnValueChanged("OnValueChange")]
            public string tagName;

            [OnValueChanged("OnValueChange")]
            public Color color;

            [EnumToggleButtons]
            [OnValueChanged("OnValueChange")]
            public Severity serity = Severity.All;

            public bool Info => (serity & Severity.Info) == Severity.Info;
            public bool Warning => (serity & Severity.Warning) == Severity.Warning;
            public bool Error => (serity & Severity.Error) == Severity.Error;

            private void OnValueChange()
            {
#if UNITY_EDITOR
                var guids = AssetDatabase.FindAssets("t:DebugColor");
                foreach (var guid in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var assset = AssetDatabase.LoadAssetAtPath<DebugColor>(path);
                    if (assset != null)
                    {
                        assset.SetToDebugLog();
                        break;
                    }
                }
#endif
            }
        }

        [OnValueChanged("OnValueChange")]
        public bool time = false;

        [OnValueChanged("OnValueChange")]
        public bool tag = true;

        [EnumToggleButtons]
        [OnValueChanged("OnValueChange")]
        public Severity allSevrity = Severity.All;

        [OnValueChanged("OnValueChange")]
        public List<TagColor> tagColors = new List<TagColor>();

        public Dictionary<string, LogUtil.LogColor> GetColorDic()
        {
            var ret = new Dictionary<string, LogUtil.LogColor>();
            foreach (var tagColor in tagColors)
            {
                var newTagColor = new LogUtil.LogColor();
                if (string.IsNullOrEmpty(tagColor.tagName))
                    continue;
                newTagColor.R = (short)Mathf.RoundToInt(tagColor.color.r * 255);
                newTagColor.G = (short)Mathf.RoundToInt(tagColor.color.g * 255);
                newTagColor.B = (short)Mathf.RoundToInt(tagColor.color.b * 255);
                newTagColor.A = (short)Mathf.RoundToInt(tagColor.color.a * 255);
                ret.Add(tagColor.tagName, newTagColor);
            }
            return ret;
        }

        public Dictionary<string, LogUtil.VisiblePack> GetVisibleDic()
        {
            var ret = new Dictionary<string, LogUtil.VisiblePack>();
            foreach (var v in tagColors)
            {
                var visible = new LogUtil.VisiblePack();
                if (string.IsNullOrEmpty(v.tagName))
                    continue;
                visible.Info = (allSevrity & Severity.Info) == Severity.Info && v.Info;
                visible.Warning = (allSevrity & Severity.Warning) == Severity.Warning && v.Warning;
                visible.Error = (allSevrity & Severity.Error) == Severity.Error && v.Error;
                ret.Add(v.tagName, visible);
            }
            return ret;
        }

        public void SetToDebugLog()
        {
            LogUtil.Debug.SetTagVisible(GetVisibleDic());
            LogUtil.Debug.SetTagStringVisible(tag);
            LogUtil.Debug.SetTimeVisible(time);
            LogUtil.Debug.SetTagColor(GetColorDic());
        }

        private void OnValueChange()
        {
            SetToDebugLog();
        }
    }

#if UNITY_EDITOR
    [InitializeOnLoad]
    public class SetUpDebugColor
    {
        static SetUpDebugColor()
        {
            var guids = AssetDatabase.FindAssets("t:DebugColor");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var assset = AssetDatabase.LoadAssetAtPath<DebugColor>(path);
                if (assset != null)
                {
                    assset.SetToDebugLog();
                    break;
                }
            }
        }
    }
#endif
}