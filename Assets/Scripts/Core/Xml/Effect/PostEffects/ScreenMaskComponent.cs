using UnityEngine;

namespace XMLGame.Effect.PostEffects
{
    public sealed class ScreenMaskComponent : XMLPostEffectsComponentRenderTexture<ScreenMaskModel>
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
