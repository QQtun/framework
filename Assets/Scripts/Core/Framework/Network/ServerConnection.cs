//using Core.Framework.Network.Data;
//using System;
//using System.Collections.Generic;
//using System.Net;

//namespace Core.Framework.Network
//{
//    /// <summary>
//    ///     預處理封包介面, 配合ServerConnection使用
//    ///     可以在IConnectionListener.OnReceiveMessage之前攔截並處理Message
//    /// </summary>
//    public interface IMessagePreprocessor
//    {
//        void OnConnected(INetConnection connection);

//        void OnDisconnected(INetConnection connection, DisconnectReason reason);

//        /// <summary>
//        ///     預處理收到的封包
//        /// </summary>
//        /// <param name="connection"></param>
//        /// <param name="message">INetConnection接收到的Message</param>
//        /// <param name="listener">處理完後的封包往外傳遞的監聽器</param>
//        /// <returns></returns>
//        bool PreprocessReceivedMessage(INetConnection connection, Message message, IConnectionListener listener);

//        /// <summary>
//        ///     預處理要發送的封包
//        /// </summary>
//        /// <param name="connection">處理完後的封包要往遠端發送的連線</param>
//        /// <param name="message">使用者要求發送的封包</param>
//        /// <returns></returns>
//        bool PreprocessSendMessage(INetConnection connection, Message message);

//        void Update(INetConnection connection, IConnectionListener listener, float dt);
//    }

//    public class ServerConnection : INetConnection, IConnectionListener, IDisposable
//    {
//        private IConnectionListener _connectionListener;
//        private NetConnection _netConnection;
//        private LinkedList<IMessagePreprocessor> _preprocessors;
//        private DateTime? _lastUpdateTime;
//        private bool _disposed;

//        public string Name
//        {
//            get { return _netConnection.NameID; }
//        }

//        public string NameID
//        {
//            get { return _netConnection.NameID; }
//        }

//        public IPAddress Ip
//        {
//            get { return _netConnection.Ip; }
//        }

//        public int Port
//        {
//            get { return _netConnection.Port; }
//        }

//        public int BufferSize
//        {
//            get { return _netConnection.BufferSize; }
//        }

//        public int RetryCount
//        {
//            get { return _netConnection.RetryCount; }
//        }

//        public int ConnectTimeout
//        {
//            get { return _netConnection.ConnectTimeout; }
//        }

//        public int SendTimeout
//        {
//            get { return _netConnection.SendTimeout; }
//        }

//        public int ReceiveTimeout
//        {
//            get { return _netConnection.ReceiveTimeout; }
//        }

//        public bool IsConnected
//        {
//            get { return _netConnection.IsConnected; }
//        }

//        public EndPoint LocalEndPoint
//        {
//            get { return _netConnection.LocalEndPoint; }
//        }

//        public DisconnectReason LastDisconnectReason
//        {
//            get { return _netConnection.LastDisconnectReason; }
//        }

//        public MessageFactory MsgFactory { get { return _netConnection.MsgFactory; } }

//        /// <summary>
//        ///
//        /// </summary>
//        /// <param name="name">連線名稱 方便偵錯</param>
//        /// <param name="bufferSize"></param>
//        /// <param name="retryCount"></param>
//        /// <param name="connectTimeout"></param>
//        /// <param name="sendTimeout"></param>
//        /// <param name="receiveTimeout"></param>
//        /// <param name="noDelay"></param>
//        /// <param name="factory"></param>
//        public ServerConnection(ServerConfig config, MessageFactory factory)
//        {
//            _preprocessors = new LinkedList<IMessagePreprocessor>();
//            _netConnection = new NetConnection(config, factory);
//            _netConnection.SetConnectionListener(this);
//        }

//        /// <summary>
//        ///     收到封包時，先加入的Processor會先接收到
//        ///     發送封包時，後加入的Processor會先接收到
//        /// </summary>
//        /// <param name="preprocessor"></param>
//        public void AddMessagePreprocessor(IMessagePreprocessor preprocessor)
//        {
//            _preprocessors.AddLast(preprocessor);
//        }

//        public void SetConnectionListener(IConnectionListener listener)
//        {
//            _connectionListener = listener;
//        }

//        public bool Connect(ServerSet set)
//        {
//            return _netConnection.Connect(set);
//        }

//        public void Disconnect()
//        {
//            _netConnection.Disconnect();
//        }

//        public void Disconnect(DisconnectReason reason)
//        {
//            _netConnection.Disconnect(reason);
//        }

//        public void Update()
//        {
//            float dt = 0f;
//            if (_lastUpdateTime != null)
//            {
//                TimeSpan deltaTime = DateTime.Now - _lastUpdateTime.Value;
//                dt = (float)deltaTime.TotalSeconds;
//            }

//            _netConnection.Update();
//            LinkedListNode<IMessagePreprocessor> node = _preprocessors.First;
//            while (node != null)
//            {
//                node.Value.Update(this, _connectionListener, dt);
//                node = node.Next;
//            }

//            _lastUpdateTime = DateTime.Now;
//        }

//        public void Send(Message message)
//        {
//            // 注意這邊要從後面的processor開始
//            LinkedListNode<IMessagePreprocessor> node = _preprocessors.Last;
//            while (node != null)
//            {
//                if (node.Value.PreprocessSendMessage(this, message))
//                {
//                    return;
//                }
//                node = node.Previous;
//            }

//            _netConnection.Send(message);
//        }

//        public void OnConnected()
//        {
//            LinkedListNode<IMessagePreprocessor> node = _preprocessors.First;
//            while (node != null)
//            {
//                node.Value.OnConnected(this);
//                node = node.Next;
//            }

//            if (_connectionListener != null)
//                _connectionListener.OnConnected();
//        }

//        public void OnReceiveMessage(Message msg)
//        {
//            LinkedListNode<IMessagePreprocessor> node = _preprocessors.First;
//            while (node != null)
//            {
//                if (node.Value.PreprocessReceivedMessage(this, msg, _connectionListener))
//                {
//                    return;
//                }
//                node = node.Next;
//            }

//            if (_connectionListener != null)
//                _connectionListener.OnReceiveMessage(msg);
//        }

//        public void OnDisconnected(DisconnectReason reason)
//        {
//            LinkedListNode<IMessagePreprocessor> node = _preprocessors.First;
//            while (node != null)
//            {
//                node.Value.OnDisconnected(this, reason);
//                node = node.Next;
//            }

//            if (_connectionListener != null)
//                _connectionListener.OnDisconnected(reason);
//        }

//        public void Dispose()
//        {
//            Dispose(true);
//        }

//        protected virtual void Dispose(bool final)
//        {
//            if (!_disposed)
//            {
//                _netConnection.Disconnect();
//                _disposed = true;
//            }
//        }
//    }
//}