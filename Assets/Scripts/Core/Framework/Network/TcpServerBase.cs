using Core.Framework.Network.Buffers;
using Core.Framework.Network.Data;
using LogUtil;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Core.Framework.Network
{
    public interface ITcpClientFactory
    {
        TcpClientBase Create(TcpClient client);
    }

    public class TcpServerBase
    {
        private TcpAccepter _accepter;

        public ITcpClientFactory ClientFactory { get; }
        public MessageFactory MessageFactory { get; }
        public BufferPool BufferPool { get; }

        private List<TcpClientBase> _clients;
        private List<TcpClientBase> _tmpList;
             
        protected TcpServerBase(ITcpClientFactory clientFactory, MessageFactory factory, BufferPool pool)
        {
            ClientFactory = clientFactory;
            MessageFactory = factory;
            BufferPool = pool;

            _accepter = new TcpAccepter();
            _clients = new List<TcpClientBase>();
            _tmpList = new List<TcpClientBase>();
        }

        public void Start(IPAddress ip, int port)
        {
            if(_accepter != null)
            {
                Stop(DisconnectReason.User);
            }

            _clients.Clear();
            _tmpList.Clear();
            _accepter = new TcpAccepter();
            _accepter.OnAccept += CreateClient;
            _accepter.Start(ip, port);
        }

        public void Stop(DisconnectReason reason)
        {
            foreach(var client in _clients)
            {
                client.Disconnect(reason);
            }

            _clients.Clear();
            _tmpList.Clear();
            _accepter?.Stop();
        }

        public void MainLoop()
        {
            _accepter?.MainLoop();

            _tmpList.Clear();
            foreach(var client in _clients)
            {
                client.MainLoop();
                if(client.Connected)
                    _tmpList.Add(client);
            }
            var tmp = _clients;
            _clients = _tmpList;
            _tmpList = tmp;
        }

        private void CreateClient(TcpClient newClient)
        {
            var newClientBase = ClientFactory.Create(newClient);
            _clients.Add(newClientBase);
            newClientBase.BegineReceive();
        }
    }
}
