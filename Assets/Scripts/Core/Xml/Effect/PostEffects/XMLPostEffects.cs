using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System;
#if CLIENT
using XMLEngine.GameEngine.Logic;
using XLua;
#endif

namespace XMLGame.Effect.PostEffects
{
#if CLIENT
    [LuaCallCSharp]
#endif

    [AddComponentMenu("XML/XML PostEffects")]
    public class XMLPostEffects : MonoBehaviour
    {
        public bool show_effectrt_for_test = false;
        public bool show_depthrt_for_test = false;
        public bool enable_depth_for_test = false;

        [HideInInspector]
        public bool IsHdr { get { return m_Bloom.isHdr; } set { } }

        //--------------------------------------------------entialiasing
        public bool enable_antialiasing = true;

        
        // 

        public delegate void EnableDelegate();

        public delegate void DisableDelegate();

        public event EnableDelegate EnableEvent;
        public event DisableDelegate DisableEvent;
        
            
        public enum FxaaPreset
        {
            ExtremePerformance,
            Performance,
            Default,
            Quality,
            ExtremeQuality
        }

        public FxaaPreset FXAAPreset = FxaaPreset.Default;

        //--------------------------------------------------bloom
        public bool enable_bloom = true;

        // hdr泛光
        [Range(0.0f, 1.5f)]
        public float threshold_role = 0.8f; // 角色
        [Range(0.0f, 1.5f)]
        public float threshold_scene = 0.8f; // 场景
        [Range(0.0f, 1.5f)]
        public float threshold_effect_high = 0.45f; // 特效高
        [Range(0.0f, 1.5f)]
        public float threshold_effect_low = 0.45f;// 特效低
        [HideInInspector]
        public float threshold_roleEmission = 0; // 角色自发光
        [HideInInspector]
        public float threshold_effectNo = 0; // 特效不泛

        [Range(0.0f, 10.0f)]
        public float intensity_role = 2f;
        [Range(0.0f, 10.0f)]
        public float intensity_scene = 1f;
        [Range(0.0f, 10.0f)]
        public float intensity_effect_high = 0.5f;
        [Range(0.0f, 10.0f)]
        public float intensity_effect_low = 0.2f;
        //[HideInInspector]
        [Range(0.0f, 10.0f)]
        public float intensity_roleEemission = 1;
        [HideInInspector]
        public float intensity_effectNo = 0;

        [Range(0.0f, 1.0f)]
        public float bloom_reduce = 0.18f;

        [Range(0.0f, 10.0f)]
        public float bloom_offset = 1.0f;
        // hdr泛光

        // 非hdr泛光
        [Range(0.0f, 1.5f)]
        public float threshold_role_nohdr = 0.8f;
        [Range(0.0f, 1.5f)]
        public float threshold_scene_nohdr = 0.8f;
        [Range(0.0f, 1.5f)]
        public float threshold_effect_high_nohdr = 0.65f;
        [Range(0.0f, 1.5f)]
        public float threshold_effect_low_nohdr = 0.65f;
        [HideInInspector]
        public float threshold_roleEmission_nohdr = 0;
        [HideInInspector]
        public float threshold_effectNo_nohdr = 0;

        [Range(0.0f, 10.0f)]
        public float intensity_role_nohdr = 1;
        [Range(0.0f, 10.0f)]
        public float intensity_scene_nohdr = 1;
        [Range(0.0f, 10.0f)]
        public float intensity_effect_high_nohdr = 4;
        [Range(0.0f, 10.0f)]
        public float intensity_effect_low_nohdr = 2f;
        //[HideInInspector]
        [Range(0.0f, 10.0f)]
        public float intensity_roleEemission_nohdr = 1;
        [HideInInspector]
        public float intensity_effectNo_nohdr = 0;

        [Range(0.0f, 1.0f)]
        public float bloom_reduce_nohdr = 0.5f;

        [Range(0.0f, 10.0f)]
        public float bloom_offset_nohdr = 1.0f;

        //通用

        [Range(1, 4)]
        public int bloomRenderCount = 4;

        //[HideInInspector]
        [Range(0.0f, 1.5f)]
        public float bloom_threshold = 0.01f;

        [Range(0.0f, 1.0f)]
        public float bloom_enhance = 0f;

        [Range(0.0f, 1.0f)]
        public float screen_reduce = 1f;

        //--------------------------------------------------tonemapping
        public enum ToneMapping
        {
            FILMIC,
	        ACES,
        }
        

        public bool enable_tonemapping = true;
        public ToneMapping tm_mode = ToneMapping.FILMIC;
        public float tm_filmic_exposure = 2f;
        public float tm_filmic_luminance = 0.6f;

