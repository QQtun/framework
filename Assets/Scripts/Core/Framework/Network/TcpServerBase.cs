using Core.Framework.Network.Buffers;
using Core.Framework.Network.Data;
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

        private LinkedList<TcpClientBase> _clients;
        private Dictionary<TcpClientBase, LinkedListNode<TcpClientBase>> _clientToNodeDic;

        protected TcpServerBase(MessageFactory factory, BufferPool pool)
        {
            MessageFactory = factory;
            BufferPool = pool;

            _accepter = new TcpAccepter();
            _clients = new LinkedList<TcpClientBase>();
            _clientToNodeDic = new Dictionary<TcpClientBase, LinkedListNode<TcpClientBase>>(1000);
            _tmpList = new List<TcpClientBase>(1000);
        }

        public void Start(IPAddress ip, int port)
        {
            if(_accepter != null)
            {
                Stop(DisconnectReason.User);
            }

            _clients.Clear();
            _clientToNodeDic.Clear();
            _tmpList.Clear();
            _accepter = new TcpAccepter();
            _accepter.OnAccept += OnAccept;
            _accepter.Start(ip, port);
        }

        public void Stop(DisconnectReason reason)
        {
            _tmpList.Clear();
            _tmpList.AddRange(_clients);
            foreach (var client in _tmpList)
            {
                client.Disconnect(reason, true);
            }

            _clients.Clear();
            _clientToNodeDic.Clear();
            _tmpList.Clear();
            _accepter?.Stop(true);
            _accepter = null;
        }

        public void RemoveClient(TcpClientBase client)
        {
            if (_clientToNodeDic.TryGetValue(client, out var node))
            {
                _clients.Remove(node);
                _clientToNodeDic.Remove(client);
            }
        }

        public void MainLoop()
        {
            _accepter?.MainLoop();

            _tmpList.Clear();
            _tmpList.AddRange(_clients);
            foreach (var client in _tmpList)
            {
                client.MainLoop();
            }
        }

        private void OnAccept(TcpClient tcpClient)
        {
            var newClient = CreateClient(tcpClient);

            var node = _clients.AddLast(newClient);
            _clientToNodeDic.Add(newClient, node);

            newClient.BegineReceive();
        }

        protected abstract TcpClientBase CreateClient(TcpClient newClient);
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

        protected override TcpClientBase CreateClient(TcpClient newClient)
        {
            return ClientFactory.Create(newClient);
        }
    }
}
