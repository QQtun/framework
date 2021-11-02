using UnityEngine;

namespace XMLGame.Effect.PostEffects
{
    public sealed class ColorBalanceComponent : XMLPostEffectsComponentRenderTexture<ColorBalanceModel>
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
            if (model.saturation != 1)
            {
                uberMaterial.SetFloat(ShaderPropertyID._saturation, model.saturation);
                uberMaterial.EnableKeyword("SATURATION");
            }
            if (model.contrast != 1)
            {
                uberMaterial.SetFloat(ShaderPropertyID._contrast, model.contrast);
                uberMaterial.EnableKeyword("CONTRAST");
            }
        }
    }
}
