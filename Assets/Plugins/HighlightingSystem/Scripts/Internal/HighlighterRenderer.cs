using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;
using System.Collections.Generic;
//using XML.Pool;

namespace HighlightingSystem
{
    [DisallowMultipleComponent]
    public class HighlighterRenderer : MonoBehaviour
    {
        private struct Data
        {
            public Material material;
            public int submeshIndex;
            //public bool transparent;
        }

        #region Constants
        // Default transparency cutoff value (used for shaders without _Cutoff property)
        static private float transparentCutoff = 0.5f;

        // Flags to hide and don't save this component in editor
        private const HideFlags flags = HideFlags.HideInInspector | HideFlags.DontSaveInEditor | HideFlags.NotEditable | HideFlags.DontSaveInBuild;

        // Cull Off
        private const int cullOff = (int)CullMode.Off;
        #endregion

        static private readonly string sRenderType = "RenderType";
        static private readonly string sHightLightShaderName = "HightLightShaderName";
        static private readonly string sOpaque = "Opaque";
        static private readonly string sTransparent = "Transparent";
        static private readonly string sTransparentCutout = "TransparentCutout";
        static private readonly string sMainTex = "_MainTex";
        static private readonly string sBaseAlphaTex = "_BaseAlphaTex";
        static private readonly string sMaskMapTex = "_MaskMapTex";
        static private readonly string sNoiseTex = "_NoiseTex";
        static private readonly string sDissolorTex = "_DissolorTex";
        static private readonly string sDissolorUvTex = "_DissolorUvTex";
        static private readonly string sMaskTex = "_MaskTex";
        static private readonly string sSelfIllumin = "_SelfIllumin";




        private Renderer r;
        private List<Data> data;
        private Camera lastCamera = null;
        private bool isAlive;
        private List<Material> _replaceMats = new List<Material>();

        Material[] dissolveMat = null;
        Material[] glowMaskMat = null;
        Material[] distortionMat = null;
        Material[] emissionMat = null;
        Material[] glowMat = null;
        Material[] discardMat = null;
        Material[] avatarMat = null;
        Material[] sTransparentMat = null;

        WaitForEndOfFrame _waitForEndOfFrame = new WaitForEndOfFrame();
        /// <summary>
        /// 缓存替换材质�?
        /// </summary>
        // private static Dictionary<string, Stack<Material>> _materialPool = new Dictionary<string, Stack<Material>>();

        #region MonoBehaviour
        // 
        void Awake()
        {
            r = GetComponent<Renderer>();
            this.hideFlags = flags;
            StartCoroutine(EndOfFrame());
        }

        // Called once (before OnPreRender) for each camera if the object is visible
        void OnWillRenderObject()
        {
            Camera cam = Camera.current;
            // Another camera may intercept rendering and send it's own OnWillRenderObject events (i.e. water rendering), 
            // so we're caching currently rendering camera only if it has HighlighterBase component
            if (HighlightingBase.IsHighlightingCamera(cam))
            {
                // VR Camera renders twice per frame (once for each eye), but OnWillRenderObject is called once so we have to cache reference to the camera
                lastCamera = cam;
            }
        }

        private void OnDestroy()
        {
            for (int i = 0; i < _replaceMats.Count; i++)
            {
                string key = _replaceMats[i].shader.name;
//                XML.Pool.Material_Pool.Add(key, _replaceMats[i]);
                //Stack<Material> matStack;
                //if (_materialPool.TryGetValue(key, out matStack))
                //{
                //    matStack.Push(_replaceMats[i]);
                //}
                //else
                //{
                //    matStack = new Stack<Material>();
                //    matStack.Push(_replaceMats[i]);
                //    _materialPool.Add(key, matStack);
                //}
            }
        }

        private Material GetMat(Shader shader)
        {
            //Stack<Material> matStack;
            //Material mat = null;
            //if (_materialPool.TryGetValue(shader.name, out matStack))
            //{
            //    if (matStack.Count > 0)
            //    {
            //        mat = matStack.Pop();
            //    }
            //}
            Material mat = null;
//            MaterialPoolObj matObj = Material_Pool.Get(shader.name, new PoolObjParams());
//            if (matObj != null)
//                mat = matObj.mat;
//            else
                mat = new Material(shader);
            return mat;
        }
        #endregion

