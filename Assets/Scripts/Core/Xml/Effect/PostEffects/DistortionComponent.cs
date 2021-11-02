using UnityEngine;

namespace XMLGame.Effect.PostEffects
{
    public sealed class DistortionComponent : XMLPostEffectsComponentRenderTexture<DistortionModel>
    {

        public override bool active
        {
            get
            {
                return model.enabled
                       && !context.interrupted;
            }
        }
    }
}
