using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Core.Framework.Utility
{
    [Serializable]
    public class ObjectReference
    {
        [FormerlySerializedAs("Name")]
        public string name;
        [FormerlySerializedAs("Obj")]
        public GameObject obj;
    }
}