        [HideInInspector]
        public float tm_aces_exposure = 0.5f;
        [HideInInspector]
        public float tm_aces_luminance = 1f;


        //--------------------------------------------------screenblur
        public bool enable_screenblur = false;
        public enum BlurType
        {
            bloomBlur,
            screenBlur,
        }
        [HideInInspector]
        public BlurType blurType = BlurType.bloomBlur;

   
        //--------------------------------------------------sharpness
        // public bool enable_colorgrading = false;
        // [Range(0.0f, 2.0f)]
        // public float sharpness = 0f;
        // [Range(0.25f, 2.0f)]
        // public float sharpness_kernel = 0.5f;

        //--------------------------------------------------distortion
        public bool enable_distortion = true;

        //--------------------------------------------------colorbalance

        //public bool enable_colorbalance = false;
        
        //[Range(0.0f, 2.0f)]
        //public float saturation = 1f;

        //--------------------------------------------------depthoffield

        public bool enable_depthoffield = false;
        public float focalDistance = 10f;
        public float farBlurScale = 50f;

        [HideInInspector]
        public Transform focalObj;

        //--------------------------------------------------motionblur
        //[Header("MotionBlur")]
        public bool enable_motionblur = false;

        //public Color motion_maxcolor = Color.red;
        public float motion_delay_time = 0f;
        public float motion_time = 0.2f;
        //[HideInInspector]
        public float motion_run_factor = 0.01f;
        private bool is_motion = false;
        private float motion_used_time = 0f;
        private float motion_delay_used_time = 0f;
        //[Space(10)]
        //--------------------------------------------------skillblur
        // public bool enable_skillblur = false;

        // public float skill_delay_time = 0f;
        // public float skill_time = 0.2f;

        // private float skill_run_alpha = 0.2f;
        // private float skill_uv_x = 0.03f;
        // private float skill_uv_y = 0f;
        // private float skill_run_saturation = 1f;
        // private Vector2 skill_run_uv = new Vector2(0, 0);
        // private bool is_skill = false;
        // private float skill_used_time = -1f;
        // private float skill_delay_used_time = -1f;        
        // private float skill_uv_flag = 0.25f;

        //--------------------------------------------------waterwave
        public bool enable_waterwave = false;

        public float wwDistanceFactor = 60.0f;
        public float wwTimeFactor = -15.0f;
        public float wwTotalFactor = 1.0f;
        public float wwCenterFactor = 1.0f;

        public float wwLifeTime = 2f; //总时间
        public float wwDelayTime = 0.2f;// 延时播放时间
        [HideInInspector]
        public float wwWaveStartTime; // 开始时间
        [HideInInspector]
        public bool wwIsTimeLimitWater = false;// 是否是限时波纹

        //--------------------------------------------------screendistortion
        public bool enable_screendistortion = false;

        //public float sdDistanceFactor = 100.0f;
        public float sdTimeFactor = 0.5f;
        public float sdTotalFactor = 0.3f;
        public float sdCenterFactor = 1.0f;
        public float sdAlphaFactor = 0.0f;
        public float sdMinAlpha = 0.0f;

        public float sdTimeFactor2 = 0.5f;
        public float sdTotalFactor2 = 0.3f;
        public float sdCenterFactor2 = 1.0f;
        public float sdAlphaFactor2 = 0.0f;
        public float sdMinAlpha2 = 0.0f;

        public float sdTotalTime = 1.0f;
        private float sdCurrentTime = 0.0f;


        //--------------------------------------------------verticalfog
        // public bool enable_verticalfog = false;
        // public Color verticalFogColor = Color.red;
        // public float verticalFogStart = 4.32f;
        // public float verticalFogDensity = 0.5f;
        // private bool is_verticalfog = false;    

        //--------------------------------------------------screenreduce
        public bool enable_screenreduce = false;
        public float screenreduce_delay_time = 0f;
        public float screenreduce_time = 0.2f;
        public float screenreduce_lifetime = 0.2f;
        public float screenreduce_min = 0.2f;
        public Color screenreduce_down_color = Color.black;
        public float screenreduce_down = 0.75f;
        public float screenreduce_duration = 0.0f;

        private bool is_screenreduce = false;
        private float screenreduce_used_time = 0f;
        private float screenreduce_delay_used_time = 0f;
        private float screenreduce = 1.0f;
        private Color screenreduce_color = Color.black;
        private bool screenreduce_apply_flag = true;

