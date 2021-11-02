using Core.Framework.Network;
using Core.Framework.Network.Data;
using System.Collections.Generic;

namespace Core.Framework.Module
{
    public interface IModule
    {
        int Id { get; }
        List<int> DependencyModules();
        void OnStart();
        void OnDestroy();
        void OnApplicationPause(bool pauseStatus);
        void OnUpdate();
    }

    public interface INetworkModule : IModule
    {
        List<int> GetMessageIds();
        void OnConnected();
        void OnGameInit();
        void OnReceivedMessage(Message msg);
        void OnDisconnected(DisconnectReason reason);
    }

    public abstract class NetworkModule : INetworkModule
    {
        public abstract int Id { get; }

        public abstract List<int> DependencyModules();
        public abstract List<int> GetMessageIds();
        public abstract void OnApplicationPause(bool pauseStatus);
        public abstract void OnConnected();
        public abstract void OnDestroy();
        public abstract void OnDisconnected(DisconnectReason reason);
        public abstract void OnGameInit();
        public abstract void OnReceivedMessage(Message msg);
        public abstract void OnStart();
        public abstract void OnUpdate();

        public void Send(int msgId, Google.Protobuf.IMessage msg)
        {
            ModuleManager.Instance.Send(Id, msgId, msg);
        }
        public void Send(Game.Network.ServerCmd msgId, Google.Protobuf.IMessage msg)
        {
            ModuleManager.Instance.Send(Id, (int)msgId, msg);
        }
    }
}