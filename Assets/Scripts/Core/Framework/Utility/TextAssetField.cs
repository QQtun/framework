using System;
using UnityEngine.Serialization;

namespace Core.Framework.Utility
{
    [Serializable]
    public class TextAssetField
    {
        [FormerlySerializedAs("Path")]
        public string path;
        [FormerlySerializedAs("Guid")]
        public string guid;
    }
}