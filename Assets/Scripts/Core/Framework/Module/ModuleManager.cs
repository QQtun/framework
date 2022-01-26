using Core.Framework.Network;
using Core.Framework.Network.Data;
using Core.Game.Network;
using System;
using System.Collections.Generic;

namespace Core.Framework.Module
{
    public class ModuleAttribute : Attribute
    {
    }

    public class ModuleManager : Singleton<ModuleManager>
    {
        private Dictionary<Type, int> mModuleTypeToIdDic = new Dictionary<Type, int>();
        private Dictionary<int, IModule> mIdToSimpleModule = new Dictionary<int, IModule>();
        private Dictionary<int, INetworkModule> mIdToNetModule = new Dictionary<int, INetworkModule>();
        private Dictionary<int, INetworkModule> mMsgIdToNetModule = new Dictionary<int, INetworkModule>();
        private Dictionary<int, List<int>> mIdToWaitingMsgIds = new Dictionary<int, List<int>>();
        private List<Message> mWaitingMsg = new List<Message>();
        private Type[] mCtorTypes;
        private object[] mCtorParam;
        private bool mStarted;
        private bool mModuleConnectedInit;
        private bool mModuleGameInit;

        public event Action OnAllModuleInitialized;
        public bool ModuleInitializing => mModuleConnectedInit || mModuleGameInit;

        protected override void Init()
        {
            base.Init();

            mStarted = false;
            //GameConnection.Instance.OnConnected += OnConnected;
            //GameConnection.Instance.OnReceivedMessage += OnReceivedMessage;
            //GameConnection.Instance.OnDisconnected += OnDisconnected;

            mModuleConnectedInit = false;
            mModuleGameInit = false;
            mCtorTypes = new Type[] {};
            mCtorParam = new object[] {};

            List<Type> moduleTypes = new List<Type>();
            var assemply = typeof(ModuleManager).Assembly;
            var allTypes = assemply.GetTypes();
            foreach (var type in allTypes)
            {
                object[] attributes = type.GetCustomAttributes(typeof(ModuleAttribute), false);
                if (attributes.Length == 0)
                    continue;

                ModuleAttribute attribute = null;
                for (int i = 0; i < attributes.Length; i++)
                {
                    attribute = attributes[i] as ModuleAttribute;
                    if (attribute != null)
                        break;
                }
                if (attribute == null)
                    continue;

                moduleTypes.Add(type);
            }

            foreach (var type in moduleTypes)
            {
                var module = CreateModule(type);
                if (module != null)
                {
                    mModuleTypeToIdDic[type] = module.Id;
                    if (module is INetworkModule)
                    {
                        INetworkModule netModule = (INetworkModule)module;
                        mIdToNetModule[module.Id] = netModule;
                        var msgList = netModule.GetMessageIds();
                        if(msgList != null)
                        {
                            for (int i = 0; i < msgList.Count; i++)
                            {
                                if (mMsgIdToNetModule.ContainsKey(msgList[i]))
                                    LogUtil.Debug.LogErrorFormat(
                                        "Can't handle the same message at 2 modules !! msgId={0}", msgList[i]);
                                else
                                    mMsgIdToNetModule.Add(msgList[i], netModule);
                            }
                        }
                    }
                    else
                    {
                        mIdToSimpleModule[module.Id] = module;
                    }
                }
            }

#if DEBUG
            CheckAllDependencyDeadlock();
#endif
        }

        public T GetModule<T>()
            where T : class , IModule
        {
            var t = typeof(T);
            if(mModuleTypeToIdDic.TryGetValue(t, out var id))
            {
                if (mIdToSimpleModule.TryGetValue(id, out var module))
                    return module as T;
                if (mIdToNetModule.TryGetValue(id, out var module2))
                    return module2 as T;
            }
            return null;
        }

        public IModule GetModule(int id)
        {
            if (mIdToSimpleModule.TryGetValue(id, out var module))
                return module;
            if (mIdToNetModule.TryGetValue(id, out var module2))
                return module2;
            return null;
        }

        public bool RegisterModule(IModule module)
        {
            if (mIdToSimpleModule.ContainsKey(module.Id))
                return false;
            mIdToSimpleModule.Add(module.Id, module);

            if (mStarted)
                module.OnStart();
#if DEBUG
            CheckDependencyDeadlock(module.Id);
#endif
            return true;
        }

