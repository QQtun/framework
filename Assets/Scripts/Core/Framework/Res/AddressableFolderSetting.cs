using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets;
#endif

namespace Core.Framework.Res
{
    [CreateAssetMenu(fileName = AddressableFolderSetting.FileName, menuName = "Config/AddressableFolderSetting")]
    public class AddressableFolderSetting : ScriptableObject
    {
        public const string FileName = "AddressableFolderSetting.asset";
        public const string Path = "Assets/PublicAssets/Configs/" + FileName;

        [Serializable]
        public class FolderSetting
        {
            public string path;
            public string groupName;
            public bool includeSubFolder;
            public string extention;
            public List<string> keys = new List<string>();
        }

        public List<FolderSetting> folders = new List<FolderSetting>();

#if UNITY_EDITOR
        [MenuItem("程式工具/Addressable ReimportAll")]
        public static void ReimportAllByDefaultAsset()
        {
            var settings = AssetDatabase.LoadAssetAtPath<AddressableFolderSetting>(Path);
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

        [ContextMenu("ReimportAll")]
        public void ReimportAll()
        {
            ReimportAll(this);
        }

        public static void AddAssetsToAddressable(string[] assetPaths)
        {
            var folderSettingAsset = AssetDatabase.LoadAssetAtPath<AddressableFolderSetting>(AddressableFolderSetting.Path);

            bool setDirtySetting = false;
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
                return;
            foreach (string str in assetPaths)
            {
                if (string.IsNullOrEmpty(str))
                    continue;
                if (str.EndsWith(".meta"))
                    continue;
                if (Directory.Exists(str))
                    continue;

                //Debug.Log("AddressableImporter Reimported Asset: " + str);                
                var guid = AssetDatabase.GUIDFromAssetPath(str);
                if (guid.Empty())
                    continue;
                var fileName = System.IO.Path.GetFileName(str);
                var extention = System.IO.Path.GetExtension(str);
                extention = extention.StartsWith(".") ? extention.Substring(1) : extention;
                var assetForderPath = str.Substring(0, str.Length - fileName.Length - 1); // -1 for '/'

                var index = folderSettingAsset.folders.FindIndex(f => f.includeSubFolder ? str.StartsWith(f.path) : f.path == assetForderPath);
                if (index >= 0)
                {
                    var folderSetting = folderSettingAsset.folders[index];
                    if(!string.IsNullOrEmpty(folderSetting.extention)
                        && !folderSetting.extention.Contains(extention))
                        continue;

                    if(folderSetting.keys.Count > 0)
                    {
                        bool pass = false;
                        foreach(var key in folderSetting.keys)
                        {
                            pass = fileName.Contains(key);
                            if (pass)
                                break;
                        }
                        if (!pass)
                            continue;
                    }

                    var group = settings.FindGroup(folderSetting.groupName);
                    if (group == null)
                        group = settings.CreateGroup(folderSetting.groupName, false, false, true, null);

                    if (group.GetAssetEntry(guid.ToString()) != null)
                        continue;

                    var entry = settings.CreateOrMoveEntry(guid.ToString(), group, false, false);
                    entry.address = AssetDatabase.GUIDToAssetPath(guid);

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
#endif
    }
}