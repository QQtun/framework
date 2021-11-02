using UnityEngine;

namespace XMLGame.Effect.PostEffects
{
    public sealed class SkillBlurComponent : XMLPostEffectsComponentRenderTexture<SkillBlurModel>
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