        //--------------------------------------------------filtermirror
        public bool enable_filtermirror = false;
        public FilterMirror filterMirror = FilterMirror.OldPic;
        public enum FilterMirror
        {
            Defalut,
            OldPic,             // 老照片
            ABaoColor,          // 阿宝色
            Blue,               // 蓝色调
            Green,              // 绿色调
            HuangHun,           // 黄昏
            BangWan,            // 傍晚
            Night,             // 夜晚
            BlackWhite,         // 黑白
            Snooper,            // 夜视镜
            Carve,              // 浮雕
        }
        [HideInInspector]
        public Color _filterColor = Color.white;
        [HideInInspector]
        public float _filterIntersity = 1.0f;
        [HideInInspector]
        public Color _StandardColor = Color.white;

        public static List<XMLPostEffects> postEffectList = new List<XMLPostEffects>();

        private HighlightingSystem.HighlightingRenderer m_effectMask = null;
        private CamerDepthManager m_depthManager = null;
        private MaterialFactory m_MaterialFactory;
        private RenderTextureFactory m_RenderTextureFactory;
        private XMLPostEffectsContext m_Context;
        private Camera m_Camera;
        private bool bInitBloomConfig = false;

        private BloomModel m_BloomModel;
        private BloomComponent m_Bloom;

        private FxaaComponent m_Fxaa;
        private FxaaModel m_FxaaModel;

        private DistortionModel m_DistortionModel;
        private DistortionComponent m_Distortion;

        private ColorBalanceModel m_ColorBalanceModel;
        private ColorBalanceComponent m_ColorBalance;

        private DepthOfFieldModel m_DepthOfFieldModel;
        private DepthOfFieldComponent m_DepthOfField;

        private MotionBlurModel m_MotionBlurModel;
        private MotionBlurComponent m_MotionBlur;

        private WaterWaveModel m_WaterWaveModel;
        private WaterWaveComponent m_WaterWave;

        private ScreenDistortionModel m_ScreenDistortionModel;
        private ScreenDistortionComponent m_ScreenDistortion;

        private SkillBlurComponent m_SkillBlur;
        private SkillBlurModel m_SkillBlurModel;        

        private VerticalFogComponent m_VerticalFog;
        private VerticalFogModel m_VerticalFogModel;

        private ScreenReduceComponent m_ScreenReduce;
        private ScreenReduceModel m_ScreenReduceModel;

        private ColorGradingComponent m_ColorGrading;
        private ColorGradingModel m_ColorGradingModel;

        private ScreenBlurComponent m_ScreenBlur;
        private ScreenBlurModel m_ScreenBlurModel;

        private ToneMappingComponent m_ToneMapping;
        private ToneMappingModel m_ToneMappingModel;

        private FilterColorModel m_FilterColorModel;
        private FilterColorComponent m_FilterColor;

        private void Awake()
        {
            m_depthManager = gameObject.AddComponent<CamerDepthManager>();
        }

        void OnEnable()
        {
#if CLIENT
            var mainCamera = Global.MainCamera;
            var thisCamera = GetComponent<Camera>();
 			 var ui3dCamera = Global.UICamera3D;
            if (!((thisCamera == mainCamera || thisCamera == ui3dCamera) && postEffectList.Contains(this)))
            {
                postEffectList.Add(this);
            }

            if (mainCamera)
            {
                XMLPostEffects mainCameraPostEffect = mainCamera.GetComponent<XMLPostEffects>();
                if (thisCamera != mainCamera && this.gameObject.layer != LayerMask.NameToLayer("UI2D") && this.gameObject.layer != LayerMask.NameToLayer("UI3D"))
                {
                    thisCamera.allowHDR = mainCamera.allowHDR;
                    thisCamera.allowMSAA = mainCamera.allowMSAA;
                    if (mainCameraPostEffect)
                    {
                        this.enable_antialiasing = mainCameraPostEffect.enable_antialiasing;
                        this.enable_bloom = mainCameraPostEffect.enable_bloom;
                    }
                }
            }

#endif

            m_MaterialFactory = new MaterialFactory();
            m_RenderTextureFactory = new RenderTextureFactory();
            m_Context = new XMLPostEffectsContext();

            m_Bloom = new BloomComponent();
            m_BloomModel = new BloomModel();

            m_Fxaa = new FxaaComponent();
            m_FxaaModel = new FxaaModel();

            m_Distortion = new DistortionComponent();
            m_DistortionModel = new DistortionModel();

            m_ColorBalance = new ColorBalanceComponent();
            m_ColorBalanceModel = new ColorBalanceModel();

            m_DepthOfField = new DepthOfFieldComponent();
            m_DepthOfFieldModel = new DepthOfFieldModel();

            m_MotionBlur = new MotionBlurComponent();
            m_MotionBlurModel = new MotionBlurModel();

            m_WaterWave = new WaterWaveComponent();
            m_WaterWaveModel = new WaterWaveModel();

            m_ScreenDistortionModel = new ScreenDistortionModel();
            m_ScreenDistortion = new ScreenDistortionComponent();

            m_SkillBlur = new SkillBlurComponent();
            m_SkillBlurModel = new SkillBlurModel();

            m_VerticalFog = new VerticalFogComponent();
            m_VerticalFogModel = new VerticalFogModel();

            m_ScreenReduce = new ScreenReduceComponent();
            m_ScreenReduceModel = new ScreenReduceModel();

            m_ColorGrading = new ColorGradingComponent();
            m_ColorGradingModel = new ColorGradingModel();

            m_ScreenBlur = new ScreenBlurComponent();
            m_ScreenBlurModel = new ScreenBlurModel();

            m_ToneMapping = new ToneMappingComponent();
            m_ToneMappingModel = new ToneMappingModel();

            m_FilterColor = new FilterColorComponent();
            m_FilterColorModel = new FilterColorModel();

            m_effectMask = GetComponent<HighlightingSystem.HighlightingRenderer>();
          
            ShaderPropertyID.Initialize();

            if(EnableEvent != null)
                EnableEvent();
        }

