using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

/// <summary>
/// 渲染影子
/// 
/// 運作原理:
/// 1.StartWork為渲染出影子
/// 2.StopWork為釋放資源
/// 3.透過LateUpdate方式 讓攝影機跟著角色
/// 
/// 注意事項:
/// 1.渲染角色的攝影機 Layer層要跟角色一致
/// 2.渲染角色需要有 Layer層(有SkinnedMeshRenderer那個物件)
/// 3.Projector要自行設定排除Layer層 (包含角色所在層，不然影子會貼在角色上)
/// 
/// 使用方法:
/// 1.透過其他腳本(有個測試腳本TestShadow)呼叫 StartShadow
/// 2.設定 渲染角色的攝影機 的Layer層
/// 3.設定 渲染角色Layer層
/// 4.Projector 設定排除Layer層
/// 5.關閉Directional Light 的 Shadow
/// </summary>

namespace Core.Framework.Camera.Shadow
{
    public class ShadowProjector : MonoBehaviour
    {
        //渲染角色
        [SerializeField]
        private GameObject mLeader;

        //投影組件
        [SerializeField]
        private Projector mProjector;

        //渲染角色的攝影機
        [SerializeField]
        private UnityEngine.Camera mLightCamera = null;

        //渲染角色的RenderTexture
        [SerializeField]
        private RenderTexture mShadowTex = null;

        //需要渲染的Skin(例:角色、武器.....)
        [SerializeField]
        private SkinnedMeshRenderer[] mShadowCasterList;

        //投影盒參數
        [SerializeField]
        private float mBoundsOffset = 1;

        //渲染用Shader 負責將攝影機圖像渲染成黑白圖
        [SerializeField]
        private Shader mShadowReplaceShader;

        //投影盒 存放點的List 
        [SerializeField]
        private List<Vector3> mVec3List = new List<Vector3>();

        //渲染攝影機大小
        [SerializeField]
        private float mCurSizeScale = 0.7f;

        //RenderTexture大小
        [SerializeField]
        private float mRenderTextureScale = 0.4f;

        //渲染參數
        [SerializeField]
        private CommandBuffer mCmdBuffer = null;


        /// <summary>
        /// 啟用影子
        /// </summary>
        /// <param name="leader">要渲染影子的物件</param>
        /// <param name="leaderMask">要渲染影子的物件的layer層</param>
        public void StartShadow(GameObject leader, string leaderMask)
        {
            //設定所需資料
            this.mLeader = leader;

            //初始化
            bool result = Init();

            if (result == true)
            {
                //更新渲染的SkinMesh
                UpdateShadowCasterList();

                //設定攝影機渲染Layer
                mLightCamera.cullingMask = LayerMask.GetMask(leaderMask);

                //設定RenderTarget參數
                if (mCmdBuffer == null ||
                mLightCamera.GetCommandBuffers(CameraEvent.BeforeImageEffectsOpaque).Length == 0)
                {
                    mCmdBuffer = new CommandBuffer();
                    mCmdBuffer.name = "ProjShadowCmdBuffer";
                    mCmdBuffer.SetRenderTarget(mShadowTex);
                    mLightCamera.AddCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, mCmdBuffer);
                }

                //開啟投影和渲染攝影機
                mProjector.enabled = true;
                mLightCamera.enabled = true;
            }
        }

        /// <summary>
        /// 關閉影子
        /// </summary>
        public void StopShadow()
        {
            if (mShadowTex != null)
                mShadowTex.Release();

            //刪除攝影機渲染參數
            if (mLightCamera != null && mCmdBuffer != null)
            {
                mLightCamera.RemoveCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, mCmdBuffer);
                mCmdBuffer.Dispose();
                mCmdBuffer = null;
            }

            //透過關閉投影和渲染攝影機方式，停止Render
            mProjector.enabled = false;
            mLightCamera.enabled = false;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private bool Init()
        {
            if (mLeader == null)
            {
                Debug.LogError("錯誤 : 未設定角色，Shadow無法執行");
                return false;
            }

            //設定組件
            mProjector = GetComponent<Projector>();
            mLightCamera = gameObject.GetComponent<UnityEngine.Camera>();
            if (mLightCamera == null)
            {
                mLightCamera = gameObject.AddComponent<UnityEngine.Camera>();
            }

            //設定渲染角色相機參數
            mLightCamera.orthographic = true;//攝影機正交視角
            // _lightCamer.cullingMask = LayerMask.GetMask("Sprites");//設定渲染的Layer
            mLightCamera.useOcclusionCulling = false;//測試過，有沒有都沒差但就放著
            mLightCamera.clearFlags = CameraClearFlags.SolidColor;
            mLightCamera.backgroundColor = new Color(0, 0, 0, 0);
            mLightCamera.allowHDR = false;
            mLightCamera.allowMSAA = false;

            //設定RenderTexture參數
            RenderTextureDescriptor rtd = new RenderTextureDescriptor((int)(Screen.width * mRenderTextureScale * mCurSizeScale), (int)(Screen.height * mRenderTextureScale * mCurSizeScale), RenderTextureFormat.R8, 0);
            rtd.useMipMap = true; //關閉多級紋理
            rtd.msaaSamples = 1; //Msaa多採樣
            mShadowTex = new RenderTexture(rtd);
            mShadowTex.filterMode = FilterMode.Bilinear;//Render方式採線性採樣
            mShadowTex.name = "_shadow";

            mLightCamera.targetTexture = mShadowTex;//設定攝影機RenderTexture
            mLightCamera.SetReplacementShader(mShadowReplaceShader, "RenderType");//設定攝影機Shader 排除 材質球"RenderType"!= Shader"RenderType"

            mProjector.material.SetTexture("_ShadowTex", mShadowTex);//設定投影器用的material

            return true;
        }


