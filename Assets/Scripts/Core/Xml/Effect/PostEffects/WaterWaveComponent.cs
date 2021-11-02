using UnityEngine;

namespace XMLGame.Effect.PostEffects
{
    public sealed class WaterWaveComponent : XMLPostEffectsComponentRenderTexture<WaterWaveModel>
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