        void OnPreCull()
        {
            // All the per-frame initialization logic has to be done in OnPreCull instead of Update
            // because [ImageEffectAllowedInSceneView] doesn't trigger Update events...

            m_Camera = GetComponent<Camera>();

            if (m_Camera == null)
                return;

            // Prepare context
            var context = m_Context.Reset();
            context.renderTextureFactory = m_RenderTextureFactory;
            context.materialFactory = m_MaterialFactory;
            context.camera = m_Camera;

            //bloom
            //bloom_threshold = 0.001f;

            if (enable_screenblur)
            {
                m_BloomModel.threshold_role = 0;
                m_BloomModel.threshold_scene = 0;
                m_BloomModel.threshold_effect_high = 0;
                m_BloomModel.threshold_effect_low = 0;
                m_BloomModel.threshold_roleEmission = 0;
                m_BloomModel.threshold_effectNo = 0;
                m_BloomModel.intensity_role = 1;
                m_BloomModel.intensity_scene = 1;
                m_BloomModel.intensity_effect_high = 1;
                m_BloomModel.intensity_effect_low = 1;
                m_BloomModel.intensity_roleEmission = 1f;
                m_BloomModel.intensity_effectNo = 1;
                m_BloomModel.bloom_reduce = 1.0f;
                m_BloomModel.offset = 1.0f;
                m_BloomModel.renderCount = 1;
                m_BloomModel.bloom_threshold = bloom_threshold;
                m_BloomModel.bloom_enhance = bloom_enhance;
                m_BloomModel.screen_reduce = screen_reduce;                
                m_BloomModel.enabled = enable_bloom;
            }
            else
            {
                if (m_Bloom.isHdr)
                {
                    m_BloomModel.threshold_role = threshold_role;
                    m_BloomModel.threshold_scene = threshold_scene;
                    m_BloomModel.threshold_effect_high = threshold_effect_high;
                    m_BloomModel.threshold_effect_low = threshold_effect_low;
                    m_BloomModel.threshold_roleEmission = 0;
                    m_BloomModel.threshold_effectNo = 0;
                    m_BloomModel.intensity_role = intensity_role;
                    m_BloomModel.intensity_scene = intensity_scene;
                    m_BloomModel.intensity_effect_high = intensity_effect_high;
                    m_BloomModel.intensity_effect_low = intensity_effect_low;
                    m_BloomModel.intensity_roleEmission = intensity_roleEemission;
                    m_BloomModel.intensity_effectNo = 0;

                    m_BloomModel.bloom_reduce = bloom_reduce;
                    m_BloomModel.offset = bloom_offset;
                    m_BloomModel.renderCount = bloomRenderCount;
                    m_BloomModel.bloom_threshold = bloom_threshold;
                    m_BloomModel.bloom_enhance = bloom_enhance;
                    m_BloomModel.screen_reduce = screen_reduce;
                    m_BloomModel.enabled = enable_bloom;
                }
                else
                {
                    m_BloomModel.threshold_role = threshold_role_nohdr;
                    m_BloomModel.threshold_scene = threshold_scene_nohdr;
                    m_BloomModel.threshold_effect_high = threshold_effect_high_nohdr;
                    m_BloomModel.threshold_effect_low = threshold_effect_low_nohdr;
                    m_BloomModel.threshold_roleEmission = 0;
                    m_BloomModel.threshold_effectNo = 0;
                    m_BloomModel.intensity_role = intensity_role_nohdr;
                    m_BloomModel.intensity_scene = intensity_scene_nohdr;
                    m_BloomModel.intensity_effect_high = intensity_effect_high_nohdr;
                    m_BloomModel.intensity_effect_low = intensity_effect_low_nohdr;
                    m_BloomModel.intensity_roleEmission = intensity_roleEemission_nohdr;
                    m_BloomModel.intensity_effectNo = 0;

                    m_BloomModel.bloom_reduce = bloom_reduce_nohdr;
                    m_BloomModel.offset = bloom_offset_nohdr;
                    m_BloomModel.renderCount = bloomRenderCount;
                    m_BloomModel.bloom_threshold = bloom_threshold;
                    m_BloomModel.bloom_enhance = bloom_enhance;
                    m_BloomModel.screen_reduce = screen_reduce;
                    m_BloomModel.enabled = enable_bloom;
                }
            }

            m_Bloom.Init(context, m_BloomModel);

            //fxaa
            m_FxaaModel.preset = (int)FXAAPreset;
            m_FxaaModel.enabled = enable_antialiasing;
            m_Fxaa.Init(context, m_FxaaModel);

            //distortion
            m_DistortionModel.enabled = enable_distortion;
            m_Distortion.Init(context, m_DistortionModel);

            //colorbalance
            //m_ColorBalanceModel.enabled = enable_colorbalance;
            //m_ColorBalanceModel.contrast = contrast;
            //m_ColorBalanceModel.saturation = saturation;
            //m_ColorBalance.Init(context, m_ColorBalanceModel);

            //depthoffield
            m_DepthOfFieldModel.enabled = enable_depthoffield;
            m_DepthOfFieldModel.camera = m_Camera;
            m_DepthOfFieldModel.focalDistance = focalDistance;
            m_DepthOfFieldModel.nearBlurScale = 0f;
            m_DepthOfFieldModel.farBlurScale = farBlurScale;
            m_DepthOfFieldModel.downSample = 1;
            m_DepthOfFieldModel.samplerScale = 1;
            m_DepthOfFieldModel.focalObj = focalObj;
            m_DepthOfFieldModel.transform = transform;
            m_DepthOfField.Init(context, m_DepthOfFieldModel);

            //motionblur
            m_MotionBlurModel.enabled = enable_motionblur;
            m_MotionBlur.Init(context, m_MotionBlurModel);

            //skillblur
            //m_SkillBlurModel.enabled = enable_skillblur;
            //m_SkillBlur.Init(context, m_SkillBlurModel);

            //waterwave
#if CLIENT
            m_WaterWaveModel.enabled = enable_waterwave && SettingManager.waterWaveEnable;
#else
            m_WaterWaveModel.enabled = enable_waterwave;
#endif
            m_WaterWave.Init(context, m_WaterWaveModel);

            //screendistortion
            m_ScreenDistortionModel.enabled = enable_screendistortion;
            m_ScreenDistortion.Init(context, m_ScreenDistortionModel);

            //verticalfog
            // m_VerticalFogModel.enabled = enable_verticalfog;
            // m_VerticalFogModel.camera = m_Camera;
            // m_VerticalFogModel.verticalFogColor = verticalFogColor;
            // m_VerticalFogModel.verticalFogStart = verticalFogStart;
            // m_VerticalFogModel.verticalFogDensity = verticalFogDensity;
            // m_VerticalFog.Init(context, m_VerticalFogModel);

            //screenreduce
            m_ScreenReduceModel.enabled = enable_screenreduce;
            m_ScreenReduce.Init(context, m_ScreenReduceModel);

            //colorgrading
            // m_ColorGradingModel.enabled = enable_colorgrading;
            // m_ColorGradingModel.sharpness = sharpness;
            // m_ColorGradingModel.sharpness_kernal = sharpness_kernel;
            // m_ColorGrading.Init(context, m_ColorGradingModel);

            //tonemapping
            m_ToneMappingModel.enabled = enable_tonemapping & enable_bloom;
            m_ToneMapping.Init(context, m_ToneMappingModel);


            m_FilterColorModel.enabled = enable_filtermirror;
            m_FilterColorModel.fmMode = filterMirror;
            m_FilterColor.Init(context, m_FilterColorModel);
        }


        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
#if UNITY_EDITOR
            if (show_effectrt_for_test && m_effectMask)
            {
                Graphics.Blit(m_effectMask.effectRT, destination);
                return;
            }
            if (show_depthrt_for_test)
            {
                var depthMaterial = m_MaterialFactory.Get("Hidden/PostEffects/Depth");
                Graphics.Blit(source, destination, depthMaterial);
                return;
            }
#endif
            if (m_Camera == null)
            {
                Graphics.Blit(source, destination);
                return;
            }

