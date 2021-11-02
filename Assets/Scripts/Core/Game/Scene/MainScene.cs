using System.Collections.Generic;
using Core.Framework.Scene;
using Core.Framework.UI;
using Core.Game.UI;
using UnityEngine;

namespace Core.Game.Scene
{
    public class MainScene : MonoBehaviour, ISceneEntry
    {
        public void OnEntering(string from, string to, Dictionary<string, object> param)
        {
            UIManager.Instance.Open(UIName.MainUI);
        }

        public int OnEnteringProcess(string from, string to, Dictionary<string, object> param)
        {
            return 100;
        }
    }

}