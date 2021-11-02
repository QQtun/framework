using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

namespace Core.Framework.Res
{
    public struct LoadParam
    {
        public bool useCache;
        public bool unloadWhenSceneChanged;

        public static LoadParam Default { get; } = new LoadParam() { useCache = false, unloadWhenSceneChanged = false };
    }

    public class ResourceManager : Singleton<ResourceManager>
    {
        private class LoadedAsset
        {
            public string path;
            public AsyncOperationHandle handle;
            public LoadParam param;
        }

        private AddressableFolderSetting mFolderSetting;
        private Dictionary<string, LoadedAsset> mAssets = new Dictionary<string, LoadedAsset>();

        public bool IsInitialized { get; private set; }

        private string[] mArrayCache;

        protected override void Init()
        {
            base.Init();

            IsInitialized = false;
            var handle = Addressables.LoadAssetAsync<AddressableFolderSetting>(AddressableFolderSetting.Path);
            handle.Completed += OnSettingLoaded;

            SceneManager.sceneUnloaded += (s) =>
            {
                var keys = mAssets.Keys;
                int keysCount = keys.Count;
                if (mArrayCache == null || mArrayCache.Length < keysCount)
                    mArrayCache = new string[keysCount * 2];
                keys.CopyTo(mArrayCache, 0);

                for (int i = 0; i < keysCount; i++)
                {
                    var path = mArrayCache[i];
                    if (mAssets.TryGetValue(path, out var loadedAsset)
                        && loadedAsset.param.unloadWhenSceneChanged)
                    {
                        mAssets.Remove(path);
                    }
                }

                Array.Clear(mArrayCache, 0, keysCount);
            };
        }

        private void OnSettingLoaded(AsyncOperationHandle<AddressableFolderSetting> handle)
        {
            mFolderSetting = handle.Result;
            IsInitialized = mFolderSetting != null;
            if (!IsInitialized)
            {
                Debug.LogError("ResourceManager AddressableFolderSetting is null !");
            }
            Addressables.Release(handle);
        }

        public bool IsCached(string path)
        {
            return mAssets.TryGetValue(path, out var loadedAsset)
                && loadedAsset.param.useCache;
        }

        public object GetCachedObject(string path)
        {
            if (mAssets.TryGetValue(path, out var loadedAsset)
                && loadedAsset.param.useCache)
            {
                return loadedAsset.handle.Result;
            }
            return null;
        }

        public void UnloadCache(string path)
        {
            if (mAssets.TryGetValue(path, out var loadedAsset)
                && loadedAsset.param.useCache)
            {
                Addressables.Release(loadedAsset.handle);
                mAssets.Remove(path);
            }
        }

        public void LoadTextAssetAsync(string path, Action<TextAsset> onCompleted)
        {
            LoadAssetAsync(path, LoadParam.Default, onCompleted);
        }

        public void LoadPrefabAssetAsync(string path, Action<UnityEngine.Object> onCompleted)
        {
            LoadAssetAsync(path, LoadParam.Default, onCompleted);
        }
        public void LoadSpriteAssetAsync(string path, Action<Sprite> onCompleted)
        {
            LoadAssetAsync(path, LoadParam.Default, onCompleted);
        }
        public void LoadAudioAssetAsync(string path, Action<AudioClip> onCompleted)
        {
            LoadAssetAsync(path, LoadParam.Default, onCompleted);
        }

        public void LoadServerConfigAsync(string path, Action<Network.ServerConfigs> onCompleted)
        {
            LoadAssetAsync(path, LoadParam.Default, onCompleted);
        }


        public void LoadAssetAsync<T>(string path, Action<T> onCompleted)
            where T : UnityEngine.Object
        {
            LoadAssetAsync<T>(path, LoadParam.Default, onCompleted);
        }

        public void LoadAssetAsync<T>(string path, LoadParam param, Action<T> onCompleted)
            where T : UnityEngine.Object
        {
            if (!IsInitialized)
            {
                StartCoroutine(LoadAssetCo(path, param, onCompleted));
                return;
            }
            DoLoadAssetAsync(path, param, onCompleted);
        }

        private IEnumerator LoadAssetCo<T>(string path, LoadParam param, Action<T> onCompleted)
            where T : UnityEngine.Object
        {
            while (!IsInitialized)
            {
                yield return null;
            }
            DoLoadAssetAsync(path, param, onCompleted);
        }
        private void DoLoadAssetAsync<T>(string path, LoadParam param, Action<T> onCompleted)
            where T : UnityEngine.Object
        {
            if (!IsInitialized)
            {
                Debug.LogErrorFormat("ResourceManager not ready !");
                return;
            }

            if (mFolderSetting.folders.FindIndex(f => path.StartsWith(f.path)) < 0)
            {
                Debug.LogErrorFormat("ResourceManager Asset not in Addressable path=" + path);
                return;
            }

            if (param.useCache && mAssets.TryGetValue(path, out var cacheAsset))
            {
                onCompleted.Invoke(cacheAsset.handle.Result as T);
                return;
            }

            var handle = Addressables.LoadAssetAsync<T>(path);
            handle.Completed += h =>
            {
                if (h.Result == null)
                {
                    Debug.LogError("ResourceManager LoaaAsset failed path=" + path);
                }

                if ((param.useCache || param.unloadWhenSceneChanged)
                    && !mAssets.ContainsKey(path))
                {
                    var loadedAsset = new LoadedAsset();
                    loadedAsset.path = path;
                    loadedAsset.handle = h;
                    loadedAsset.param = param;
                    mAssets.Add(path, loadedAsset);
                }
                onCompleted.Invoke(h.Result);
            };
        }
    }
}