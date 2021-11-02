using UnityEngine;
using System.Collections;

namespace XMLGame.Effect.PostEffects
{
	static public class ShaderPropertyID
	{
		#region PUBLIC FIELDS
		// Common
		static public int _MainTex { get; private set; }
        static public int _EffectTex { get; private set; }

		// bloom
        static public int _threshhold { get; private set; }
        static public int _intensity { get; private set; }
        static public int _offset { get; private set; }
        static public int _BloomTex0 { get; private set; }
        static public int _BaseTex { get; private set; }
        static public int _Bloom_Settings { get; private set; }
        static public int _threshhold2 { get; private set; }
        static public int _intensity2 { get; private set; }

        // tonemapping
        static public int _ToneMapping_Settings1 { get; private set; }
        static public int _ToneMapping_Settings2 { get; private set; }
       
        // fxaa
        static public int _QualitySettings { get; private set; }
        static public int _ConsoleSettings { get; private set; }

        // colorbalance
        static public int _saturation { get; private set; }
        static public int _contrast { get; private set; }

        // depthoffield
        static public int _offsets { get; private set; }
        static public int _BlurTex { get; private set; }
        static public int _focalDistance { get; private set; }
        static public int _nearBlurScale { get; private set; }
        static public int _farBlurScale { get; private set; }

        // distortion
        static public int _DistortionTex { get; private set; }

        // motionblur
        static public int _motionFactor { get; private set; }

        // skillblur
        static public int _skillUV { get; private set; }
        static public int _skillSaturation { get; private set; }
        static public int _skillAlpha { get; private set; }

        // waterwave
        static public int _distanceFactor { get; private set; }
        static public int _timeFactor { get; private set; }
        static public int _totalFactor { get; private set; }
        static public int _centerFactor { get; private set; }
        static public int _wwFactor { get; private set; }

        // screendistortion
        static public int _DownSampleTex { get; private set; }
        static public int _sdFactor { get; private set; }
        static public int _minAlpha { get; private set; }


        // verticalfog
        static public int _frustumCornersWS { get; private set; }
        static public int _verticalFogColor { get; private set; }
        static public int _verticalFogStart { get; private set; }
        static public int _verticalFogDensity { get; private set; }        

        // screenreduce
        static public int _screenReduce { get; private set; }
        static public int _screenReduceColor { get; private set; }

        // sharpness
        static public int _sharpnessOffsets { get; private set; }
        static public int _sharpness { get; private set; }

        // filtermirror
        static public int _filterMirrorColor { get; private set; }
        static public int _filterStandardColor { get; private set; }
		#endregion

		#region PRIVATE FIELDS
		static private bool initialized = false;
		#endregion

		// 
		static public void Initialize()
		{
			if (initialized) { return; }

			_MainTex = Shader.PropertyToID("_MainTex");
            _EffectTex = Shader.PropertyToID("_EffectTex");
            
            _threshhold = Shader.PropertyToID("_threshhold");
            _intensity = Shader.PropertyToID("_intensity");
            _threshhold2 = Shader.PropertyToID("_threshhold2");
            _intensity2 = Shader.PropertyToID("_intensity2");
            _offset = Shader.PropertyToID("_offset");
            _BloomTex0 = Shader.PropertyToID("_BloomTex0");
            _BaseTex = Shader.PropertyToID("_BaseTex");
            _Bloom_Settings = Shader.PropertyToID("_Bloom_Settings");

            _ToneMapping_Settings1 = Shader.PropertyToID("_ToneMapping_Settings1");
            _ToneMapping_Settings2 = Shader.PropertyToID("_ToneMapping_Settings2");
            
            _QualitySettings = Shader.PropertyToID("_QualitySettings");
            _ConsoleSettings = Shader.PropertyToID("_ConsoleSettings");

            _saturation = Shader.PropertyToID("_saturation");
            _contrast = Shader.PropertyToID("_contrast");            

            _offsets = Shader.PropertyToID("_offsets");
            _BlurTex = Shader.PropertyToID("_BlurTex");   
            _focalDistance = Shader.PropertyToID("_focalDistance");
            _nearBlurScale = Shader.PropertyToID("_nearBlurScale");   
            _farBlurScale = Shader.PropertyToID("_farBlurScale");               

            _DistortionTex = Shader.PropertyToID("_DistortionTex");

            _motionFactor = Shader.PropertyToID("_motionFactor");      
        
            _skillUV = Shader.PropertyToID("_skillUV");
            _skillSaturation = Shader.PropertyToID("_skillSaturation");   
            _skillAlpha = Shader.PropertyToID("_skillAlpha");   

            _distanceFactor = Shader.PropertyToID("_distanceFactor");
            _timeFactor = Shader.PropertyToID("_timeFactor");   
            _totalFactor = Shader.PropertyToID("_totalFactor");   
            _centerFactor = Shader.PropertyToID("_centerFactor");
            _wwFactor = Shader.PropertyToID("_wwFactor");

            _DownSampleTex = Shader.PropertyToID("_DownSampleTex");
            _sdFactor = Shader.PropertyToID("_sdFactor");
            _minAlpha = Shader.PropertyToID("_minAlpha");

            _frustumCornersWS = Shader.PropertyToID("_frustumCornersWS");
            _verticalFogColor = Shader.PropertyToID("_verticalFogColor");
            _verticalFogStart = Shader.PropertyToID("_verticalFogStart");
            _verticalFogDensity = Shader.PropertyToID("_verticalFogDensity");

            _screenReduce = Shader.PropertyToID("_screenReduce");
            _screenReduceColor = Shader.PropertyToID("_screenReduceColor");

            _sharpnessOffsets = Shader.PropertyToID("_sharpnessOffsets");
            _sharpness = Shader.PropertyToID("_sharpness");

            _filterMirrorColor = Shader.PropertyToID("_filterMirrorColor");
            _filterStandardColor = Shader.PropertyToID("_filterStandardColor");         

			initialized = true;
		}
	}
}