            bool uberActive = false;
            bool fxaaActive = m_Fxaa.active;

            var uberMaterial = m_MaterialFactory.Get("Hidden/PostEffects/Uber");
            uberMaterial.shaderKeywords = null;

            var src = source;
            var dst = destination;

            RenderTexture effectRT = null;
            bool needDistortion = false;
            if (m_effectMask)
            {
                effectRT = m_effectMask.effectRT;
                needDistortion = m_effectMask.needDistortion;
            }

            if (/*m_VerticalFog.active || */m_DepthOfField.active)
            {
               if (m_depthManager)
                   m_depthManager.OutSideSetDepthMode(DepthTextureMode.Depth);
            }
            else
            {
               if (m_depthManager)
                   m_depthManager.OutSidRestoreDepthMode();
            }

            if (enable_depth_for_test && m_depthManager)
                m_depthManager.OutSideSetDepthMode(DepthTextureMode.Depth);

            if (m_DepthOfField.active)
            {
                uberActive = true;
                m_DepthOfField.Prepare(src, uberMaterial);
            }


            if (m_Distortion.active && needDistortion)
            {
                uberActive = true;
                uberMaterial.SetTexture(ShaderPropertyID._DistortionTex, effectRT);
                uberMaterial.EnableKeyword("DISTORTION");
            }

