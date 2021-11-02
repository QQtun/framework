using UnityEngine;

namespace XMLGame.Effect.PostEffects
{
    public sealed class ScreenOverlayerComponent : XMLPostEffectsComponentRenderTexture<ScreenOverlayerModel>
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
