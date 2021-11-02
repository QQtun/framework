using UnityEngine;
using Core.Framework.Utility;
using Core.Framework.Lua;
using UnityEngine.Serialization;

namespace Core.Framework.UI
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Canvas))]
    public class UIRoot : MonoBehaviour
    {
        public const int PlaneDistance = 500;
        public const int PlaneDistanceClose = 10000;

        [FormerlySerializedAs("Canvas")]
        public Canvas canvas;
        [FormerlySerializedAs("UILayer")]
        public UILayer uiLayer;
        [FormerlySerializedAs("CloseWhenSceneChange")]
        public bool closeWhenSceneChange;
        [FormerlySerializedAs("CloseMainCamera")]
        public bool closeMainCamera;
        [FormerlySerializedAs("RenderSetting")]
        public RenderSettingsAsset renderSetting;

        private SimpleLuaBehaviour mLuaBehaviour;

        public SimpleLuaBehaviour LuaBehaviour => mLuaBehaviour;

        private void Awake()
        {
            canvas = GetComponent<Canvas>();
            mLuaBehaviour = GetComponent<SimpleLuaBehaviour>();
            canvas.sortingLayerID = (int)uiLayer;
        }

        public virtual void OnOpen(string name, object param)
        {
            canvas.planeDistance = PlaneDistance;

            if(mLuaBehaviour != null)
            {
                mLuaBehaviour.Set("mRoot", this);
                mLuaBehaviour.Set("mName", name);
                mLuaBehaviour.Set("mParam", param);
                mLuaBehaviour.TryInvokeFunc("OnOpen");
            }
        }

        public virtual void OnClose()
        {
            canvas.planeDistance = PlaneDistanceClose;

            if (mLuaBehaviour != null)
                mLuaBehaviour.TryInvokeFunc("OnClose");
        }
    }
}