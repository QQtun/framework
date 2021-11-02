using UnityEngine;

namespace XMLGame.Effect.PostEffects
{
    public sealed class MotionBlurComponent : XMLPostEffectsComponentRenderTexture<MotionBlurModel>
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
