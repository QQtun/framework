using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace GupInstanceGrass
{
    public class GrassInstance : MonoBehaviour
    {
        public static Vector2Int BlockSize = new Vector2Int(15, 15);

        public class SubmitInstance
        {
            public Matrix4x4[] Matrixs;
            public MaterialPropertyBlock PropertyBlock = new MaterialPropertyBlock();
        }

        public bool IsDebug = false;

        public Camera RenderCamera;

        [SerializeField]
        List<Material> _GrassMaterialList = new List<Material>();

        [SerializeField]
        Mesh _CurMesh;

        [SerializeField]
        Texture2D _CombineTexture;

        int GrassLevel = 0;

        Vector3 RolePosition = Vector3.zero;
        Quaternion RoleRotation = Quaternion.identity;
        GrassData GrassDataInfo;

        List<Material> _RealUseGrassMaterialList = new List<Material>();

        Material _CurMaterial;

        List<SubmitInstance> _CurSubmitInstances = new List<SubmitInstance>();

        Vector2Int[] _CurShowBlocks;

        //int[] _ShowBlockIndexCalcInts = new[] { -1, 0, 1 };
        List<Matrix4x4> _TempMatrix4x4s = new List<Matrix4x4>();
        List<Vector4> _TempVector4s1 = new List<Vector4>();
        List<Vector4> _TempVector4s2 = new List<Vector4>();
        List<float> _TempWindSpeed = new List<float>();
        Dictionary<string, Vector4> _TextureNameToUV0Offset = new Dictionary<string, Vector4>();

        private bool _ShowBlockDirty = false;
        private bool _SubmitInstanceDirty = false;

        private int _ColorPropID = Shader.PropertyToID("_Color");
        private int _UV0OffsetPropID = Shader.PropertyToID("_UV0Offset");
        private int _MainTexPropID = Shader.PropertyToID("_MainTex");
        private int _RolePositionPropID = Shader.PropertyToID("_RolePosition");
        private int _WindSpeed = Shader.PropertyToID("_Speed");


        private CullingGroup cullingGroup = null;
        //设置遮挡剔除
        void SetCullingGroup()
        {
            if (cullingGroup != null)
            {
                cullingGroup.onStateChanged -= OnStateChange;
                cullingGroup.Dispose();
                cullingGroup = null;
            }


            cullingGroup = new CullingGroup();
            cullingGroup.targetCamera = Camera.main;

            BoundingSphere[] spheres = new BoundingSphere[GrassDataInfo.BlockList.Count];

            for (int i = 0; i < spheres.Length; i++)
            {
                int x = (i) % GrassDataInfo.BlockXMax;
                int y = (i) / GrassDataInfo.BlockXMax;
                spheres[i].position =  new Vector3(BlockSize.x*(x+0.5f), 18.89f, BlockSize.y * (y + 0.5f));
                spheres[i].radius = BlockSize.x * 0.5f;
            }

            cullingGroup.SetBoundingSpheres(spheres);// 将数组的引用提供给CullingGroup

            cullingGroup.SetBoundingSphereCount(spheres.Length);// 确切的告诉CullingGroup，你到底使用了几个包围盒

            cullingGroup.SetDistanceReferencePoint(Camera.main.transform);// 设置参考点

            //cullingGroup.SetBoundingDistances(new float[] { 5f, 10f, 20f });// 设置参考距离 类似LOD的level绑定距离

            // 注册 更新可见性状态时 的回调
            cullingGroup.onStateChanged += OnStateChange;

        }

        void OnStateChange(CullingGroupEvent evt)
        {
            if (evt.hasBecomeVisible)
            {       // 当某个包围球变得可见时
                //Debug.LogFormat("Sphere {0} has become visible!", evt.index);
                _ShowBlockDirty = true;
                UpdateShowBlock();
                return;
            }

            if (evt.hasBecomeInvisible)
            {   // 当某个包围球变得不可见时
                //Debug.LogFormat("Sphere {0} has become invisible!", evt.index);
                _ShowBlockDirty = true;
                UpdateShowBlock();
                return;
            }

            if (evt.currentDistance != evt.previousDistance)
            {   // 当某个包围球进入参考点的某个绑定距离范围
                Debug.LogFormat("Sphere {0} has enter distance band {1}!", evt.index, evt.currentDistance);
                return;
            }
        }

        public void SetGrassLevel(int level)
        {
            GrassLevel = level;
            _CurMaterial = null;
            if (_RealUseGrassMaterialList.Count > GrassLevel)
            {
                _CurMaterial = _RealUseGrassMaterialList[GrassLevel];
            }
        }

        public void SetMapGrassData(GrassData grassData)
        {
            GrassDataInfo = grassData;
            SetTextures(grassData.GrassTextures);
            SetCullingGroup();
            _ShowBlockDirty = true;
            _SubmitInstanceDirty = true;
        }

        public void SetTextures(List<Texture2D> textures)
        {
            var rects = _CombineTexture.PackTextures(textures.ToArray(), 0, 1024, true);
            _TextureNameToUV0Offset.Clear();
            for (int i = 0; i < textures.Count; i++)
            {
                _TextureNameToUV0Offset.Add(textures[i].name, new Vector4(rects[i].min.x, rects[i].min.y, rects[i].max.x, rects[i].max.y));
            }

            foreach (var material in _RealUseGrassMaterialList)
            {
                material.SetTexture(_MainTexPropID, _CombineTexture);
            }
        }

        public void SetRolePosition(Transform tran)
        {
            float x, y, z;
            x = tran.position.x;
            y = tran.position.y;
            z = tran.position.z;

            if (Math.Abs(RolePosition.x - x) > 0.001f || Math.Abs(RolePosition.y - y) > 0.001f || Math.Abs(RolePosition.z - z) > 0.001f
                || Math.Abs(Quaternion.Angle(RoleRotation, tran.rotation)) > 0.5f)
            {
                RolePosition.x = x;
                RolePosition.y = y;
                RolePosition.z = z;
                RoleRotation = tran.rotation;
                Shader.SetGlobalVector(_RolePositionPropID, RolePosition);
                //_ShowBlockDirty = true;
            }
        }

        public void Clear()
        {
            SetMapGrassData(null);
        }

        void Awake()
        {
            foreach (var material in _GrassMaterialList)
            {
                _RealUseGrassMaterialList.Add(Instantiate(material));
            }
            //for (int i = 0; i < _CurShowBlocks.Length; i++)
            //{
            //    _CurShowBlocks[i] = new Vector2Int(-1, -1);
            //}
            _CombineTexture = new Texture2D(0, 0, TextureFormat.RGBA32, false, false);
            SetGrassLevel(0);
        }

        // Update is called once per frame
        void Update()
        {
            //UpdateShowBlock();
            UpdateSubmitInstance();
            UpdateGrassDraw();
        }

        bool  CompareArr(int[] arr1, int[] arr2)
        {
            var q = from a in arr1 join b in arr2 on a equals b select a;

            bool flag = arr1.Length == arr2.Length && q.Count() == arr1.Length;

            return flag;//内容相同返回true,反之返回false。
        }

        private int[] LastResultIndices;
        void UpdateShowBlock()
        {
            if (!_ShowBlockDirty)
            {
                return;
            }
            _ShowBlockDirty = false;

            // 返回满足条件的包围球的index
            int[] resultIndices = new int[GrassDataInfo.BlockList.Count];

            // 查询所有可见的包围球
            int numResults = cullingGroup.QueryIndices(true, resultIndices, 0);

            if ( numResults>0 &&  LastResultIndices != null && (LastResultIndices.Length == resultIndices.Length) && CompareArr(LastResultIndices, resultIndices))
            {
                return;
            }
            LastResultIndices = resultIndices;

            _CurShowBlocks = null;
            _CurShowBlocks = new Vector2Int[numResults];
            for (int i = 0; i < numResults; i++)
            {
                int x = (resultIndices[i]) % GrassDataInfo.BlockXMax;
                int y = (resultIndices[i]) / GrassDataInfo.BlockXMax;

                _CurShowBlocks[i] = new Vector2Int(x, y);
            }

            _SubmitInstanceDirty = true;

            //int blockX4, blockY4;
            //PosToBlock(RolePosition.x, RolePosition.z, out blockX4, out blockY4);
            //if (_CurShowBlocks[4].x == blockX4 && _CurShowBlocks[4].y == blockY4)
            //{
            //    return;
            //}
            //for (int j = 0; j < 3; j++)
            //{
            //    for (int i = 0; i < 3; i++)
            //    {
            //        int index = j * 3 + i;
            //        var oldX = _CurShowBlocks[index].x;
            //        var oldY = _CurShowBlocks[index].y;
            //        var newX = blockX4 + _ShowBlockIndexCalcInts[i];
            //        var newY = blockY4 + _ShowBlockIndexCalcInts[j];
            //        if (oldX != newX || oldY != newY)
            //        {
            //            _CurShowBlocks[index].x = newX;
            //            _CurShowBlocks[index].y = newY;
            //            _SubmitInstanceDirty = true;
            //        }
            //    }
            //}
        }

        void UpdateSubmitInstance()
        {
            if (!_SubmitInstanceDirty)
            {
                return;
            }
            _SubmitInstanceDirty = false;

            SubmitInstance curSubmitInstance = null;
            _TempMatrix4x4s.Clear();
            _TempVector4s1.Clear();
            _TempVector4s2.Clear();
            _TempWindSpeed.Clear();
            _CurSubmitInstances.Clear();
            if (GrassDataInfo != null && _CurShowBlocks !=null)
            {
                foreach (var curShowBlock in _CurShowBlocks)
                {
                    if (curShowBlock.x < 0 || curShowBlock.y < 0)
                    {
                        continue;
                    }

                    var index = BlockToIndex(curShowBlock.x, curShowBlock.y);
                    if (index >= GrassDataInfo.BlockList.Count)
                    {
                        continue;
                    }

                    var grassBlock = GrassDataInfo.BlockList[index];
                    foreach (var grassItem in grassBlock.GrassItems)
                    {
                        if (curSubmitInstance == null)
                        {
                            curSubmitInstance = new SubmitInstance();
                            _CurSubmitInstances.Add(curSubmitInstance);
                        }

                        //Vector3 pos = grassItem.TransformPos;
                        //if (!IsAPointInACamera(Camera.main, pos))
                        //    continue;

                        _TempMatrix4x4s.Add(grassItem.TransformMatrix);
                        _TempVector4s1.Add(grassItem.ColorInfo);
                        //_TempVector4s2.Add(_TextureNameToUV0Offset[grassItem.TextureName]);
                        _TempWindSpeed.Add(grassItem.WindSpeed);
                        if (_TempMatrix4x4s.Count >= 1023)
                        {
                            curSubmitInstance.Matrixs = _TempMatrix4x4s.ToArray();
                            curSubmitInstance.PropertyBlock.SetVectorArray(_ColorPropID, _TempVector4s1);
                            curSubmitInstance.PropertyBlock.SetVectorArray(_UV0OffsetPropID, _TempVector4s2);
                            if (GrassLevel > 0)
                            {
                                curSubmitInstance.PropertyBlock.SetFloatArray(_WindSpeed, _TempWindSpeed);
                                
                            }

                            _TempMatrix4x4s.Clear();
                            _TempVector4s1.Clear();
                            _TempVector4s2.Clear();
                            _TempWindSpeed.Clear();
                            curSubmitInstance = null;
                        }
                    }
                }
                if (curSubmitInstance != null)
                {
                    curSubmitInstance.Matrixs = _TempMatrix4x4s.ToArray();
                    curSubmitInstance.PropertyBlock.SetVectorArray(_ColorPropID, _TempVector4s1);
                    curSubmitInstance.PropertyBlock.SetVectorArray(_UV0OffsetPropID, _TempVector4s2);
                    if (GrassLevel > 0)
                    {
                        curSubmitInstance.PropertyBlock.SetFloatArray(_WindSpeed, _TempWindSpeed);
                    }
                    //if (GrassLevel > 0)
                    //{
                    //    curSubmitInstance.PropertyBlock.SetFloat(_WindSpeed, grassBlock.WindSpeed);

                    //}

                    _TempMatrix4x4s.Clear();
                    _TempVector4s1.Clear();
                    _TempVector4s2.Clear();
                    _TempWindSpeed.Clear();
                    curSubmitInstance = null;
                }
            }
        }

        void UpdateGrassDraw()
        {
            if (!_CurMaterial)
            {
                return;
            }
            if (!_CurMesh)
            {
                return;
            }
            if (!RenderCamera)
            {
                return;
            }
            foreach (var curSubmitInstance in _CurSubmitInstances)
            {
                if (IsDebug)
                {
                    Graphics.DrawMeshInstanced(
                        _CurMesh,
                        0,
                        _CurMaterial,
                        curSubmitInstance.Matrixs,
                        curSubmitInstance.Matrixs.Length,
                        curSubmitInstance.PropertyBlock,
                        ShadowCastingMode.Off,
                        false,
                        0,
                        null);
                }
                else
                {
                    Graphics.DrawMeshInstanced(
                        _CurMesh,
                        0,
                        _CurMaterial,
                        curSubmitInstance.Matrixs,
                        curSubmitInstance.Matrixs.Length,
                        curSubmitInstance.PropertyBlock,
                        ShadowCastingMode.Off,
                        false,
                        0,
                        RenderCamera);
                }
            }
        }

        private void PosToBlock(float x, float z, out int blockX, out int blockY)
        {
            blockX = Mathf.FloorToInt(1.0f * x / BlockSize.x);
            blockY = Mathf.FloorToInt(1.0f * z / BlockSize.y);
        }

        private int BlockToIndex(int blockX, int blockY)
        {
            int index = 0;
            if (GrassDataInfo != null)
            {
                index = blockY * GrassDataInfo.BlockXMax + blockX;
            }

            return index;
        }

        private int PosToBlockIndex(float x, float z)
        {
            int blockX, blockY;
            PosToBlock(x, z, out blockX, out blockY);
            return BlockToIndex(blockX, blockY);
        }


        bool IsAPointInACamera(Camera cam, Vector3 wordPos)
        {
            // 是否在视野内
            bool result1 = false;
            Vector3 posViewport = cam.WorldToViewportPoint(wordPos);
            //Debug.LogError(posViewport.z);
            // 视野在10米内全部渲染（不管是否在摄像机内）
            if (posViewport.z >= cam.nearClipPlane && posViewport.z <= cam.nearClipPlane + 1)
            {
                return true;
            }


            Rect rect = new Rect(0, 0, 1, 1);
            result1 = rect.Contains(posViewport);
            // 是否在远近平面内
            bool result2 = false;
            if (posViewport.z >= cam.nearClipPlane && posViewport.z <= cam.farClipPlane)
            {
                result2 = true;
            }

            // 综合判断
            bool result = result1 && result2;

            if (result)
            {
                if (posViewport.z >= cam.nearClipPlane + 15)
                {
                    float t1 = cam.farClipPlane - cam.nearClipPlane - 15;
                    float t2 = posViewport.z - cam.nearClipPlane - 15;

                    float t3 = 1 - t2 / t1;

                    var random = UnityEngine.Random.Range(0, 10000);
                    if (10000 * t3 < random)
                        return false;
                }
            }


            return result;
        }

        private void OnDestroy()
        {
           if (cullingGroup != null)
           {
                cullingGroup.onStateChanged -= OnStateChange;
                cullingGroup.Dispose();
               cullingGroup = null;
           }
        }
    }

}
