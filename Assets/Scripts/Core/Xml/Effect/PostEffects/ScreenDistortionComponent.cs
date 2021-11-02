using UnityEngine;

namespace XMLGame.Effect.PostEffects
{
    public sealed class ScreenDistortionComponent : XMLPostEffectsComponentRenderTexture<ScreenDistortionModel>
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
        }
    }
}