            if (m_Bloom.active)
            {
                uberActive = true;
                if (enable_screenblur)
                    m_Bloom.Prepare(src, uberMaterial, effectRT, BlurType.screenBlur);
                else
                    m_Bloom.Prepare(src, uberMaterial, effectRT, BlurType.bloomBlur);

                if (enable_tonemapping)
                {
                    if (tm_mode == ToneMapping.FILMIC)
                    {
                        uberMaterial.SetVector(ShaderPropertyID._ToneMapping_Settings1, new Vector4(tm_filmic_exposure, tm_filmic_luminance, 0, 0));
                        uberMaterial.EnableKeyword("TONEMAPPING_FILMIC");
                    }
                    else
                    {
                        uberMaterial.SetVector(ShaderPropertyID._ToneMapping_Settings1, new Vector4(tm_aces_exposure, tm_aces_luminance, 0, 0));
                        uberMaterial.EnableKeyword("TONEMAPPING_ACES");
                    }
                }
            }

            // if (m_ColorGrading.active)
            // {
            //     uberActive = true;
            //     m_ColorGrading.Prepare(src, uberMaterial);
            // }

            //if (m_ColorBalance.active)
            //{
            //    uberActive = true;
            //    m_ColorBalance.Prepare(src, uberMaterial);
            //}


            if (m_MotionBlur.active)
            {
                if (!is_motion)
                {
                    is_motion = true;
                }

                if (is_motion && motion_used_time > 0)
                {
                    uberActive = true;
                    //uberMaterial.SetFloat(ShaderPropertyID._motionFactor, motion_run_factor);
                    uberMaterial.EnableKeyword("MOTIONBLUR");
                }
            }
            else
            {
                motion_used_time = 0;
                motion_delay_used_time = 0;
                is_motion = false;
            }

            /*
            if (m_SkillBlur.active && !m_MotionBlur.active)
            {
                if (!is_skill)
                {
                    is_skill = true;
                }

                if (is_skill && skill_used_time > 0)
                {
                    uberActive = true;
                    uberMaterial.SetVector(ShaderPropertyID._skillUV, skill_run_uv);
                    uberMaterial.SetFloat(ShaderPropertyID._skillSaturation, skill_run_saturation);
                    uberMaterial.SetFloat(ShaderPropertyID._skillAlpha, skill_run_alpha);
                    uberMaterial.EnableKeyword("SKILLBLUR");
                }
            }
            else
            {
                skill_used_time = 0;
                skill_delay_used_time = 0;
                is_skill = false;
            }
            */

            if (m_ScreenReduce.active)
            {
                is_screenreduce = true;

                if (is_screenreduce && screenreduce_used_time > 0)
                {
                    Shader.SetGlobalFloat(ShaderPropertyID._screenReduce, screenreduce);
                    Shader.SetGlobalColor(ShaderPropertyID._screenReduceColor, screenreduce_color);
                    screenreduce_apply_flag = true;
                }
            }
            else
            {
                screenreduce_used_time = 0;
                screenreduce_delay_used_time = 0;
                if (!screenreduce_apply_flag || is_screenreduce)                
                {
                    is_screenreduce = false;
                    screenreduce = 1.0f;
                    Shader.SetGlobalFloat(ShaderPropertyID._screenReduce, screenreduce);
                    Shader.SetGlobalColor(ShaderPropertyID._screenReduceColor, Color.black);
                    screenreduce_apply_flag = true;
                }
            }

