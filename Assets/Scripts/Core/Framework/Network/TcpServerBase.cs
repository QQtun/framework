using Core.Framework.Network.Buffers;
using Core.Framework.Network.Data;
using LogUtil;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Core.Framework.Network
{
    public interface ITcpClientFactory<T>
        where T : TcpClientBase
    {
        T Create(TcpClient client);
    }

    public abstract class TcpServerBase
    {
        private TcpAccepter _accepter;

        public MessageFactory MessageFactory { get; }
        public BufferPool BufferPool { get; }

        private List<TcpClientBase> _tmpList;
        protected List<TcpClientBase> clients;

        protected TcpServerBase(MessageFactory factory, BufferPool pool)
        {
            MessageFactory = factory;
            BufferPool = pool;

            _accepter = new TcpAccepter();
            clients = new List<TcpClientBase>();
            _tmpList = new List<TcpClientBase>();
        }

        public void Start(IPAddress ip, int port)
        {
            if(_accepter != null)
            {
                Stop(DisconnectReason.User);
            }

            clients.Clear();
            _tmpList.Clear();
            _accepter = new TcpAccepter();
            _accepter.OnAccept += CreateClient;
            _accepter.Start(ip, port);
        }

        public void Stop(DisconnectReason reason)
        {
            foreach(var client in clients)
            {
                client.Disconnect(reason, true);
            }

            clients.Clear();
            _tmpList.Clear();
            _accepter?.Stop(true);
        }

        public void MainLoop()
        {
            _accepter?.MainLoop();

            _tmpList.Clear();
            foreach(var client in clients)
            {
                client.MainLoop();
                if(client.Connected)
                    _tmpList.Add(client);
            }
            var tmp = clients;
            clients = _tmpList;
            _tmpList = tmp;
        }

        protected abstract void CreateClient(TcpClient newClient);
    }


    public class TcpServerBase<T> : TcpServerBase
        where T : TcpClientBase
    {
        public ITcpClientFactory<T> ClientFactory { get; }

        protected TcpServerBase(ITcpClientFactory<T> clientFactory, MessageFactory factory, BufferPool pool)
            : base(factory, pool)
        {
            ClientFactory = clientFactory;
        }

        protected override void CreateClient(TcpClient newClient)
        {
            var newClientBase = ClientFactory.Create(newClient);
            clients.Add(newClientBase);
            newClientBase.BegineReceive();
        }
    }
}
