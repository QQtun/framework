using Core.Framework.Module;
using Core.Framework.Network;
using Core.Framework.Network.Data;
using Core.Game.Network;
using System.Collections.Generic;

namespace Core.Game.Module
{
    //[Module]
    public class TestModule2 : NetworkModule
    {
        private List<int> mDependencies = new List<int>()
        {
            (int) ModuleId.TestModule1,
        };

        private List<int> mMsgIds = new List<int>()
        {
            //(int) ServerCmd.CMD_SPR_GETROLEUSINGGOODSDATALIST,
        };

        public override int Id => (int)ModuleId.TestModule2;

        public override List<int> DependencyModules()
        {
            return mDependencies;
        }

        public override List<int> GetMessageIds()
        {
            return mMsgIds;
        }

        public override void OnApplicationPause(bool pauseStatus)
        {
            LogUtil.Debug.Log("TestModule2 OnApplicationPause pauseStatus=" + pauseStatus);
        }

        public override void OnConnected()
        {
            LogUtil.Debug.Log("TestModule2 OnConnected");
        }

        public override void OnDestroy()
        {
            LogUtil.Debug.Log("TestModule2 OnDestroy");
        }

        public override void OnDisconnected(DisconnectReason reason)
        {
            LogUtil.Debug.Log("TestModule2 OnDisconnected reason=" + reason);
        }

        public override void OnGameInit()
        {
            LogUtil.Debug.Log("TestModule2 OnGameInit");
        }

        public override void OnReceivedMessage(Message msg)
        {
            LogUtil.Debug.Log("TestModule2 OnReceivedMessage msgId=" + msg.MessageId);
        }

        public override void OnStart()
        {
            LogUtil.Debug.Log("TestModule2 OnStart");
        }

        public override void OnUpdate()
        {
            //LogUtil.Debug.Log("TestModule2 OnUpdate");
        }
    }
}