            if (m_WaterWave.active)
            {
                uberActive = true;

                if (wwIsTimeLimitWater)
                {
                    if (Time.time - wwWaveStartTime >= wwDelayTime)
                    {
                        uberMaterial.SetVector(ShaderPropertyID._wwFactor, new Vector4(wwDistanceFactor, wwTimeFactor, wwTotalFactor, wwCenterFactor));
                        if (Time.time - wwWaveStartTime > wwLifeTime)
                            enable_waterwave = false;

                        uberMaterial.EnableKeyword("WATERWAVE");
                    }
                }
                else
                {
                    uberMaterial.SetVector(ShaderPropertyID._wwFactor, new Vector4(wwDistanceFactor, wwTimeFactor, wwTotalFactor, wwCenterFactor));
                    uberMaterial.EnableKeyword("WATERWAVE");
                }
            }
#if CLIENT
            if (m_ScreenDistortion.active)
            {
                sdCurrentTime += Time.deltaTime;
                sdCurrentTime = Math.Min(sdCurrentTime, sdTotalTime);
                float precent = sdTotalTime == 0 ? 1 : sdCurrentTime / sdTotalTime;

                uberActive = true;
                uberMaterial.SetVector(ShaderPropertyID._sdFactor, new Vector4(Global.GetFloatIntervalValue(sdAlphaFactor, sdAlphaFactor2, precent),
                                                                                Global.GetFloatIntervalValue(sdTimeFactor, sdTimeFactor2, precent),
                                                                                Global.GetFloatIntervalValue(sdTotalFactor, sdTotalFactor2, precent),
                                                                                Global.GetFloatIntervalValue(sdCenterFactor, sdCenterFactor2, precent)));
                uberMaterial.SetFloat(ShaderPropertyID._minAlpha, Global.GetFloatIntervalValue(sdMinAlpha, sdMinAlpha2, precent));
                uberMaterial.EnableKeyword("SCREENDISTORTION");
            }
            else
            {
                sdCurrentTime = 0.0f;
            }
#endif


            if (enable_filtermirror)
            {
                uberActive = true;
                m_FilterColor.Prepare(uberMaterial, _filterColor, _StandardColor, _filterIntersity);
            }


            // if (m_VerticalFog.active)
            // {
            //     uberActive = true;
            //     m_VerticalFog.Prepare(src, uberMaterial);
            //     if (!is_verticalfog)
            //     {
            //         is_verticalfog = true;
            //     }
            // }
            // else
            // {
            //     if (is_verticalfog)
            //     {
            //         is_verticalfog = false;
            //     }
            // }

            if (fxaaActive)
            {
                if (uberActive)
                {
                    var output = m_RenderTextureFactory.Get(src);
                    Graphics.Blit(src, output, uberMaterial, 0);
                    src = output;
                }

                m_Fxaa.Render(src, dst);
            }
            else
            {
                if (uberActive)
                {
                    Graphics.Blit(src, dst, uberMaterial, 0);
                }
            }

            if (!uberActive && !fxaaActive)
                Graphics.Blit(src, dst);

            m_RenderTextureFactory.ReleaseAll();
        }

        void OnDisable()
        {
#if CLIENT
            //如果是主摄像机和ui3d摄像机则不移除
            var mainCamera = Global.MainCamera;
            var ui3dCamera = Global.UICamera3D;
            var thisCamera = GetComponent<Camera>();
            if (thisCamera != mainCamera || thisCamera != ui3dCamera){
                postEffectList.Remove(this);
            }
#endif
            m_MaterialFactory.Dispose();
            m_RenderTextureFactory.Dispose();
            GraphicsUtils.Dispose();
#if CLIENT
            if (m_Camera == Global.MainCamera)
            {
                screenreduce = 1.0f;
                Shader.SetGlobalFloat(ShaderPropertyID._screenReduce, screenreduce);
                Shader.SetGlobalColor(ShaderPropertyID._screenReduceColor, Color.black);
            }
#endif
            if(DisableEvent != null)
                DisableEvent();
        }

        public void DisableAllPostEffect()
        {
            enable_antialiasing = false;
            enable_bloom = false;
            enable_screenblur = false;
            enable_tonemapping = false;
            //enable_colorbalance = false;
            //enable_colorgrading = false;
            enable_depthoffield = false;
            enable_distortion = false;
            enable_motionblur = false;
            enable_screenreduce = false;
            //enable_skillblur = false;
            //enable_verticalfog = false;
            enable_waterwave = false;
            enable_screendistortion = false;
            enable_filtermirror = false;
        }

