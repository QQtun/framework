using Core.Framework.Res;
using LogUtil;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AddressableAssets;


public class AddressableImporter : AssetPostprocessor
{
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        // rename 會同時觸發 importedAssets 和 movedAssets
        var set = new HashSet<string>();
        if (importedAssets != null)
            set.UnionWith(importedAssets);
        if (movedAssets != null)
            set.UnionWith(movedAssets);
        if (set.Count > 0)
        {
            string[] array = new string[set.Count];
            set.CopyTo(array);
            AddressableFolderSetting.AddAssetsToAddressable(array);
        }

        //if (importedAssets != null && importedAssets.Length > 0)
        //{
        //    Debug.Log("=== start importedAssets ===");
        //    foreach (var path in importedAssets)
        //    {
        //        Debug.Log("Import Asset: " + path);
        //    }
        //}

        //if(deletedAssets != null && deletedAssets.Length > 0)
        //{
        //    Debug.Log("=== start deletedAssets ===");
        //    foreach (string path in deletedAssets)
        //    {
        //        Debug.Log("Deleted Asset: " + path);
        //    }
        //}

        //if (movedAssets != null && movedAssets.Length > 0)
        //{
        //    Debug.Log("=== start movedAssets ===");
        //    for (int i = 0; i < movedAssets.Length; i++)
        //    {
        //        Debug.Log("Moved Asset: " + movedAssets[i] + " from: " + movedFromAssetPaths[i]);
        //    }
        //}
    }
}
