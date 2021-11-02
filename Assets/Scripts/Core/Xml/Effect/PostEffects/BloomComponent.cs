using UnityEngine;


namespace XMLGame.Effect.PostEffects
{
    //-----------------------------------
    // 修改日志：
    // 1:2019年2月1日 by wz 
    //   由于在android平台获取的RenderTexture不会自动释放，造成内存问题，这里将所有RenderTexture设置为静态,在改变分辨率的时候重置。
    //-----------------------------------

    public sealed class BloomComponent : XMLPostEffectsComponentRenderTexture<BloomModel>
    {
        public XMLPostEffects.BlurType blurType = XMLPostEffects.BlurType.bloomBlur;
        private bool supportHDRTextures = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.DefaultHDR);

        static readonly int m_nRenderCount = 4;
        static readonly RenderTexture[] m_BuffList1 = new RenderTexture[m_nRenderCount];
        static readonly RenderTexture[] m_BuffList2 = new RenderTexture[m_nRenderCount];
        static RenderTexture bloomRT;

        public bool isHdr = false;

        public override bool active
        {
            get
            {
                return model.enabled
                       && !context.interrupted;
            }
        }


        public static int lastW = 0;
        public static int lastH = 0;
        public static RenderTextureFormat lastRTF = RenderTextureFormat.RInt;
        public void Prepare(RenderTexture source, Material uberMaterial, RenderTexture effectRT, XMLPostEffects.BlurType bType)
        {
            var material = context.materialFactory.Get("Hidden/PostEffects/Bloom");
            material.shaderKeywords = null;


            bool doHdr = context.isHdr && supportHDRTextures;
            isHdr = doHdr;
            var rtFormat = (doHdr) ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default;

            if (doHdr && bType == XMLPostEffects.BlurType.bloomBlur)
            {
                material.EnableKeyword("BLOOM_HDR");
            }

            var rtW = source.width / 2;
            var rtH = source.height / 2;
            if (bType == XMLPostEffects.BlurType.screenBlur)
            {
                rtW = source.width;
                rtH = source.height;
            }

            if(rtW != lastW || rtH != lastH || lastRTF != rtFormat)
            {
                ReleaseRT();
                lastW = rtW;
                lastH = rtH;
                lastRTF = rtFormat;
            }

            material.SetVector(ShaderPropertyID._threshhold, new Vector4(model.threshold_role, model.threshold_scene, model.threshold_effect_high, model.threshold_effect_low));
            material.SetVector(ShaderPropertyID._intensity, new Vector4(model.intensity_role, model.intensity_scene, model.intensity_effect_high, model.intensity_effect_low));
#if CLIENT
            float bloom_threshold_temp = SettingManager.softLight ? model.bloom_threshold : 0;
            material.SetVector(ShaderPropertyID._threshhold2, new Vector4(model.threshold_roleEmission, model.threshold_effectNo, bloom_threshold_temp, 0));
            float bloom_enhance_temp = SettingManager.softLight ? model.bloom_enhance : 0;
            material.SetVector(ShaderPropertyID._intensity2, new Vector4(model.intensity_roleEmission, model.intensity_effectNo, bloom_enhance_temp, 0));
#else
            material.SetVector(ShaderPropertyID._threshhold2, new Vector4(model.threshold_roleEmission, model.threshold_effectNo, model.bloom_threshold, 0));
            material.SetVector(ShaderPropertyID._intensity2, new Vector4(model.intensity_roleEmission, model.intensity_effectNo, model.bloom_enhance, 0));
#endif


            //bloomRT = context.renderTextureFactory.Get(rtW, rtH, 0, rtFormat);
            if (bloomRT == null)
            {
                bloomRT = RenderTexture.GetTemporary(rtW, rtH, 0, rtFormat);
                bloomRT.wrapMode = TextureWrapMode.Clamp;
                bloomRT.name = "bloomRT";
            }
            bloomRT.filterMode = FilterMode.Bilinear;

            if (effectRT != null)
            {
                material.SetTexture(ShaderPropertyID._EffectTex, effectRT);
                Graphics.Blit(source, bloomRT, material, 1);
            }
            else
            {
                Graphics.Blit(source, bloomRT, material, 0);
            }

            var last = bloomRT;
            float offset = model.offset;

            int nRenderCount = Mathf.Min(model.renderCount, m_nRenderCount);
            for (int level = 0; level < nRenderCount; level++)
            {
                //m_BuffList1[level] = context.renderTextureFactory.Get(last.width / 2, last.height / 2, 0, rtFormat);
                if(m_BuffList1[level] == null)
                {
                    m_BuffList1[level] = RenderTexture.GetTemporary(last.width / 2, last.height / 2, 0, rtFormat); // add forgotten param rw
                    m_BuffList1[level].wrapMode = TextureWrapMode.Clamp;
                    m_BuffList1[level].name = "BlomBuff1";
                }
                m_BuffList1[level].filterMode = FilterMode.Bilinear;
                Graphics.Blit(last, m_BuffList1[level], material, 4);
                last = m_BuffList1[level];
            }

            for (int level = nRenderCount - 2; level >= 0; level--)
            {
                var baseTex = m_BuffList1[level];
                material.SetTexture(ShaderPropertyID._BaseTex, baseTex);
                //offset = model.offset * (level + 2);
                material.SetFloat(ShaderPropertyID._offset, offset);
                //m_BuffList2[level] = context.renderTextureFactory.Get(baseTex.width, baseTex.height, 0, rtFormat);
                if (m_BuffList2[level] == null)
                {
                    m_BuffList2[level] = RenderTexture.GetTemporary(baseTex.width, baseTex.height, 0, rtFormat); // add forgotten param rw
                    m_BuffList2[level].wrapMode = TextureWrapMode.Clamp;
                    m_BuffList2[level].name = "BlomBuff2";
                }
                m_BuffList2[level].filterMode = FilterMode.Bilinear;
                Graphics.Blit(last, m_BuffList2[level], material, 5);
                last = m_BuffList2[level];
            }

            var bloomTex = last;

            // Release the temporary buffers

            //for (int i = 0; i < nRenderCount; i++)
            //{
            //    if (m_BuffList1[i] != null)
            //        context.renderTextureFactory.Release(m_BuffList1[i]);

            //    if (m_BuffList2[i] != null && m_BuffList2[i] != bloomTex)
            //        context.renderTextureFactory.Release(m_BuffList2[i]);

            //    m_BuffList1[i] = null;
            //    m_BuffList2[i] = null;
            //}

            //context.renderTextureFactory.Release(bloomRT);

            uberMaterial.SetTexture(ShaderPropertyID._BloomTex0, bloomTex);
#if CLIENT
            float bloom_threshold_tmp = SettingManager.softLight ? model.bloom_threshold : 0;
            float screen_reduce_temp = SettingManager.softLight ? model.screen_reduce : 1;
            uberMaterial.SetVector(ShaderPropertyID._Bloom_Settings, new Vector4(model.offset, model.bloom_reduce, screen_reduce_temp, bloom_threshold_tmp));
#else
            uberMaterial.SetVector(ShaderPropertyID._Bloom_Settings, new Vector4(model.offset, model.bloom_reduce, model.screen_reduce, model.bloom_threshold));
#endif
            if (bType == XMLPostEffects.BlurType.screenBlur)
            {
                uberMaterial.EnableKeyword("SCREENBLUR");
            }
            else
            {
                if (doHdr)
                    uberMaterial.EnableKeyword("BLOOM_HDR");
                else
                    uberMaterial.EnableKeyword("BLOOM");
            }

        }

        public static void ReleaseRT()
        {
            for (int i = 0; i < m_nRenderCount; i++)
            {
                RenderTexture.ReleaseTemporary(m_BuffList1[i]);
                RenderTexture.ReleaseTemporary(m_BuffList2[i]);
                m_BuffList1[i] = null;
                m_BuffList2[i] = null;
            }
            RenderTexture.ReleaseTemporary(bloomRT);
            bloomRT = null;
            //Debug.LogError("Release RT");
            //Resources.UnloadUnusedAssets();
            //System.GC.Collect();
        }
    }
}