        void Update()
        {
#if CLIENT
            if (Global.gameLua.ENU == null)
                return;
            LoadBloomEffect();

            if (is_screenreduce)
            {
                screenreduce_delay_used_time += Time.deltaTime;

                if (screenreduce_delay_used_time >= screenreduce_delay_time)
                {
                    screenreduce_used_time += Time.deltaTime;
                    if (screenreduce_used_time >= screenreduce_time)
                    {
                        enable_screenreduce = false;
                        is_screenreduce = false;
                    }
                    else if (screenreduce_used_time >= screenreduce_time * (screenreduce_down + screenreduce_duration))
                    {
                        float f = (screenreduce_used_time - screenreduce_time * (screenreduce_down + screenreduce_duration)) / (screenreduce_time - screenreduce_time * (screenreduce_down + screenreduce_duration));
                        screenreduce = screenreduce_min + (1 - screenreduce_min) * f;
                        screenreduce_color = Color.Lerp(screenreduce_down_color, Color.black, f);
                    }
                    else
                    {
                        float f = screenreduce_used_time / (screenreduce_time * screenreduce_down);
                        f = f >= 1 ? 1 : f;
                        screenreduce = 1 - (1 - screenreduce_min) * f;
                        screenreduce_color = Color.Lerp(Color.black, screenreduce_down_color, f);
                    }
                    screenreduce_apply_flag = false;
                }
            }


            // if (is_skill)
            // {
            //     skill_delay_used_time += Time.deltaTime;

            //     if (skill_delay_used_time >= skill_delay_time)
            //     {
            //         skill_used_time += Time.deltaTime;
            //         if (skill_used_time >= skill_time)
            //         {
            //             enable_skillblur = false;
            //             is_skill = false;
            //         }
            //         else if (skill_used_time >= skill_time * skill_uv_flag)
            //         {
            //             float f = (skill_used_time - skill_time * skill_uv_flag) / (skill_time - skill_time * skill_uv_flag);
            //             skill_run_saturation = 1 + f/2;
            //             skill_run_uv.x = skill_uv_x * skill_uv_flag;
            //             skill_run_uv.y = skill_uv_y * skill_uv_flag;
            //         }
            //         else
            //         {
            //             skill_run_saturation = 1;
            //             skill_run_uv.x = skill_uv_x;
            //             skill_run_uv.y = skill_uv_y;
            //         }
            //     }
            // }


            if (is_motion)
            {
                motion_delay_used_time += Time.deltaTime;
                if (motion_delay_used_time >= motion_delay_time)
                {
                    motion_used_time += Time.deltaTime;
                    if (motion_used_time >= motion_time)
                    {
                        enable_motionblur = false;
                        is_motion = false;
                    }
                }
            }
#endif
        }

        public void LoadBloomEffect()
        {
#if CLIENT
            if (bInitBloomConfig)
                return;
            bInitBloomConfig = true;

            //SystemInformation.GetSystemInfo();

            string phoneName = string.Empty;
#if UNITY_IOS
            phoneName = "iphone";
#elif UNITY_ANDROID
            phoneName = "andriod";
#else
            phoneName = "andriod";
#endif
            Global.BloomEffectInfo bloomInfo = Global.GetBloomEffectXmlNode(phoneName);
            if (bloomInfo == null)
                return;

            threshold_role = bloomInfo.threshold_role;
            threshold_scene = bloomInfo.threshold_scene;
            threshold_effect_high = bloomInfo.threshold_effect_high;
            threshold_effect_low = bloomInfo.threshold_effect_low;
            intensity_role = bloomInfo.intensity_role;
            intensity_scene = bloomInfo.intensity_scene;
            intensity_effect_high = bloomInfo.intensity_effect_high;
            intensity_effect_low = bloomInfo.intensity_effect_low;
            bloom_reduce = bloomInfo.bloom_reduce;
            bloom_offset = bloomInfo.bloom_offset;

            threshold_role_nohdr = bloomInfo.threshold_role_nohdr;
            threshold_scene_nohdr = bloomInfo.threshold_scene_nohdr;
            threshold_effect_high_nohdr = bloomInfo.threshold_effect_high_nohdr;
            threshold_effect_low_nohdr = bloomInfo.threshold_effect_low_nohdr;
            intensity_role_nohdr = bloomInfo.intensity_role_nohdr;
            intensity_scene_nohdr = bloomInfo.intensity_scene_nohdr;
            intensity_effect_high_nohdr = bloomInfo.intensity_effect_high_nohdr;
            intensity_effect_low_nohdr = bloomInfo.intensity_effect_low_nohdr;
            bloom_reduce_nohdr = bloomInfo.bloom_reduce_nohdr;
            bloom_offset_nohdr = bloomInfo.bloom_offset_nohdr;
#endif
        }
    }
}