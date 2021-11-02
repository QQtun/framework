using UnityEngine;

namespace XMLGame.Effect.PostEffects
{
    public sealed class ScreenBlurComponent : XMLPostEffectsComponentRenderTexture<ScreenBlurModel>
    {
        public override bool active
        {
            get
            {
                return model.enabled
                       && !context.interrupted;
            }
        }

        public void Prepare(RenderTexture source, Material uberMaterial)
        {
            //uberMaterial.EnableKeyword("SCREENBLUR");
            /*
            if (model.screenBlurType == XMLPostEffects.ScreenBlurType.Guassion2)
            {
                var material = context.materialFactory.Get("Hidden/PostEffects/Blur");
                material.shaderKeywords = null;
                float widthMod = 1.0f / (1.0f * (1 << model.downSample));
                material.SetFloat("_DownSampleValue", model.blurSpread * widthMod);

                int renderWidth = source.width >> model.downSample;
                int renderHeight = source.height >> model.downSample;

                RenderTexture renderBuffer = context.renderTextureFactory.Get(renderWidth, renderHeight, 0, source.format);
                renderBuffer.filterMode = FilterMode.Bilinear;
                Graphics.Blit(source, renderBuffer, material, 0);

                for (int i = 0; i < model.iterations; i++)
                {
                    for (int j = 1; j <= 2; j++)
                    {
                        RenderTexture tempBuffer = context.renderTextureFactory.Get(renderWidth, renderHeight, 0, source.format);
                        Graphics.Blit(renderBuffer, tempBuffer, material, j);
                        context.renderTextureFactory.Release(renderBuffer);
                        renderBuffer = tempBuffer;
                    }
                }
                uberMaterial.SetTexture(ShaderPropertyID._screenBlurTex, renderBuffer);
                uberMaterial.EnableKeyword("SCREENBLUR");

                context.renderTextureFactory.Release(renderBuffer);
            }
            else
            {
                //教程高斯模糊
                var material = context.materialFactory.Get("Hidden/PostEffects/GaussianBlur");
                material.shaderKeywords = null;

                var rtW = source.width / model.downSample;
                var rtH = source.height / model.downSample;

                material.SetFloat(ShaderPropertyID._blurSpread, model.blurSpread);


                RenderTexture buffer0 = context.renderTextureFactory.Get(rtW, rtH, 0);
                buffer0.filterMode = FilterMode.Bilinear;

                Graphics.Blit(source, buffer0);

                RenderTexture buffer1 = context.renderTextureFactory.Get(rtW, rtH, 0);
                for (int i = 0; i < model.iterations; i++)
                {
                    material.SetFloat("_BlurSize", 1.0f + i * model.blurSpread);

                    for (int j = 0; j < 2; j++)
                    {
                        if (i != 0 || j != 0)
                            buffer1 = context.renderTextureFactory.Get(rtW, rtH, 0);

                        // Render the  pass
                        Graphics.Blit(buffer0, buffer1, material, j);
                        context.renderTextureFactory.Release(buffer0);
                        buffer0 = buffer1;
                    }
                }


                uberMaterial.SetTexture(ShaderPropertyID._screenBlurTex, buffer0);
                uberMaterial.EnableKeyword("SCREENBLUR");

                context.renderTextureFactory.Release(buffer0);
            }
            */
        }
    }
}