        #region Private Methods
        // 
        IEnumerator EndOfFrame()
        {
            while (true)
            {
                yield return _waitForEndOfFrame;// new WaitForEndOfFrame();

                lastCamera = null;

                if (!isAlive)
                {
                    Destroy(this);
                }
            }
        }
        #endregion

        #region Public Methods
        // 
        public void Initialize(Material sharedOpaqueMaterial, Shader transparentShader, Shader distortionShader)
        {
            data = new List<Data>();

            if (r == null)
                return;
            Material[] materials = r.sharedMaterials;
            _replaceMats.Clear();

            if (materials != null)
            {
                for (int i = 0; i < materials.Length; i++)
                {
                    Material sourceMat = materials[i];

                    if (sourceMat == null) { continue; }

                    Data d = new Data();
                    string hightLightTag = sourceMat.GetTag(sHightLightShaderName, false);
//                     if (hightLightTag == "Dissolve")
//                     {
//                         if (dissolveMat == null)
//                             dissolveMat = GetMaterial(materials.Length, dissolveShader);
//                         Material replacementMat = dissolveMat[i];// new Material(dissolveShader);
//                         replacementMat.SetTexture(ShaderPropertyID._MainTex, sourceMat.mainTexture);
//                         replacementMat.SetTextureOffset(sMainTex, sourceMat.mainTextureOffset);
//                         replacementMat.SetTextureScale(sMainTex, sourceMat.mainTextureScale);
//                         replacementMat.SetColor(ShaderPropertyID._TinColor, sourceMat.GetColor(ShaderPropertyID._TinColor));
//                         replacementMat.SetTexture(ShaderPropertyID._DissolorTex, sourceMat.GetTexture(ShaderPropertyID._DissolorTex));
//                         replacementMat.SetTextureOffset(sDissolorTex, sourceMat.GetTextureOffset(sDissolorTex));
//                         replacementMat.SetTextureScale(sDissolorTex, sourceMat.GetTextureScale(sDissolorTex));
//                         replacementMat.SetVector(ShaderPropertyID._DissolorTexUVMoveSpeed, sourceMat.GetVector(ShaderPropertyID._DissolorTexUVMoveSpeed));
//                         replacementMat.SetTexture(ShaderPropertyID._DissolorUvTex, sourceMat.GetTexture(ShaderPropertyID._DissolorUvTex));
//                         replacementMat.SetTextureOffset(sDissolorUvTex, sourceMat.GetTextureOffset(sDissolorUvTex));
//                         replacementMat.SetTextureScale(sDissolorUvTex, sourceMat.GetTextureScale(sDissolorUvTex));
//                         replacementMat.SetVector(ShaderPropertyID._DissolorUvSpeed, sourceMat.GetVector(ShaderPropertyID._DissolorUvSpeed));
//                         replacementMat.SetFloat(ShaderPropertyID._RAmount, sourceMat.GetFloat(ShaderPropertyID._RAmount));
//                         replacementMat.SetFloat(ShaderPropertyID._DissolorWith, sourceMat.GetFloat(ShaderPropertyID._DissolorWith));
//                         replacementMat.SetColor(ShaderPropertyID._DissColor, sourceMat.GetColor(ShaderPropertyID._DissColor));
//                         replacementMat.SetFloat(ShaderPropertyID._Emission, sourceMat.GetFloat(ShaderPropertyID._Emission));
//                         replacementMat.SetFloat(ShaderPropertyID._DissColorIlluminate, sourceMat.GetFloat(ShaderPropertyID._DissColorIlluminate));
//                         if (sourceMat.HasProperty(ShaderPropertyID._MaskThreshold))
//                         {
//                             replacementMat.SetTexture(ShaderPropertyID._MaskTex, sourceMat.GetTexture(ShaderPropertyID._MaskTex));
//                             replacementMat.SetTextureOffset(sMaskTex, sourceMat.GetTextureOffset(sMaskTex));
//                             replacementMat.SetTextureScale(sMaskTex, sourceMat.GetTextureScale(sMaskTex));
//                             replacementMat.SetFloat(ShaderPropertyID._MaskThreshold, sourceMat.GetFloat(ShaderPropertyID._MaskThreshold));
//                         }
// 
//                         replacementMat.SetInt(ShaderPropertyID._Cull, sourceMat.HasProperty(ShaderPropertyID._Cull) ? sourceMat.GetInt(ShaderPropertyID._Cull) : 2);
//                         _replaceMats.Add(replacementMat);
//                         d.material = replacementMat;
//                     }
//                     if (hightLightTag == "GlowMask")
//                     {
//                         if (glowMaskMat == null)
//                             glowMaskMat = GetMaterial(materials.Length, glowMaskShader);
//                         Material replacementMat = glowMaskMat[i];// new Material(glowMaskShader);
//                         replacementMat.SetTexture(ShaderPropertyID._MainTex, sourceMat.mainTexture);
//                         replacementMat.SetTextureOffset(sMainTex, sourceMat.mainTextureOffset);
//                         replacementMat.SetTextureScale(sMainTex, sourceMat.mainTextureScale);
//                         replacementMat.SetVector(ShaderPropertyID._MainTexUvSpeed, sourceMat.GetVector(ShaderPropertyID._MainTexUvSpeed));
//                         replacementMat.SetTexture(ShaderPropertyID._BaseAlphaTex, sourceMat.GetTexture(ShaderPropertyID._BaseAlphaTex));
//                         replacementMat.SetTextureOffset(sBaseAlphaTex, sourceMat.GetTextureOffset(sBaseAlphaTex));
//                         replacementMat.SetTextureScale(sBaseAlphaTex, sourceMat.GetTextureScale(sBaseAlphaTex));
//                         replacementMat.SetVector(ShaderPropertyID._BaseMoveSpeed, sourceMat.GetVector(ShaderPropertyID._BaseMoveSpeed));
//                         replacementMat.SetTexture(ShaderPropertyID._MaskMapTex, sourceMat.GetTexture(ShaderPropertyID._MaskMapTex));
//                         replacementMat.SetTextureOffset(sMaskMapTex, sourceMat.GetTextureOffset(sMaskMapTex));
//                         replacementMat.SetTextureScale(sMaskMapTex, sourceMat.GetTextureScale(sMaskMapTex));
//                         replacementMat.SetVector(ShaderPropertyID._MaskUVSpeed, sourceMat.GetVector(ShaderPropertyID._MaskUVSpeed));
//                         replacementMat.SetColor(ShaderPropertyID._Color, sourceMat.GetColor(ShaderPropertyID._Color));
//                         replacementMat.SetColor(ShaderPropertyID._ConstantColor, sourceMat.GetColor(ShaderPropertyID._ConstantColor));
//                         replacementMat.SetFloat(ShaderPropertyID._GlowScale, sourceMat.GetFloat(ShaderPropertyID._GlowScale));
//                         replacementMat.SetFloat(ShaderPropertyID._Param, sourceMat.GetFloat(ShaderPropertyID._Param));
// 
//                         int cutoff = ShaderPropertyID._Cutoff;
//                         replacementMat.SetFloat(cutoff, sourceMat.HasProperty(cutoff) ? sourceMat.GetFloat(cutoff) : transparentCutoff);
//                         replacementMat.SetInt(ShaderPropertyID._Cull, sourceMat.HasProperty(ShaderPropertyID._Cull) ? sourceMat.GetInt(ShaderPropertyID._Cull) : 2);
//                         _replaceMats.Add(replacementMat);
//                         d.material = replacementMat;
// 
//                     }
//                    else if (hightLightTag == "Distortion")
                    if (hightLightTag == "Distortion")
                    {
                        if (distortionMat == null)
                            distortionMat = GetMaterial(materials.Length, distortionShader);
                        Material replacementMat = distortionMat[i];// new Material(distortionShader);

                        replacementMat.SetTexture(ShaderPropertyID._NoiseTex, sourceMat.GetTexture(ShaderPropertyID._NoiseTex));
                        replacementMat.SetTextureOffset(sNoiseTex, sourceMat.GetTextureOffset(sNoiseTex));
                        replacementMat.SetTextureScale(sNoiseTex, sourceMat.GetTextureScale(sNoiseTex));

                        replacementMat.SetFloat(ShaderPropertyID._HeatForce, sourceMat.GetFloat(ShaderPropertyID._HeatForce));

                        if (sourceMat.HasProperty(ShaderPropertyID._MaskTex))
                        {
                            replacementMat.SetTexture(ShaderPropertyID._MaskTex, sourceMat.GetTexture(ShaderPropertyID._MaskTex));
                            replacementMat.SetTextureOffset(sMaskTex, sourceMat.GetTextureOffset(sMaskTex));
                            replacementMat.SetTextureScale(sMaskTex, sourceMat.GetTextureScale(sMaskTex));
                        }
                        _replaceMats.Add(replacementMat);
                        d.material = replacementMat;
                    }
//                     else if (hightLightTag == "Emission")
//                     {
//                         if (emissionMat == null)
//                             emissionMat = GetMaterial(materials.Length, emissionShader);
//                         Material replacementMat = emissionMat[i];// new Material(emissionShader);
// 
//                         replacementMat.SetTexture(ShaderPropertyID._SelfIllumin, sourceMat.GetTexture(ShaderPropertyID._SelfIllumin));
//                         replacementMat.SetTextureOffset(sSelfIllumin, sourceMat.GetTextureOffset(sSelfIllumin));
//                         replacementMat.SetTextureScale(sSelfIllumin, sourceMat.GetTextureScale(sSelfIllumin));
// 
//                         replacementMat.SetFloat(ShaderPropertyID._EmissionThreshold, sourceMat.GetFloat(ShaderPropertyID._EmissionThreshold));
//                         _replaceMats.Add(replacementMat);
//                         d.material = replacementMat;
//                     }
//                     else if (hightLightTag == "Glow")
//                     {
//                         if (glowMat == null)
//                             glowMat = GetMaterial(materials.Length, glowShader);
//                         Material replacementMat = glowMat[i];// new Material(glowShader);
// 
//                         replacementMat.SetTexture(ShaderPropertyID._MainTex, sourceMat.mainTexture);
//                         replacementMat.SetTextureOffset(sMainTex, sourceMat.mainTextureOffset);
//                         replacementMat.SetTextureScale(sMainTex, sourceMat.mainTextureScale);
// 
//                         replacementMat.SetFloat(ShaderPropertyID._EmissionThreshold, sourceMat.GetFloat(ShaderPropertyID._EmissionThreshold));
//                         _replaceMats.Add(replacementMat);
//                         d.material = replacementMat;
//                     }
//                     else if (hightLightTag == "Discard")
//                     {
//                         if (discardMat == null)
//                             discardMat = GetMaterial(materials.Length, discardShader);
//                         Material replacementMat = discardMat[i];// new Material(discardShader);
//                         _replaceMats.Add(replacementMat);
//                         d.material = replacementMat;
//                     }
                    else
                    {
                        string tag = sourceMat.GetTag(sRenderType, true, sOpaque);
                        if (tag == sTransparent || tag == sTransparentCutout)
                        {
                            if (sTransparentMat == null)
                                sTransparentMat = GetMaterial(materials.Length, transparentShader);
                            Material replacementMat = sTransparentMat[i];// new Material(transparentShader);

                            // To render both sides of the Sprite
                            if (r is SpriteRenderer) { replacementMat.SetInt(ShaderPropertyID._Cull, cullOff); }

                            if (sourceMat.HasProperty(ShaderPropertyID._MainTex))
                            {
                                replacementMat.SetTexture(ShaderPropertyID._MainTex, sourceMat.mainTexture);
                                replacementMat.SetTextureOffset(sMainTex, sourceMat.mainTextureOffset);
                                replacementMat.SetTextureScale(sMainTex, sourceMat.mainTextureScale);
                            }

                            int cutoff = ShaderPropertyID._Cutoff;
                            replacementMat.SetFloat(cutoff, sourceMat.HasProperty(cutoff) ? sourceMat.GetFloat(cutoff) : transparentCutoff);
                            _replaceMats.Add(replacementMat);
                            d.material = replacementMat;
                        }
                        else
                        {
                            d.material = sharedOpaqueMaterial;
                        }
                    }

                    d.submeshIndex = i;
                    data.Add(d);
                }
            }
        }

        // 
        public bool FillBuffer(CommandBuffer buffer, bool bFroceDrow)
        {
            if (r == null) { return false; }

            if (lastCamera == Camera.current || bFroceDrow)
            {
                for (int i = 0, imax = data.Count; i < imax; i++)
                {
                    Data d = data[i];
                    //if (d.material.shader.name == "Hidden/Highlighted/Emission")
                    //{
                    //    Debug.LogError("1");
                    //}
                    buffer.DrawRenderer(r, d.material, d.submeshIndex);
                }
            }
            return true;
        }

        // 
        public void SetState(bool alive)
        {
            isAlive = alive;
        }

        public Material[] GetMaterial(int l, Shader s)
        {
            Material[] mats = new Material[l];
            for (int j = 0; j < mats.Length; j++)
            {
                mats[j] = GetMat(s);
            }
            return mats;
        }                  
    }
    #endregion
}