        public bool RegisterNetworkModule(INetworkModule module)
        {
            if (mIdToNetModule.ContainsKey(module.Id))
                return false;
            mIdToNetModule.Add(module.Id, module);

            if (mStarted)
                module.OnStart();

            var msgList = module.GetMessageIds();
            if (msgList != null)
            {
                for (int i = 0; i < msgList.Count; i++)
                {
                    if (mMsgIdToNetModule.ContainsKey(msgList[i]))
                        LogUtil.Debug.LogErrorFormat(
                            "Can't handle the same message at 2 modules !! msgId={0}", msgList[i]);
                    else
                        mMsgIdToNetModule.Add(msgList[i], module);
                }
            }
#if DEBUG
            CheckDependencyDeadlock(module.Id);
#endif
            return true;
        }

        public void StartGameInit()
        {
            mModuleGameInit = true;

            mWaitingMsg.Clear();
            var iter2 = mIdToNetModule.GetEnumerator();
            while (iter2.MoveNext())
            {
                iter2.Current.Value.OnGameInit();
            }
            iter2.Dispose();

            mModuleGameInit = mIdToWaitingMsgIds.Count > 0;
        }

        public void Send(int moduleId, Message msg)
        {
            //GameConnection.Instance.Send(msg);

            if(ModuleInitializing)
            {
                if (!mIdToWaitingMsgIds.TryGetValue(moduleId, out var msgIds))
                {
                    msgIds = new List<int>();
                    mIdToWaitingMsgIds[moduleId] = msgIds;
                }
                msgIds.Add(msg.MessageId);
            }
        }

        public void Send(int moduleId, int msgId, Google.Protobuf.IMessage msg)
        {
            //GameConnection.Instance.Send(msgId, msg);

            if(ModuleInitializing)
            {
                if (!mIdToWaitingMsgIds.TryGetValue(moduleId, out var msgIds))
                {
                    msgIds = new List<int>();
                    mIdToWaitingMsgIds[moduleId] = msgIds;
                }
                msgIds.Add(msgId);
            }
        }

        //public void Send(int moduleId, int msgId, string msg, bool encrypt = true)
        //{
        //    GameConnection.Instance.Send(msgId, msg, encrypt);

        //    if (mModuleInitializing)
        //    {
        //        if (!mIdToWaitingMsgIds.TryGetValue(moduleId, out var msgIds))
        //        {
        //            msgIds = new List<int>();
        //            mIdToWaitingMsgIds[moduleId] = msgIds;
        //        }
        //        msgIds.Add(msgId);
        //    }
        //}

        private void OnDisconnected(DisconnectReason reason)
        {
            var iter2 = mIdToNetModule.GetEnumerator();
            while (iter2.MoveNext())
            {
                iter2.Current.Value.OnDisconnected(reason);
            }
            iter2.Dispose();
        }

        private void OnReceivedMessage(Message msg)
        {
            if(ModuleInitializing)
            {
                if (mMsgIdToNetModule.TryGetValue(msg.MessageId, out var module)
                    && mIdToWaitingMsgIds.TryGetValue(module.Id, out var waitingMsgIds)
                    && waitingMsgIds.Contains(msg.MessageId))
                {
                    mWaitingMsg.Add(msg);
                    TryInitModule(module);
                }
                else
                {
                    if(module == null)
                    {
                        // 不是module處裡的封包?
                    }
                    else
                    {
                        module.OnReceivedMessage(msg);
                    }
                }
            }
            else
            {
                if(mMsgIdToNetModule.TryGetValue(msg.MessageId, out var module))
                {
                    module.OnReceivedMessage(msg);
                }
            }
        }

        private void OnConnected()
        {
            mModuleConnectedInit = true;

            mWaitingMsg.Clear();
            var iter2 = mIdToNetModule.GetEnumerator();
            while (iter2.MoveNext())
            {
                iter2.Current.Value.OnConnected();
            }
            iter2.Dispose();

            mModuleConnectedInit = mIdToWaitingMsgIds.Count > 0;
        }

        private void Start()
        {
            mStarted = true;
            var iter = mIdToSimpleModule.GetEnumerator();
            while (iter.MoveNext())
            {
                iter.Current.Value.OnStart();
            }
            iter.Dispose();

            var iter2 = mIdToNetModule.GetEnumerator();
            while (iter2.MoveNext())
            {
                iter2.Current.Value.OnStart();
            }
            iter2.Dispose();
        }

        private void OnDestroy()
        {
            var iter = mIdToSimpleModule.GetEnumerator();
            while (iter.MoveNext())
            {
                iter.Current.Value.OnDestroy();
            }
            iter.Dispose();

            var iter2 = mIdToNetModule.GetEnumerator();
            while (iter2.MoveNext())
            {
                iter2.Current.Value.OnDestroy();
            }
            iter2.Dispose();
        }

