using UnityEngine;

namespace XMLGame.Effect.PostEffects
{
    public sealed class ScreenReduceComponent : XMLPostEffectsComponentRenderTexture<ScreenReduceModel>
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
