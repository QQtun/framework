using System;

namespace XMLGame.Effect.PostEffects
{
    public class BloomModel : XMLPostEffectsModel
    {
        public float threshold_role;
        public float threshold_scene;
        public float threshold_effect_high;
        public float threshold_effect_low;
        public float intensity_role;
        public float intensity_scene;
        public float intensity_effect_high;
        public float intensity_effect_low;

        public float threshold_roleEmission;
        public float threshold_effectNo;
        public float intensity_roleEmission;
        public float intensity_effectNo;
        public float bloom_reduce;
        public float offset;
        public int renderCount = 4;
        public float bloom_threshold;
        public float bloom_enhance;
        public float screen_reduce;
        public override void Reset()
        {
            
        }
    }
}