        /// <summary>
        /// 修改Material參數
        /// </summary>
        /// <param name="matList"></param>
        private void ChangeShaderRenderType(Material[] matList)
        {
            for (int i = 0; i < matList.Length; i++)
            {
                if (matList[i] == null)
                    return;

                //設定材質球RenderType 參數
                //因攝影機設定會自動排除掉 材質球RenderType != 攝影機Shader RenderType
                matList[i].SetOverrideTag("RenderType", "Shadow");
            }
        }

        /// <summary>
        /// 撈角色下的Skin組件
        /// </summary>
        private void UpdateShadowCasterList()
        {
            if (mLeader == null)
                return;

            //更新需要渲染的Skin
            mShadowCasterList = mLeader.GetComponentsInChildren<SkinnedMeshRenderer>();
            for (int i = 0; i < mShadowCasterList.Length; i++)
            {
                ChangeShaderRenderType(mShadowCasterList[i].materials);
            }
        }

        /// <summary>
        /// 設定渲染攝影機參數
        /// </summary>
        /// <param name="b">邊界盒</param>
        /// <param name="lightCamera">渲染攝影機</param>
        private void SetLightCamera(Bounds b, UnityEngine.Camera lightCamera)
        {
            lightCamera.transform.position = b.center;
            Vector3 centPos = lightCamera.transform.position;
            Vector4 vnLeftUp = new Vector3(b.max.x, b.max.y, b.max.z) - centPos;
            Vector4 vnRightUp = new Vector3(b.max.x, b.min.y, b.max.z) - centPos;
            Vector4 vnLeftDonw = new Vector3(b.max.x, b.max.y, b.min.z) - centPos;
            Vector4 vnRightDonw = new Vector3(b.min.x, b.max.y, b.max.z) - centPos;
            Vector4 vfLeftUp = new Vector3(b.min.x, b.min.y, b.min.z) - centPos;
            Vector4 vfRightUp = new Vector3(b.min.x, b.max.y, b.min.z) - centPos;
            Vector4 vfLeftDonw = new Vector3(b.min.x, b.min.y, b.max.z) - centPos;
            Vector4 vfRightDonw = new Vector3(b.max.x, b.min.y, b.min.z) - centPos;
            mVec3List.Clear();
            mVec3List.Add(vnLeftUp * mCurSizeScale);
            mVec3List.Add(vnRightUp * mCurSizeScale);
            mVec3List.Add(vnLeftDonw * mCurSizeScale);
            mVec3List.Add(vnRightDonw * mCurSizeScale);
            mVec3List.Add(vfLeftUp * mCurSizeScale);
            mVec3List.Add(vfRightUp * mCurSizeScale);
            mVec3List.Add(vfLeftDonw * mCurSizeScale);
            mVec3List.Add(vfRightDonw * mCurSizeScale);
            float maxX = -float.MaxValue;
            float maxY = -float.MaxValue;
            float maxZ = -float.MaxValue;
            float minZ = float.MaxValue;
            for (int i = 0; i < mVec3List.Count; i++)
            {
                Vector4 v = mVec3List[i];
                if (Mathf.Abs(v.x) > maxX)
                {
                    maxX = Mathf.Abs(v.x);
                }
                if (Mathf.Abs(v.y) > maxY)
                {
                    maxY = Mathf.Abs(v.y);
                }
                if (v.z > maxZ)
                {
                    maxZ = v.z;
                }
                else if (v.z < minZ)
                {
                    minZ = v.z;
                }
            }
            if (minZ < 0)
            {
                lightCamera.transform.position += -lightCamera.transform.forward.normalized * Mathf.Abs(minZ);
                maxZ = maxZ - minZ;
            }
            lightCamera.orthographic = true;
            lightCamera.aspect = maxX / maxY;
            lightCamera.orthographicSize = maxY;
            lightCamera.nearClipPlane = 0.0f;
            lightCamera.farClipPlane = Mathf.Abs(maxZ);
        }


        //LateUpdate 負責移動攝影機 + 渲染的邊界盒
        private void LateUpdate()
        {
            //設置渲染邊界盒參數
            Bounds b = new Bounds(mLeader.transform.position, Vector3.zero);
            for (int i = 0; i < mShadowCasterList.Length; i++)
            {
                if (mShadowCasterList[i] == null)
                    continue;
                b.Encapsulate(mShadowCasterList[i].bounds);
            }
            b.extents += Vector3.one * mBoundsOffset;

            // 設定渲染攝影機參數
            SetLightCamera(b, mLightCamera);

            //設定投影機參數
            mProjector.aspectRatio = mLightCamera.aspect;
            mProjector.orthographicSize = mLightCamera.orthographicSize;
            mProjector.nearClipPlane = mLightCamera.nearClipPlane;
            mProjector.farClipPlane = mLightCamera.farClipPlane;
        }
    }
}