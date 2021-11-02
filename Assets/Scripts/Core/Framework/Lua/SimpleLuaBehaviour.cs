using System;
using Core.Framework.Utility;
using UnityEngine;
using XLua;

namespace Core.Framework.Lua
{
    [System.Serializable]
    public class Injection
    {
        public string name;
        public GameObject value;
    }

    public class SimpleLuaBehaviour : MonoBehaviour
    {
        public TextAssetField luaScript;
        public Injection[] injections;

        private LuaTable mScriptEnv;
        private Action<LuaTable> mLuaAwake;
        private Action<LuaTable> mLuaStart;
        private Action<LuaTable> mLuaEnable;
        private Action<LuaTable> mLuaDisable;
        private Action<LuaTable> mLuaUpdate;
        private Action<LuaTable> mLuaOnDestroy;

        public LuaTable GetLuaUIController()
        {
            return mScriptEnv;
        }

        private void Awake()
        {
            var path = LuaManager.Instance.GetRelativePath(luaScript.path);
            var bytes = LuaManager.Instance.GetLuaBytesByFullPath(path);
            if (bytes == null)
            {
                Debug.LogError("not lua bytes path=" + luaScript.path);
                return;
            }

            var luaEnv = LuaManager.Instance.LuaEnv;
            mScriptEnv = luaEnv.NewTable();

            // 为每个脚本设置一个独立的环境，可一定程度上防止脚本间全局变量、函数冲突
            LuaTable meta = luaEnv.NewTable();
            meta.Set("__index", luaEnv.Global);
            mScriptEnv.SetMetaTable(meta);
            meta.Dispose();

            mScriptEnv.Set("mBehaviour", this);
            foreach (var injection in injections)
            {
                mScriptEnv.Set(injection.name, injection.value);
            }
            luaEnv.DoString(bytes, path, mScriptEnv);

            mLuaAwake = mScriptEnv.Get<Action<LuaTable>>("Awake");
            mLuaStart = mScriptEnv.Get<Action<LuaTable>>("Start");
            mLuaEnable = mScriptEnv.Get<Action<LuaTable>>("OnEnable");
            mLuaDisable = mScriptEnv.Get<Action<LuaTable>>("OnDisable");
            mLuaUpdate = mScriptEnv.Get<Action<LuaTable>>("Update");
            mLuaOnDestroy = mScriptEnv.Get<Action<LuaTable>>("OnDestroy");

            if (mLuaAwake != null)
            {
                mLuaAwake.Invoke(mScriptEnv);
            }
        }

        private void Start()
        {
            if (mLuaStart != null)
            {
                mLuaStart.Invoke(mScriptEnv);
            }
        }

        private void OnEnable()
        {
            if(mLuaEnable != null)
            {
                mLuaEnable.Invoke(mScriptEnv);
            }

            if (mLuaUpdate != null)
                LuaManager.Instance.OnUpdate += DoUpdate;
        }

        private void OnDisable()
        {
            if(mLuaDisable != null)
            {
                mLuaDisable.Invoke(mScriptEnv);
            }

            if (mLuaUpdate != null && LuaManager.Instance != null)
                LuaManager.Instance.OnUpdate -= DoUpdate;
        }

        private void OnDestroy()
        {
            if (mLuaOnDestroy != null)
            {
                mLuaOnDestroy.Invoke(mScriptEnv);
            }
            mLuaOnDestroy = null;
            mLuaUpdate = null;
            mLuaStart = null;
            mLuaEnable = null;
            mLuaDisable = null;
            mLuaAwake = null;
            mScriptEnv.Dispose();
            injections = null;
        }

        private void DoUpdate()
        {
            if (mLuaUpdate != null)
                mLuaUpdate.Invoke(mScriptEnv);
        }

        public void Set<T>(string key, T name)
        {
            mScriptEnv.Set(key, name);
        }

        public bool TryInvokeFunc(string funcName)
        {
            var func = mScriptEnv.Get<Action<LuaTable>>(funcName);
            if (func != null)
            {
                func.Invoke(mScriptEnv);
                return true;
            }
            return false;
        }
    }
}