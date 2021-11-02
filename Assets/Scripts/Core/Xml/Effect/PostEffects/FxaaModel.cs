﻿using System;

namespace XMLGame.Effect.PostEffects
{
    public class FxaaModel : XMLPostEffectsModel
    {
        public struct FxaaQualitySettings
        {
            //[Tooltip("The amount of desired sub-pixel aliasing removal. Effects the sharpeness of the output.")]
            //[Range(0f, 1f)]
            public float subpixelAliasingRemovalAmount;

            //[Tooltip("The minimum amount of local contrast required to qualify a region as containing an edge.")]
            //[Range(0.063f, 0.333f)]
            public float edgeDetectionThreshold;

            //[Tooltip("Local contrast adaptation value to disallow the algorithm from executing on the darker regions.")]
            //[Range(0f, 0.0833f)]
            public float minimumRequiredLuminance;

            public static FxaaQualitySettings[] presets =
                {
                    // ExtremePerformance
                    new FxaaQualitySettings
                    {
                        subpixelAliasingRemovalAmount = 0f,
                        edgeDetectionThreshold = 0.333f,
                        minimumRequiredLuminance = 0.0833f
                    },

                    // Performance
                    new FxaaQualitySettings
                    {
                        subpixelAliasingRemovalAmount = 0.25f,
                        edgeDetectionThreshold = 0.25f,
                        minimumRequiredLuminance = 0.0833f
                    },

                    // Default
                    new FxaaQualitySettings
                    {
                        subpixelAliasingRemovalAmount = 0.75f,
                        edgeDetectionThreshold = 0.166f,
                        minimumRequiredLuminance = 0.0833f
                    },

                    // Quality
                    new FxaaQualitySettings
                    {
                        subpixelAliasingRemovalAmount = 1f,
                        edgeDetectionThreshold = 0.125f,
                        minimumRequiredLuminance = 0.0625f
                    },

                    // ExtremeQuality
                    new FxaaQualitySettings
                    {
                        subpixelAliasingRemovalAmount = 1f,
                        edgeDetectionThreshold = 0.063f,
                        minimumRequiredLuminance = 0.0312f
                    }
                };
        }

        public struct FxaaConsoleSettings
        {
            //[Tooltip("The amount of spread applied to the sampling coordinates while sampling for subpixel information.")]
            //[Range(0.33f, 0.5f)]
            public float subpixelSpreadAmount;

            //[Tooltip("This value dictates how sharp the edges in the image are kept; a higher value implies sharper edges.")]
            //[Range(2f, 8f)]
            public float edgeSharpnessAmount;

            //[Tooltip("The minimum amount of local contrast required to qualify a region as containing an edge.")]
            //[Range(0.125f, 0.25f)]
            public float edgeDetectionThreshold;

            //[Tooltip("Local contrast adaptation value to disallow the algorithm from executing on the darker regions.")]
            //[Range(0.04f, 0.06f)]
            public float minimumRequiredLuminance;

            public static FxaaConsoleSettings[] presets =
                {
                    // ExtremePerformance
                    new FxaaConsoleSettings
                    {
                        subpixelSpreadAmount = 0.33f,
                        edgeSharpnessAmount = 8f,
                        edgeDetectionThreshold = 0.25f,
                        minimumRequiredLuminance = 0.06f
                    },

                    // Performance
                    new FxaaConsoleSettings
                    {
                        subpixelSpreadAmount = 0.33f,
                        edgeSharpnessAmount = 8f,
                        edgeDetectionThreshold = 0.125f,
                        minimumRequiredLuminance = 0.06f
                    },

                    // Default
                    new FxaaConsoleSettings
                    {
                        subpixelSpreadAmount = 0.5f,
                        edgeSharpnessAmount = 8f,
                        edgeDetectionThreshold = 0.125f,
                        minimumRequiredLuminance = 0.05f
                    },

                    // Quality
                    new FxaaConsoleSettings
                    {
                        subpixelSpreadAmount = 0.5f,
                        edgeSharpnessAmount = 4f,
                        edgeDetectionThreshold = 0.125f,
                        minimumRequiredLuminance = 0.04f
                    },

                    // ExtremeQuality
                    new FxaaConsoleSettings
                    {
                        subpixelSpreadAmount = 0.5f,
                        edgeSharpnessAmount = 2f,
                        edgeDetectionThreshold = 0.125f,
                        minimumRequiredLuminance = 0.04f
                    }
                };
        }

        public int preset = 2;

        public override void Reset()
        {
            preset = 2;
        }
    }
}
