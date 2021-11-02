using Core.Framework.Scene;
using Core.Framework.UI;
using Core.Game.UI;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif 

public class GameApp : Singleton<GameApp>
{
    public void StartInit()
    {
        Application.targetFrameRate = 60;

        InitSceneControl();
        InitTableLoader();
        InitLeanTouch();
    }

    private void InitSceneControl()
    {
        SceneManager.Instance.OnStarted += (from, to, param) =>
        {
            UIManager.Instance.Open(UIName.Loading);
        };

        //SceneManager.Instance.OnEntering += (from, to, param) =>
        //{
        //    // TODO 
        //    if(to == "LoginScene")
        //    {
        //        var go = new GameObject("LoginSceneEntry");
        //        go.AddComponent<LoginScene>();
        //    }
        //    else if(to == "MainScene")
        //    {
        //        var go = new GameObject("MainSceneEntry");
        //        go.AddComponent<MainScene>();
        //    }
        //};

        SceneManager.Instance.OnFinished += (from, to, param) =>
        {
            UIManager.Instance.Close(UIName.Loading);
        };
    }

    private void InitTableLoader()
    {
        Core.Game.Table.TableGroup.Loader += (name, byteCB, jsonCB) =>
        {
#if UNITY_EDITOR
            var txtAsset = AssetDatabase.LoadAssetAtPath<TextAsset>($"Assets/PublicAssets/Table/{name}JsonData.bytes");
            jsonCB.Invoke(txtAsset.text);
#else
            Core.Framework.Res.ResourceManager.Instance.LoadAssetAsync<TextAsset>($"Assets/PublicAssets/Table/{name}ByteData.bytes",
                (asset) =>
                {
                    byteCB.Invoke(asset.bytes);
                });
#endif
        };
    }

    private void InitLeanTouch()
    {
        var prefab = Resources.Load<Lean.Touch.LeanTouch>("Prefabs/LeanTouch");
        var leanTouch = Instantiate(prefab);
        DontDestroyOnLoad(leanTouch.gameObject);
    }
}
