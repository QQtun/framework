using UnityEngine;

namespace XMLGame.Effect.PostEffects
{
    public sealed class ToneMappingComponent : XMLPostEffectsComponentRenderTexture<ToneMappingModel>
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
