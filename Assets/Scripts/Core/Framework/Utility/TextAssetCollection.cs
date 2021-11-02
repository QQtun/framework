using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "TextAssetCollection", menuName = "Collection/Create TextAsset Collection")]
public class TextAssetCollection : ScriptableObject
{
    [Serializable]
    public class TextAssetRow
    {
        [FormerlySerializedAs("name")]
        public string name;
        [FormerlySerializedAs("Asset")]
        public TextAsset asset;
    }

    [FormerlySerializedAs("Assets")]
    public List<TextAssetRow> assets = new List<TextAssetRow>();

    public TextAsset GetAsset(string path)
    {
        for (int i = 0; i < assets.Count; i++)
        {
            if (assets[i].name == path)
                return assets[i].asset;
        }
        return null;
    }

    public TextAsset GetAssetByFileName(string name)
    {
        for (int i = 0; i < assets.Count; i++)
        {
            if (assets[i].asset.name == name)
                return assets[i].asset;
        }
        return null;
    }

    public TextAsset GetAssetByIndex(int index)
    {
        if (assets.Count > index)
            return assets[index].asset;
        return null;
    }
}
