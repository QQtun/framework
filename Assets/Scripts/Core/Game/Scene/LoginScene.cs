using System.Collections.Generic;
using Core.Framework.Scene;
using Core.Framework.UI;
using Core.Game.UI;
using UnityEngine;
//using UnityEngine.Rendering.Universal;

namespace Core.Game.Scene
{
    public class LoginScene : MonoBehaviour, ISceneEntry
    {
        private void Awake()
        {
            SceneManager.Instance.AddEntry(this, true);
        }

        private void OnDestroy()
        {
            if(SceneManager.Instance != null)
                SceneManager.Instance.RemoveEntry(this);
        }

        public void Init()
        {
            UIManager.Instance.Open(UIName.LoginUI);

            //var cameraData = Camera.main.GetUniversalAdditionalCameraData();
            //cameraData.cameraStack.Add(UIManager.Instance.uiCamera);
        }

        public void OnEntering(string from, string to, Dictionary<string, object> param)
        {
            // no use
        }

        public int OnEnteringProcess(string from, string to, Dictionary<string, object> param)
        {
            if (UIManager.Instance.IsOpened(UIName.LoginUI))
                return 100;
            return 0;
        }
    }

}