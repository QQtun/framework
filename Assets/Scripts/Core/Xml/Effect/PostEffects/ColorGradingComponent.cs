using UnityEngine;

namespace XMLGame.Effect.PostEffects
{
    public sealed class ColorGradingComponent : XMLPostEffectsComponentRenderTexture<ColorGradingModel>
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
            uberMaterial.SetVector(ShaderPropertyID._sharpnessOffsets, new Vector4(model.sharpness_kernal / source.width, model.sharpness_kernal / source.height));
            uberMaterial.SetFloat(ShaderPropertyID._sharpness, model.sharpness);
            uberMaterial.EnableKeyword("SHARPNESS");
        }
    }
}
