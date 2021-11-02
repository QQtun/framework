using UnityEditor;
using UnityEngine;

public class LuaAssetImporter : AssetPostprocessor
{
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        var collection = AssetDatabase.LoadAssetAtPath<TextAssetCollection>("Assets/Scripts/LuaScripts/AllLuaScripts.asset");
        bool isDirtry = false;
        var luaScriptsFolder = "Assets/Scripts/LuaScripts";
        var luaScriptExtention = ".lua.txt";
        foreach (var path in importedAssets)
        {
            if(path.StartsWith(luaScriptsFolder) && path.EndsWith(luaScriptExtention))
            {
                var relativePathAndName = path.Substring(luaScriptsFolder.Length + 1, path.Length - luaScriptsFolder.Length - 1 - luaScriptExtention.Length);
                if (collection.GetAsset(relativePathAndName) == null)
                {
                    var row = new TextAssetCollection.TextAssetRow();
                    row.asset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                    row.name = relativePathAndName;
                    collection.assets.Add(row);
                    isDirtry = true;
                }
            }
        }
        if(isDirtry)
        {
            EditorUtility.SetDirty(collection);
            AssetDatabase.SaveAssets();
        }
    }
}
