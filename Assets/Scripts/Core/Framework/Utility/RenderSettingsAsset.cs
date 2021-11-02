
using UnityEngine;
using UnityEngine.Rendering;
using System;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Core.Framework.Utility
{
    [CreateAssetMenu(fileName = "RenderSettingsAsset", menuName = "Config/RenderSettingsAsset")]
    public class RenderSettingsAsset : ScriptableObject
    {
#if UNITY_EDITOR
        [MenuItem("程式工具/Create RenderSettingAsset By Current Scene")]
        public static void CreateRenderSettingByCurrentScene()
        {
            var newAsset = CreateInstance<RenderSettingsAsset>();
            newAsset.CopyFromSettings();
            var path = EditorUtility.SaveFilePanel("檔案位置", "Assets", "RenderSettingsAsset", "");
            if (!string.IsNullOrEmpty(path))
            {
                path = path.Substring(path.IndexOf("Assets/"));
                AssetDatabase.CreateAsset(newAsset, path + ".asset");
            }
        }
#endif
        [Serializable]
        public class LightmapData
        {
            [FormerlySerializedAs("LightmapColor")]
            public Texture2D lightmapColor;
            [FormerlySerializedAs("LightmapDir")]
            public Texture2D lightmapDir;
            [FormerlySerializedAs("ShadowMask")]
            public Texture2D shadowMask;
        }

        [FormerlySerializedAs("Fog")]
        public bool fog;
        [FormerlySerializedAs("FogStartDistance")]
        public float fogStartDistance;
        [FormerlySerializedAs("FogEndDistance")]
        public float fogEndDistance;
        [FormerlySerializedAs("FogMode")]
        public FogMode fogMode;
        [FormerlySerializedAs("ShadowMask")]
        public Color fogColor;
        [FormerlySerializedAs("FogDensity")]
        public float fogDensity;
        [FormerlySerializedAs("AmbientMode")]
        public AmbientMode ambientMode;
        [FormerlySerializedAs("AmbientSkyColor")]
        public Color ambientSkyColor;
        [FormerlySerializedAs("AmbientEquatorColor")]
        public Color ambientEquatorColor;
        [FormerlySerializedAs("AmbientGroundColor")]
        public Color ambientGroundColor;
        [FormerlySerializedAs("AmbientIntensity")]
        public float ambientIntensity;
        [FormerlySerializedAs("AmbientLight")]
        public Color ambientLight;
        [FormerlySerializedAs("SubtractiveShadowColor")]
        public Color subtractiveShadowColor;
        [FormerlySerializedAs("Skybox")]
        public Material skybox;
        [FormerlySerializedAs("Sun")]
        public Light sun;
        [FormerlySerializedAs("AmbientProbe")]
        public SphericalHarmonicsL2 ambientProbe;
        [FormerlySerializedAs("CustomReflection")]
        public Cubemap customReflection;
        [FormerlySerializedAs("ReflectionIntensity")]
        public float reflectionIntensity;
        [FormerlySerializedAs("ReflectionBounces")]
        public int reflectionBounces;
        [FormerlySerializedAs("DefaultReflectionMode")]
        public DefaultReflectionMode defaultReflectionMode;
        [FormerlySerializedAs("DefaultReflectionResolution")]
        public int defaultReflectionResolution;
        [FormerlySerializedAs("HaloStrength")]
        public float haloStrength;
        [FormerlySerializedAs("FlareStrength")]
        public float flareStrength;
        [FormerlySerializedAs("FlareFadeSpeed")]
        public float flareFadeSpeed;
        [FormerlySerializedAs("LightMaps")]
        public LightmapData[] lightMaps;

        public bool IsEmpty { get; private set; } = true;

        public void Clear()
        {
            IsEmpty = true;
        }

        [ContextMenu("Apply")]
        public void Apply()
        {
            if (IsEmpty)
                return;

            RenderSettings.fog = fog;
            RenderSettings.fogStartDistance = fogStartDistance;
            RenderSettings.fogEndDistance = fogEndDistance;
            RenderSettings.fogMode = fogMode;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogDensity = fogDensity;
            RenderSettings.ambientMode = ambientMode;
            RenderSettings.ambientSkyColor = ambientSkyColor;
            RenderSettings.ambientEquatorColor = ambientEquatorColor;
            RenderSettings.ambientGroundColor = ambientGroundColor;
            RenderSettings.ambientIntensity = ambientIntensity;
            RenderSettings.ambientLight = ambientLight;
            RenderSettings.subtractiveShadowColor = subtractiveShadowColor;
            RenderSettings.skybox = skybox;
            RenderSettings.sun = sun;
            RenderSettings.ambientProbe = ambientProbe;
            RenderSettings.customReflection = customReflection;
            RenderSettings.reflectionIntensity = reflectionIntensity;
            RenderSettings.reflectionBounces = reflectionBounces;
            RenderSettings.defaultReflectionMode = defaultReflectionMode;
            RenderSettings.defaultReflectionResolution = defaultReflectionResolution;
            RenderSettings.haloStrength = haloStrength;
            RenderSettings.flareStrength = flareStrength;
            RenderSettings.flareFadeSpeed = flareFadeSpeed;

            var unityLightmaps = new UnityEngine.LightmapData[lightMaps.Length];
            for (int i = 0; i < lightMaps.Length; i++)
            {
                var lm = lightMaps[i];
                unityLightmaps[i] = new UnityEngine.LightmapData() { lightmapColor = lm.lightmapColor, lightmapDir = lm.lightmapDir, shadowMask = lm.shadowMask };
            }
            LightmapSettings.lightmaps = unityLightmaps;
        }

        [ContextMenu("CopyFromSettings")]
        public void CopyFromSettings()
        {
            fog = RenderSettings.fog;
            fogStartDistance = RenderSettings.fogStartDistance;
            fogEndDistance = RenderSettings.fogEndDistance;
            fogMode = RenderSettings.fogMode;
            fogColor = RenderSettings.fogColor;
            fogDensity = RenderSettings.fogDensity;
            ambientMode = RenderSettings.ambientMode;
            ambientSkyColor = RenderSettings.ambientSkyColor;
            ambientEquatorColor = RenderSettings.ambientEquatorColor;
            ambientGroundColor = RenderSettings.ambientGroundColor;
            ambientIntensity = RenderSettings.ambientIntensity;
            ambientLight = RenderSettings.ambientLight;
            subtractiveShadowColor = RenderSettings.subtractiveShadowColor;
            skybox = RenderSettings.skybox;
            sun = RenderSettings.sun;
            ambientProbe = RenderSettings.ambientProbe;
            customReflection = RenderSettings.customReflection;
            reflectionIntensity = RenderSettings.reflectionIntensity;
            reflectionBounces = RenderSettings.reflectionBounces;
            defaultReflectionMode = RenderSettings.defaultReflectionMode;
            defaultReflectionResolution = RenderSettings.defaultReflectionResolution;
            haloStrength = RenderSettings.haloStrength;
            flareStrength = RenderSettings.flareStrength;
            flareFadeSpeed = RenderSettings.flareFadeSpeed;

            var unityLightmaps = LightmapSettings.lightmaps;
            lightMaps = new LightmapData[unityLightmaps.Length];
            for (int i = 0; i < unityLightmaps.Length; i++)
            {
                var lm = unityLightmaps[i];
                lightMaps[i] = new LightmapData() { lightmapColor = lm.lightmapColor, lightmapDir = lm.lightmapDir, shadowMask = lm.shadowMask };
            }

            IsEmpty = false;
        }
    }
}