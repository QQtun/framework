using System;
using System.Collections.Generic;
using Core.Framework.Utility;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace Core.Framework.UI
{
    public enum UILayer
    {
        Bottom = 1,
        Main = 2,
        Window = 3,
        UnderLoading = 4,
        Loading = 5,
        AboveLoading = 6,
    }

    public delegate void OnUIOpen(string Name, object param);
    public delegate void OnUIClose(string Name);

    [SingletonPrefab("Prefabs/Singleton/UIManager")]
    public class UIManager : Singleton<UIManager>
    {
        private class UIOpen
        {
            public string Name;
            public object Param;
            public UIRoot Root;
        }

        public UnityEngine.Camera uiCamera;

        public event OnUIOpen OnUIOpen;
        public event OnUIClose OnUIClose;

        private List<UIOpen> mUICaches = new List<UIOpen>();
        private List<UIOpen>[] mOpenUIStack;
        private Transform mTransfromCache;
        private RenderSettingsAsset mRenderSettingCache;
        private UnityEngine.Camera mMainCameraCache;

        public bool IsOpened(string uiName)
        {
            var ui = GetUIByName(uiName);
            return ui != null;
        }

        public XLua.LuaTable GetUILuaCtrl(string uiName)
        {
            var ui = GetUIByName(uiName);
            return ui.Root.LuaBehaviour.GetLuaUIController();
        }

        private UIOpen GetTopWindow()
        {
            var windows = mOpenUIStack[(int)UILayer.Window];
            for (int j = windows.Count - 1; j >= 0; j--)
            {
                if (windows[j] != null)
                    return windows[j];
            }
            return null;
        }

        private UIOpen GetUIByName(string name)
        {
            for (int i = mOpenUIStack.Length - 1; i >= 0; i--)
            {
                var layerUIs = mOpenUIStack[i];
                if (layerUIs == null)
                    continue;
                for (int j = layerUIs.Count - 1; j >= 0; j--)
                {
                    var ui = layerUIs[j];
                    if (ui != null && ui.Name == name)
                        return ui;
                }
            }
            return null;
        }

        protected override void Init()
        {
            base.Init();
            mTransfromCache = transform;
            mRenderSettingCache = ScriptableObject.CreateInstance<RenderSettingsAsset>();
            var values = Enum.GetValues(typeof(UILayer));
            mOpenUIStack = new List<UIOpen>[values.Length + 1];
            for (int i = 0; i< values.Length;i++)
            {
                mOpenUIStack[(int)values.GetValue(i)] = new List<UIOpen>();
            }

            SceneManager.sceneUnloaded += (s) =>
            {
                for (int i = mUICaches.Count - 1; i >= 0; i--)
                {
                    var ui = mUICaches[i];
                    if (ui.Root.closeWhenSceneChange)
                    {
                        Addressables.ReleaseInstance(ui.Root.gameObject);
                        mUICaches.RemoveAt(i);
                    }
                }

                UIOpen oldTop = GetTopWindow();
                for (int i = mOpenUIStack.Length - 1; i >= 0; i--)
                {
                    var layerUIs = mOpenUIStack[i];
                    if (layerUIs == null)
                        continue;
                    for (int j = layerUIs.Count - 1; j >= 0; j--)
                    {
                        var ui = layerUIs[j];
                        if (ui.Root.closeWhenSceneChange)
                        {
                            Close(ui);
                            Addressables.ReleaseInstance(ui.Root.gameObject);
                            layerUIs.RemoveAt(j);
                        }
                    }
                }
                var newTop = GetTopWindow();
                if (newTop != null && newTop != oldTop)
                {
                    Open(newTop);
                }
            };
        }

        public void Open(string name, object param = null)
        {
            var ui = GetUIByName(name);
            if (ui != null)
            {
                if (ui.Root.uiLayer != UILayer.Window)
                    return;

                // in stack
                if (ui == GetTopWindow())
                {
                    // on top
                    return;
                }

                // not on top
                int closeCount = 0;
                var windows = mOpenUIStack[(int)UILayer.Window];
                for (int i = windows.Count - 1; i >= 0; i--)
                {
                    if (windows[i] == ui)
                    {
                        closeCount = windows.Count - (i + 1);
                        break;
                    }
                }

                // remove current top
                for (int i = 0; i < closeCount; i++)
                {
                    var topUI = windows[windows.Count - 1];
                    Close(topUI);
                    windows.Remove(topUI);
                    mUICaches.Add(topUI);
                }

                Open(ui);
                return;
            }

            var index = mUICaches.FindIndex(i => i.Name == name);
            if (index >= 0)
            {
                // has Cache
                var cacheUI = mUICaches[index];
                if(cacheUI.Root.uiLayer == UILayer.Window)
                {
                    // close current top
                    var windows = mOpenUIStack[(int)UILayer.Window];
                    if (windows.Count > 0)
                    {
                        var closeTop = windows[windows.Count - 1];
                        Close(closeTop);
                    }
                }

                mUICaches.RemoveAt(index);
                cacheUI.Param = param;
                mOpenUIStack[(int)cacheUI.Root.uiLayer].Add(cacheUI);
                Open(cacheUI);
                return;
            }

            string path = $"Assets/PublicAssets/UI/{name}.prefab";
            var handle = Addressables.InstantiateAsync(path);
            handle.Completed += (h) =>
            {
                var root = h.Result.GetComponent<UIRoot>();
                root.canvas.worldCamera = uiCamera;
                root.transform.SetParent(mTransfromCache, false);

                if(root.uiLayer == UILayer.Window)
                {
                    // close current top
                    var windows = mOpenUIStack[(int)UILayer.Window];
                    if (windows.Count > 0)
                    {
                        var closeTop = windows[windows.Count - 1];
                        Close(closeTop);
                    }
                }

                var openUI = new UIOpen();
                openUI.Name = name;
                openUI.Param = param;
                openUI.Root = root;
                mOpenUIStack[(int)openUI.Root.uiLayer].Add(openUI);

                Open(openUI);
            };
        }

        public void Close(string name)
        {
            var targetUI = GetUIByName(name);
            if (targetUI == null)
                return;

            bool isTopWindow = false;
            if (targetUI.Root.uiLayer == UILayer.Window)
            {
                isTopWindow = GetTopWindow() == targetUI;
            }

            mOpenUIStack[(int)targetUI.Root.uiLayer].Remove(targetUI);
            mUICaches.Add(targetUI);
            Close(targetUI);

            if (isTopWindow)
            {
                var windows = mOpenUIStack[(int)UILayer.Window];
                if (windows.Count > 0)
                {
                    // open previous
                    var nextTopUI = windows[windows.Count - 1];
                    Open(nextTopUI);
                }
                else
                {
                    if (mMainCameraCache != null)
                    {
                        mMainCameraCache.enabled = true;
                        mMainCameraCache = null;
                    }
                    if (!mRenderSettingCache.IsEmpty)
                    {
                        mRenderSettingCache.Apply();
                        mRenderSettingCache.Clear();
                    }
                }
            }
        }

        public void CloseTopWindow()
        {
            var windows = mOpenUIStack[(int)UILayer.Window];
            if (windows.Count == 0)
                return;

            var topUI = windows[windows.Count - 1];
            Close(topUI);
            windows.Remove(topUI);
            mUICaches.Add(topUI);

            if (windows.Count > 0)
            {
                topUI = windows[windows.Count - 1];
                Open(topUI);
            }
            else
            {
                if (mMainCameraCache != null)
                {
                    mMainCameraCache.enabled = true;
                    mMainCameraCache = null;
                }
                if (!mRenderSettingCache.IsEmpty)
                {
                    mRenderSettingCache.Apply();
                    mRenderSettingCache.Clear();
                }
            }
        }

        private void Open(UIOpen ui)
        {
            ui.Root.OnOpen(ui.Name, ui.Param);

            if (ui.Root.renderSetting != null)
            {
                if (mRenderSettingCache.IsEmpty)
                {
                    mRenderSettingCache.CopyFromSettings();
                }
                ui.Root.renderSetting.Apply();
            }
            else
            {
                if (!mRenderSettingCache.IsEmpty)
                {
                    mRenderSettingCache.Apply();
                    mRenderSettingCache.Clear();
                }
            }

            if(ui.Root.closeMainCamera)
            {
                if (mMainCameraCache == null)
                    mMainCameraCache = UnityEngine.Camera.main;
                mMainCameraCache.enabled = false;
            }
            else
            {
                if (mMainCameraCache != null)
                {
                    mMainCameraCache.enabled = true;
                    mMainCameraCache = null;
                }
            }

            OnUIOpen?.Invoke(ui.Name, ui.Param);
        }

        private void Close(UIOpen ui)
        {
            ui.Root.OnClose();
            OnUIClose?.Invoke(ui.Name);
        }
    }
}