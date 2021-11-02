//using System.Collections.Generic;
//using Core.Framework.Scene;
//using Core.Framework.UI;
//using UnityEngine.UI;

//namespace Core.Game.UI
//{
//    public class Loading : UILogicBase, ISceneEntry
//    {
//        private int mProgress;
//        private Text mProgressText;

//        public Loading(string name, UIRoot root) : base(name, root)
//        {
//            //mProgressText = root.GetObject("mProgressText").GetComponent<Text>();
//        }

//        public override void OnClose()
//        {
//            SceneManager.Instance.RemoveEntry(this);
//        }

//        public override void OnOpen(object param)
//        {
//            SceneManager.Instance.AddEntry(this);
//            mProgress = 0;
//            mProgressText.text = $"progress... {mProgress}%";
//        }

//        public void OnEntering(string from, string to, Dictionary<string, object> param)
//        {
//        }

//        public int OnEnteringProcess(string from, string to, Dictionary<string, object> param)
//        {
//            mProgress++;
//            mProgressText.text = $"progress... {mProgress}%";
//            return mProgress;
//        }
//    }
//}