using System.Collections.Generic;
using System.Diagnostics;
using Core.Game.Log;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using XLua;

namespace Core.Framework.Lua
{
    public class LuaManager : Singleton<LuaManager>
    {
        public const string LuaFolderPath = "Assets/Scripts/LuaScripts";
        public const string LuaCollectionPath = LuaFolderPath + "/AllLuaScripts.asset";

        private LuaEnv mLuaEnv;
        private AsyncOperationHandle<TextAssetCollection> mLuaAssetHandle;
        private TextAssetCollection mAllLuaScripts;
        private Stopwatch mTickTimer = new Stopwatch();
        private Dictionary<string, byte[]> mFileNameToBytesDic = new Dictionary<string, byte[]>();

        public LuaEnv LuaEnv => mLuaEnv;
        public bool IsLuaDownloaded => mAllLuaScripts != null;

        public event System.Action OnUpdate;

        protected override void Init()
        {
            base.Init();

            mLuaEnv = new XLua.LuaEnv();
            mLuaEnv.AddLoader((ref string filename) =>
            {
                if (mAllLuaScripts == null)
                {
                    LogUtil.Debug.LogError("LuaManager is not ready", LogTagEx.Lua);
                    return null;
                }

                var newName = filename.Substring(0);
                newName = newName.Replace('.', '/');
                newName = newName.StartsWith("/") ? newName.Substring(1) : newName;
                return GetFileBytesByRelativePath(newName);
            });
        }

        public void DownloadAllLua()
        {
            mLuaAssetHandle = Addressables.LoadAssetAsync<TextAssetCollection>(LuaCollectionPath);
            mLuaAssetHandle.Completed += (h) =>
            {
                mAllLuaScripts = h.Result;
            };
        }

        public void StartLuaMain()
        {
            mLuaEnv.DoString(GetFileBytesByRelativePath("Main"), "Main");
        }

        private void Update()
        {
            if(mLuaEnv != null)
            {
                mTickTimer.Reset();
                mTickTimer.Start();
                mLuaEnv.Tick();
                mTickTimer.Stop();
                if(mTickTimer.ElapsedMilliseconds > 1000)
                {
                    UnityEngine.Debug.Log("lua env tick spend too much time !! ms=" + mTickTimer.ElapsedMilliseconds);
                }
            }

            OnUpdate?.Invoke();
        }

        public byte[] GetFileBytesByRelativePath(string path)
        {
            if(mFileNameToBytesDic.TryGetValue(path, out var bytes))
            {
                return bytes;
            }
            var txt = mAllLuaScripts.GetAsset(path);
            if(txt != null)
            {
                bytes = System.Text.Encoding.UTF8.GetBytes(txt.text);
                mFileNameToBytesDic.Add(path, bytes);
                return bytes;
            }
            LogUtil.Debug.LogError("cant get lua bytes path=" + path, LogTagEx.Lua);
            return null;
        }

        public byte[] GetLuaBytesByFullPath(string path)
        {
            var relativePath = GetRelativePath(path);
            return GetFileBytesByRelativePath(relativePath);
        }

        public string GetRelativePath(string path)
        {
            var txtExt = ".lua.txt";
            path = path.EndsWith(txtExt) ? path.Substring(0, path.Length - txtExt.Length) : path;
            var relativePath = path.StartsWith(LuaFolderPath) ? path.Substring(LuaFolderPath.Length + 1) : path;
            return relativePath;
        }
    }
}