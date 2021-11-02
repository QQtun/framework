using System;
using UnityEngine;
#if CLIENT
using XMLEngine.GameEngine.Logic;
#endif

namespace XMLGame.Effect.PostEffects
{
    public class VerticalFogModel : XMLPostEffectsModel
    {
        public Color verticalFogColor;
        public float verticalFogStart;
        public float verticalFogDensity;
        public Camera camera;
        public override void Reset()
        {
            
        }
    }
}
