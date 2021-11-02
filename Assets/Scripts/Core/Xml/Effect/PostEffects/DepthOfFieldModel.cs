using System;
using UnityEngine;

namespace XMLGame.Effect.PostEffects
{
    public class DepthOfFieldModel : XMLPostEffectsModel
    {
        public Camera camera;
        public float focalDistance = 10.0f;
        public float nearBlurScale = 0.0f;
        public float farBlurScale = 50.0f;
        public int downSample = 1;
        public int samplerScale = 1;
        public Transform focalObj;
        public Transform transform;

        public override void Reset()
        {
            
        }
    }
}
