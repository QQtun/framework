using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GupInstanceGrass
{
    public class GrassData : ScriptableObject
    {

        [Serializable]
        public class GrassItem
        {
            public Matrix4x4 TransformMatrix;
            public Color ColorInfo;
            public Vector4 UV0Offset;
            public int TextureIndex;
            public float  WindSpeed;
            public Vector3 TransformPos;
            public Vector4 UVLightMap;
            public int LightMapIndex;
        }

        [Serializable]
        public class GrassBlock
        {
            public List<GrassItem> GrassItems = new List<GrassItem>();
        }

        public int BlockXMax;
        public int BlockYMax;
        public List<int> LightMapIndexs =  new List<int>();
        public List<Texture2D> GrassTextures = new List<Texture2D>();
        public List<GrassBlock> BlockList = new List<GrassBlock>();
    }
}
