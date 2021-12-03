using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets;
#endif

namespace Core.Framework.Res
{
    [CreateAssetMenu(fileName = AddressableFolderSetting.FileName, menuName = "Asset Config/AddressableFolderSetting")]
    public class AddressableFolderSetting : ScriptableObject
    {
        public const string FileName = "AddressableFolderSetting.asset";
        public const string AssetPath = "Assets/PublicAssets/Configs/" + FileName;

        [Serializable]
        public class Filter
        {
            public enum Compare
            {
                Contain,
                NotContain,
                StartWith,
                EndWith,
            }

            public enum Operation
            {
                Include,
                Exclude,
            }

            public string key;
            public Compare compare;
            public Operation operation;

            public bool Check(string fileName)
            {
                if (string.IsNullOrEmpty(key))
                    return true;

                bool compareResult = false;
                switch (compare)
                {
                    case Compare.Contain:
                        compareResult = fileName.Contains(key);
                        break;
                    case Compare.NotContain:
                        compareResult = !fileName.Contains(key);
                        break;
                    case Compare.StartWith:
                        compareResult = fileName.StartsWith(key);
                        break;
                    case Compare.EndWith:
                        compareResult = fileName.EndsWith(key);
                        break;
                }

                return (compareResult && operation == Operation.Include) || ((!compareResult) && operation == Operation.Exclude);
            }
        }

        [Serializable]
        public class FolderSetting
        {
            public enum AddLabelRule
            {
                None,
                Directly,
                ForderName,
                SubFolderName,
            }

            public string path;
            public string groupName;
            [EnumToggleButtons]
            public AddLabelRule addLabelRule;
            [EnableIf("addLabelRule", AddLabelRule.Directly)]
            public string label;
            public bool includeSubFolder;
            [Tooltip("檔名必須可以轉成int，如果檔名包含\"_\"則前段必須可以轉成int")]
            public bool intAddress;
            [Tooltip("只允許以下附檔名加入Addressable，如果未填則一律加入")]
            public List<string> extentions;
            [Tooltip("針對檔名做過濾，條件皆通過才加入Addressable，如果未填則一律加入")]
            public List<Filter> filters = new List<Filter>();

            public bool CheckExtention(string extention)
            {
                if (extentions == null || extentions.Count == 0)
                    return true;

                var index = extentions.FindIndex(ex => ex.Trim() == extention);
                return index >= 0;
            }

            public bool Filter(string fileName)
            {
                if (filters == null || filters.Count == 0)
                    return true;

                foreach (var f in filters)
                {
                    if (!f.Check(fileName))
                        return false;
                }
                return true;
            }
        }

        public bool enable = true;
        public List<FolderSetting> folders = new List<FolderSetting>();

#if UNITY_EDITOR
        [MenuItem("程式工具/Addressable ReimportAll")]
        public static void ReimportAllByDefaultAsset()
        {
            var settings = AssetDatabase.LoadAssetAtPath<AddressableFolderSetting>(AssetPath);
            ReimportAll(settings);
        }

        public static void ReimportAll(AddressableFolderSetting settings)
        {
            foreach (var setting in settings.folders)
            {
                var allFilePaths = System.IO.Directory.GetFiles(setting.path, "*.*", SearchOption.AllDirectories);
                AddAssetsToAddressable(allFilePaths);
            }
        }

        [Button("ReimportAll")]
        public void ReimportAll()
        {
            ReimportAll(this);
        }

        public static void AddAssetsToAddressable(string[] assetPaths)
        {
            var folderSettingAsset = AssetDatabase.LoadAssetAtPath<AddressableFolderSetting>(AssetPath);
            if (!folderSettingAsset.enable)
                return;

            bool setDirtySetting = false;
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
                return;

            foreach (string path in assetPaths)
            {
                if (string.IsNullOrEmpty(path))
                    continue;
                if (path.EndsWith(".meta"))
                    continue;
                if (Directory.Exists(path))
                    continue;

                //Debug.Log("AddressableImporter Reimported Asset: " + str);                
                var guid = AssetDatabase.AssetPathToGUID(path);
                if (string.IsNullOrEmpty(guid))
                    continue;

                var fileName = Path.GetFileName(path);
                var fileNameWithoutEx = Path.GetFileNameWithoutExtension(path);
                var extention = Path.GetExtension(path);
                extention = extention.StartsWith(".") ? extention.Substring(1) : extention;
                var assetForderPath = path.Substring(0, path.Length - fileName.Length - 1); // -1 for '/'

                var index = folderSettingAsset.folders.FindIndex(f => f.includeSubFolder ? path.StartsWith(f.path) : f.path == assetForderPath);
                if (index >= 0)
                {
                    var folderSetting = folderSettingAsset.folders[index];
                    if (!folderSetting.CheckExtention(extention))
                        continue;
                    if (!folderSetting.Filter(fileNameWithoutEx))
                        continue;

                    var group = settings.FindGroup(folderSetting.groupName);
                    if (group == null)
                    {
                        group = settings.CreateGroup(folderSetting.groupName, false, false, true, null);
                        setDirtySetting = true;
                    }

                    if (group.GetAssetEntry(guid) != null)
                        continue;

                    var address = fileNameWithoutEx;
                    if (folderSetting.intAddress)
                    {
                        // 檔名 或是 _之前的部分 要能轉int
                        if (address.Contains("_"))
                            address = address.Substring(0, address.IndexOf("_"));
                        if (!int.TryParse(address, out var _))
                            continue;
                    }

                    var entry = settings.CreateOrMoveEntry(guid, group, false, false);
                    entry.address = address;

                    AddLebal(settings, folderSetting, entry, path);

                    EditorUtility.SetDirty(group);
                    setDirtySetting = true;
                }
            }
            if (setDirtySetting)
            {
                EditorUtility.SetDirty(settings);
                AssetDatabase.SaveAssets();
            }
        }

        private static void AddLebal(
            UnityEditor.AddressableAssets.Settings.AddressableAssetSettings settings,
            FolderSetting folderSetting, UnityEditor.AddressableAssets.Settings.AddressableAssetEntry entry, string path)
        {
            if (folderSetting.addLabelRule == FolderSetting.AddLabelRule.Directly)
            {
                if (!string.IsNullOrEmpty(folderSetting.label))
                {
                    var existLabels = settings.GetLabels();
                    if (!existLabels.Contains(folderSetting.label))
                    {
                        settings.AddLabel(folderSetting.label);
                    }
                    entry.labels.Add(folderSetting.label);
                }
            }
            else if (folderSetting.addLabelRule == FolderSetting.AddLabelRule.ForderName)
            {
                var folderPath = Path.GetDirectoryName(path);
                var folderName = Path.GetFileNameWithoutExtension(folderPath);

                var existLabels = settings.GetLabels();
                if (!existLabels.Contains(folderName))
                {
                    settings.AddLabel(folderName);
                }
                entry.labels.Add(folderName);
            }
            else if (folderSetting.addLabelRule == FolderSetting.AddLabelRule.SubFolderName)
            {
                var folderName = Path.GetFileNameWithoutExtension(folderSetting.path);
                var subFolderPath = Path.GetDirectoryName(path);
                var subFolderName = Path.GetFileNameWithoutExtension(subFolderPath);
                if (path.StartsWith(folderSetting.path) && folderName != subFolderName)
                {
                    var existLabels = settings.GetLabels();
                    if (!existLabels.Contains(subFolderName))
                    {
                        settings.AddLabel(subFolderName);
                    }
                    entry.labels.Add(subFolderName);
                }
            }
        }
#endif
    }
}