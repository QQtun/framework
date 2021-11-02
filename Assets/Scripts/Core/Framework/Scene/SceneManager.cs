using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Core.Framework.Scene
{
    public delegate void OnSceneChange(string from, string to, Dictionary<string, object> param);

    public interface ISceneEntry
    {
        void OnEntering(string from, string to, Dictionary<string, object> param);
        int OnEnteringProcess(string from, string to, Dictionary<string, object> param);
    }

    public class SceneManager : Singleton<SceneManager>
    {
        private string mCurSceneName;

        private string mTargetSceneName;
        private Dictionary<string, object> mParamCache;
        private Coroutine mCoroutine;
        private List<ISceneEntry> mEntries = new List<ISceneEntry>();
        private List<ISceneEntry> mProcessingEntries = new List<ISceneEntry>();
        private List<ISceneEntry> mFinishedEntries = new List<ISceneEntry>();
        private ISceneEntry mMainEntry;

        public event OnSceneChange OnStarted;
        public event OnSceneChange OnEntering;
        public event OnSceneChange OnFinished;

        public string CurSceneName { get => mCurSceneName; set => mCurSceneName = value; }
        public float EntryPercentage { get; private set; }
        public ISceneEntry SceneEntry => mMainEntry;

        public void AddEntry(ISceneEntry entry, bool asMain = false)
        {
            mEntries.Add(entry);
            if (asMain)
                mMainEntry = entry;
        }

        public void RemoveEntry(ISceneEntry entry)
        {
            mEntries.Remove(entry);
            if (entry == mMainEntry)
                mMainEntry = null;
        }

        public void LoadScene(string sceneName, Dictionary<string, object> param = null, bool force = false)
        {
            if (!force && (mCurSceneName == sceneName || !string.IsNullOrEmpty(mTargetSceneName)))
                return;

            EntryPercentage = 0;
            if (mCoroutine != null)
                StopCoroutine(mCoroutine);
            mCoroutine = null;

            OnStarted?.Invoke(mCurSceneName, sceneName, param);

            mTargetSceneName = sceneName;
            mParamCache = param;
            mCoroutine = StartCoroutine(LoadSceneCo(param));
        }

        private IEnumerator LoadSceneCo(Dictionary<string, object> param)
        {
            var op = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Empty");
            yield return op;

            AsyncOperation async = Resources.UnloadUnusedAssets();
            yield return async;
            GC.Collect();

            var handle = Addressables.LoadSceneAsync($"Assets/PublicAssets/Scenes/{mTargetSceneName}.unity");
            yield return handle;

            var lastSceneName = mCurSceneName;
            mCurSceneName = mTargetSceneName;

            if (handle.Result.Scene != null)
            {
                OnEntering?.Invoke(lastSceneName, mCurSceneName, mParamCache);

                var rootGOs = handle.Result.Scene.GetRootGameObjects();
                for (int i = 0; i < rootGOs.Length; i++)
                {
                    var rootGo = rootGOs[i];
                    var rootEnties = rootGo.GetComponentsInChildren<ISceneEntry>();
                    if (rootEnties != null && rootEnties.Length > 0)
                        mEntries.AddRange(rootEnties);
                }

                mProcessingEntries.Clear();
                mFinishedEntries.Clear();

                mProcessingEntries.AddRange(mEntries);
                for (int i = 0; i < mProcessingEntries.Count; i++)
                {
                    mProcessingEntries[i].OnEntering(lastSceneName, mCurSceneName, mParamCache);
                }

                while(mProcessingEntries.Count > 0)
                {
                    yield return null;
                    for (int i = 0; i < mProcessingEntries.Count; i++)
                    {
                        var progress = mProcessingEntries[i].OnEnteringProcess(lastSceneName, mCurSceneName, mParamCache);
                        if (progress == 100)
                            mFinishedEntries.Add(mProcessingEntries[i]);
                    }
                    for (int i = 0; i < mFinishedEntries.Count; i++)
                    {
                        mProcessingEntries.Remove(mFinishedEntries[i]);
                    }
                    EntryPercentage = (mEntries.Count - mFinishedEntries.Count) / (float)mEntries.Count;
                }
            }

            OnFinished?.Invoke(lastSceneName, mCurSceneName, mParamCache);

            mTargetSceneName = null;
            mParamCache = null;
            mCoroutine = null;
        }
    }
}