        private void OnApplicationPause(bool pause)
        {
            var iter = mIdToSimpleModule.GetEnumerator();
            while (iter.MoveNext())
            {
                iter.Current.Value.OnApplicationPause(pause);
            }
            iter.Dispose();

            var iter2 = mIdToNetModule.GetEnumerator();
            while (iter2.MoveNext())
            {
                iter2.Current.Value.OnApplicationPause(pause);
            }
            iter2.Dispose();
        }

        private void Update()
        {
            if(ModuleInitializing)
            {
                foreach(var kv in mIdToWaitingMsgIds)
                {
                    var id = kv.Key;
                    var module = GetModule(id) as INetworkModule;
                    TryInitModule(module);
                }
            }

            var iter = mIdToSimpleModule.GetEnumerator();
            while (iter.MoveNext())
            {
                iter.Current.Value.OnUpdate();
            }
            iter.Dispose();

            //if (GameConnection.Instance != null && GameConnection.Instance.CanSendToGame)
            //{
            //    var iter2 = mIdToNetModule.GetEnumerator();
            //    while(iter2.MoveNext())
            //    {
            //        iter2.Current.Value.OnUpdate();
            //    }
            //    iter2.Dispose();
            //}
        }

        private IModule CreateModule(Type type)
        {
            var constructor = type.GetConstructor(mCtorTypes);
            if(constructor != null)
            {
                var module = (IModule)constructor.Invoke(mCtorParam);
                if(module == null)
                {
                    LogUtil.Debug.LogErrorFormat("why can't allocate ?????, System Name: {0}",
                        type.Name, LogUtil.LogTag.System);
                }
                return module;
            }
            LogUtil.Debug.LogFormat("there is no accessible constructor, System Name: {0}",
                type.Name, LogUtil.LogTag.System);
            return null;
        }

        private void TryInitModule(INetworkModule module)
        {
            if (module == null)
            {
                LogUtil.Debug.LogError("should not happend here !!");
                return;
            }

            if(!mIdToWaitingMsgIds.TryGetValue(module.Id, out var waitingMsgIds))
            {
                return;
            }

            var dependencies = module.DependencyModules();
            for (int i = 0; i < dependencies.Count; i++)
            {
                if (mIdToWaitingMsgIds.ContainsKey(dependencies[i]))
                {
                    return;
                }
            }

            for (int i = 0; i < waitingMsgIds.Count; i++)
            {
                var index = mWaitingMsg.FindIndex(m => m.MessageId == waitingMsgIds[i]);
                if (index < 0)
                {
                    return;
                }
            }

            int count = waitingMsgIds.Count;
            for (int i = 0; i < count; i++)
            {
                var index = mWaitingMsg.FindIndex(m => m.MessageId == waitingMsgIds[i]);
                var waitingMsg = mWaitingMsg[index];
                mWaitingMsg.RemoveAt(index);
                module.OnReceivedMessage(waitingMsg);
            }
            mIdToWaitingMsgIds.Remove(module.Id);
            if(mModuleConnectedInit)
                mModuleConnectedInit = mIdToWaitingMsgIds.Count > 0;
            if(mModuleGameInit)
                mModuleGameInit = mIdToWaitingMsgIds.Count > 0;

            if (!ModuleInitializing)
                OnAllModuleInitialized?.Invoke();
        }

#if DEBUG
        private void CheckDependencyDeadlock(int id)
        {
            var module = GetModule(id);
            HashSet<int> allDependentSet = new HashSet<int>();
            var dependencies = module.DependencyModules();
            for (int i = 0; i < dependencies.Count; i++)
            {
                GetAllDependencies(dependencies[i], allDependentSet);
            }
            if (allDependentSet.Contains(id))
            {
                LogUtil.Debug.LogError("this dependent relationship will cause dead lock !!!");
            }
        }
        private void CheckAllDependencyDeadlock()
        {
            foreach (var kv in mIdToNetModule)
            {
                var id = kv.Key;
                CheckDependencyDeadlock(id);
            }
            foreach (var kv in mIdToSimpleModule)
            {
                var id = kv.Key;
                CheckDependencyDeadlock(id);
            }
        }

        private void GetAllDependencies(int id, HashSet<int> allDependentSet)
        {
            if (allDependentSet.Add(id))
            {
                var module = GetModule(id);
                if (module == null)
                    return;
                var dependencies = module.DependencyModules();
                for (int i = 0; i < dependencies.Count; i++)
                {
                    GetAllDependencies(dependencies[i], allDependentSet);
                }
            }
        }

#endif
    }
}