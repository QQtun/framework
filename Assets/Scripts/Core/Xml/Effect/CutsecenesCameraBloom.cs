//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using HighlightingSystem;

//using XMLGame.Effect.PostEffects;
//#if CLIENT
//using XMLEngine.GameEngine.Logic;
//#endif
//namespace CinemaDirector
//{
//    public class CutsecenesCameraBloom : MonoBehaviour
//    {
//        public bool setToCamera = true;
//        public bool addCameraInfo = true;
//        public bool keepHdrSeting = false;
//        /// <summary>
//        /// 后处理
//        /// </summary>
//        //private Shader postEffectsShader;

//        void Awake()
//        {

//            Camera CutCamera = this.GetComponent<CinemaShot>().shotCamera;
//            //postEffectsShader = Shader.Find("Xml/ImageEffect/PostEffects");
//            if (CutCamera != null)
//            {
//                if (setToCamera)
//                {
//                    if(CutCamera.gameObject.GetComponent<HighlightingRenderer>() == null)
//                        CutCamera.gameObject.AddComponent<HighlightingRenderer>();
//                    XMLPostEffects xmlpe = CutCamera.gameObject.AddComponent<XMLPostEffects>();
//#if CLIENT
//                    XMLPostEffects MainCameraPostEff = Global.MainCamera.GetComponent<XMLPostEffects>();
//#else
//                    XMLPostEffects MainCameraPostEff = null;
//#endif
//                    if (MainCameraPostEff != null)
//                    {
//                        xmlpe.threshold_role = MainCameraPostEff.threshold_role;
//                        xmlpe.threshold_scene = MainCameraPostEff.threshold_scene;
//                        xmlpe.threshold_effect_high = MainCameraPostEff.threshold_effect_high;
//                        xmlpe.threshold_effect_low = MainCameraPostEff.threshold_effect_low;
//                        xmlpe.threshold_roleEmission = MainCameraPostEff.threshold_roleEmission;
//                        xmlpe.threshold_effectNo = MainCameraPostEff.threshold_effectNo;
//                        xmlpe.intensity_role = MainCameraPostEff.intensity_role;
//                        xmlpe.intensity_scene = MainCameraPostEff.intensity_scene;
//                        xmlpe.intensity_effect_high = MainCameraPostEff.intensity_effect_high;
//                        xmlpe.intensity_effect_low = MainCameraPostEff.intensity_effect_low;
//                        xmlpe.intensity_roleEemission = MainCameraPostEff.intensity_roleEemission;
//                        xmlpe.intensity_effectNo = MainCameraPostEff.intensity_effectNo;
//                        xmlpe.bloom_reduce = MainCameraPostEff.bloom_reduce;
//                        xmlpe.bloom_offset = MainCameraPostEff.bloom_offset;
//                        xmlpe.threshold_role_nohdr = MainCameraPostEff.threshold_role_nohdr;
//                        xmlpe.threshold_scene_nohdr = MainCameraPostEff.threshold_scene_nohdr;
//                        xmlpe.threshold_effect_high_nohdr = MainCameraPostEff.threshold_effect_high_nohdr;
//                        xmlpe.threshold_effect_low_nohdr = MainCameraPostEff.threshold_effect_low_nohdr;
//                        xmlpe.threshold_roleEmission_nohdr = MainCameraPostEff.threshold_roleEmission_nohdr;
//                        xmlpe.threshold_effectNo_nohdr = MainCameraPostEff.threshold_effectNo_nohdr;
//                        xmlpe.intensity_role_nohdr = MainCameraPostEff.intensity_role_nohdr;
//                        xmlpe.intensity_scene_nohdr = MainCameraPostEff.intensity_scene_nohdr;
//                        xmlpe.intensity_effect_high_nohdr = MainCameraPostEff.intensity_effect_high_nohdr;
//                        xmlpe.intensity_effect_low_nohdr = MainCameraPostEff.intensity_effect_low_nohdr;
//                        xmlpe.intensity_roleEemission_nohdr = MainCameraPostEff.intensity_roleEemission_nohdr;
//                        xmlpe.intensity_effectNo_nohdr = MainCameraPostEff.intensity_effectNo_nohdr;
//                        xmlpe.bloom_reduce_nohdr = MainCameraPostEff.bloom_reduce_nohdr;
//                        xmlpe.bloom_offset_nohdr = MainCameraPostEff.bloom_offset_nohdr;

//                        xmlpe.bloomRenderCount = MainCameraPostEff.bloomRenderCount;
//                        xmlpe.bloom_threshold = MainCameraPostEff.bloom_threshold;
//                        xmlpe.bloom_enhance = MainCameraPostEff.bloom_enhance;
//                        xmlpe.screen_reduce = MainCameraPostEff.screen_reduce;
//                    }

//                }
//                if (addCameraInfo)
//                {
//                    CameraInfo cameinfo = CutCamera.gameObject.AddComponent<CameraInfo>();
//                    cameinfo.cameraType = CameraInfo.CameraType.movie;
//                    cameinfo.keepHdrSetting = keepHdrSeting;
//                }
//                CutCamera.cullingMask &= ~(1 << 16);
//                CutCamera.cullingMask &= ~(1 << 20);
//                CutCamera.cullingMask &= ~(1 << 21);
//            }
//        }

//    }
//}
