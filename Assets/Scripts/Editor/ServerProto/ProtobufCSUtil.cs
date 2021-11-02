using UnityEditor;
using UnityEngine;

public class CopyProtobufCS
{
    [MenuItem("程式工具/複製ServerProtoCS進專案")]
    public static void CopyServerSC()
    {
        ServerProtoAsset asset = null;
        var assets = AssetDatabase.FindAssets("t:ServerProtoAsset");
        for (int i = 0; i < assets.Length; i++)
        {
            var guid = assets[i];
            var path = AssetDatabase.GUIDToAssetPath(guid);
            asset = AssetDatabase.LoadAssetAtPath<ServerProtoAsset>(path);
        }

        if (asset == null)
        {
            Debug.LogError("請先建立ServerProtoAsset");
            return;
        }

        if (string.IsNullOrEmpty(asset.csSourcFrom)
            || string.IsNullOrEmpty(asset.csCopyTo)
            || string.IsNullOrEmpty(asset.pbSourceFrom)
            || string.IsNullOrEmpty(asset.pbCopyTo))
        {
            Debug.LogError("請先設定好ServerProtoAsset");
            return;
        }

        Copy(asset.csSourcFrom, asset.csCopyTo);
        Copy(asset.pbSourceFrom, asset.pbCopyTo, ".txt");

        AssetDatabase.Refresh();
    }

    private static void Copy(string fromFolder, string toFolder, string newExtention = null)
    {
        var filePaths = System.IO.Directory.GetFiles(fromFolder);
        for (int i = 0; i < filePaths.Length; i++)
        {
            var from = filePaths[i];
            var fileName = System.IO.Path.GetFileName(from);
            var coptyTo = toFolder.EndsWith("/") ? toFolder : toFolder + "/";
            var to = coptyTo + fileName;
            if (!string.IsNullOrEmpty(newExtention))
                to += newExtention;
            System.IO.File.Copy(from, to, true);
        }
    }
}
