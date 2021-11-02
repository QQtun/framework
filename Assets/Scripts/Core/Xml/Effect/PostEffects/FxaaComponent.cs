using UnityEngine;

namespace XMLGame.Effect.PostEffects
{
    public sealed class FxaaComponent : XMLPostEffectsComponentRenderTexture<FxaaModel>
    {

        public override bool active
        {
            get
            {
                return model.enabled
                       && !context.interrupted;
            }
        }
        public void Render(RenderTexture source, RenderTexture destination)
        {
            var material = context.materialFactory.Get("Hidden/PostEffects/FXAA");
            var qualitySettings = FxaaModel.FxaaQualitySettings.presets[model.preset];
            var consoleSettings = FxaaModel.FxaaConsoleSettings.presets[model.preset];

            material.SetVector(ShaderPropertyID._QualitySettings,
                new Vector3(
                    qualitySettings.subpixelAliasingRemovalAmount,
                    qualitySettings.edgeDetectionThreshold,
                    qualitySettings.minimumRequiredLuminance
                    )
                );

            material.SetVector(ShaderPropertyID._ConsoleSettings,
                new Vector4(
                    consoleSettings.subpixelSpreadAmount,
                    consoleSettings.edgeSharpnessAmount,
                    consoleSettings.edgeDetectionThreshold,
                    consoleSettings.minimumRequiredLuminance
                    )
                );

            Graphics.Blit(source, destination, material, 0);
        }
    }
}
