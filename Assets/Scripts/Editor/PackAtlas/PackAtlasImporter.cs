using System.Collections.Generic;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

public class PackAtlasImporter : AssetPostprocessor
{
    public const string PackPath = "Assets/Arts/UI/Images";
    public const string DonotPachFolder = "DontPack";
    public const string AtlasPath = "Assets/PublicAssets/UI/Atlas";


    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        //Debug.Log("importedAssets.Length=" + importedAssets.Length);
        List<Sprite> sprites = new List<Sprite>();
        foreach(var path in importedAssets)
        {

            var sp = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sp != null)
                sprites.Add(sp);
        }

        //Debug.Log("sprites.Count=" + sprites.Count);
        if (sprites.Count == 0)
            return;

        Dictionary<string, List<Sprite>> folderToSpriteDic = new Dictionary<string, List<Sprite>>();

        foreach (var sp in sprites)
        {
            var path = AssetDatabase.GetAssetPath(sp);

            if (!path.StartsWith(PackPath))
                continue;

            var folderPath = System.IO.Path.GetDirectoryName(path);
            folderPath = folderPath.Replace("\\", "/");
            folderPath = folderPath.Substring(PackPath.Length + 1);

            bool isSubFolder = folderPath.IndexOf("/") > 0;
            var rootFolderName = folderPath;
            if (isSubFolder)
                rootFolderName = rootFolderName.Substring(0, rootFolderName.IndexOf("/"));
            var realFolderName = folderPath.Substring(folderPath.LastIndexOf("/") + 1);

            //Debug.Log($"rootFolderName={rootFolderName}  realFolderName={realFolderName}");

            if (realFolderName == DonotPachFolder)
                continue;

            if (!folderToSpriteDic.TryGetValue(rootFolderName, out var list))
            {
                list = new List<Sprite>();
                folderToSpriteDic[rootFolderName] = list;
            }
            list.Add(sp);
        }

        foreach (var kv in folderToSpriteDic)
        {
            var atlasPath = AtlasPath + $"/{kv.Key}Atlas.spriteatlas";
            var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasPath);
            if (atlas == null)
            {
                atlas = new SpriteAtlas();
                if (!System.IO.Directory.Exists(AtlasPath))
                {
                    System.IO.Directory.CreateDirectory(AtlasPath);
                    AssetDatabase.Refresh();
                }
                AssetDatabase.CreateAsset(atlas, atlasPath);
            }

            List<Object> packables = new List<Object>(atlas.GetPackables());
            var spList = kv.Value;
            List<Object> addList = new List<Object>();
            foreach (var sp in spList)
            {
                if (!packables.Contains(sp))
                {
                    addList.Add(sp);
                }
            }
            if (addList.Count > 0)
            {
                atlas.Add(addList.ToArray());
                EditorUtility.SetDirty(atlas);
            }
        }
        AssetDatabase.SaveAssets();
    }
}