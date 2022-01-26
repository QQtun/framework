using Core.Framework.Network.Buffers;
using Core.Framework.Network.Data;
using LogUtil;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Core.Framework.Network
{
    public abstract class TcpClientBase
    {
        private static readonly byte[] ZeroBuffer = new byte[0];

        private class SendState
        {
            public bool sending;
            public uint reqSerial;
            public uint rspSerial;
            public List<Message> sendMsgs = new List<Message>();
        }

        private class DisconnetState
        {
            public bool invoke;
            public DisconnectReason reason;
        }

        private class ConnectState
        {
            public bool invoke;
        }

        private enum RecvState
        {
            Header,
            Content,
            Footer,
        }

        private class TcpState
        {
            public IPAddress ip;
            public int port;
            public bool connecting;

            public TcpClient tcpClient;
            public NetworkStream netStream;

            public BufferStream bufferStream;
            public RecvState recvState;

            public Header header;
            public Message msg;
        }

        private TcpState _tcpState;

        private ConnectState _connectState;

        private List<Message> _recvMsgs;
        private List<Message> _recvMsgsMainLoop;

        private DisconnetState _disconnetState;
        private SendState _sendState;

        public MessageFactory MessageFactory { get; }
        private BufferPoolProvider BufferPoolProvider { get; }
        public EndPoint RemoteEndPoint
        {
            get
            {
                lock(_tcpState)
                {
                    return _tcpState?.tcpClient?.Client.RemoteEndPoint;
                }
            }
        }

        public bool Connected
        {
            get
            {
                lock (_tcpState)
                {
                    return _tcpState?.tcpClient?.Connected ?? false;
                }
            }
        }

        protected TcpClientBase(MessageFactory factory, BufferPoolProvider pool)
        {
            MessageFactory = factory;
            BufferPoolProvider = pool;

            _connectState = new ConnectState();
            _recvMsgs = new List<Message>();
            _recvMsgsMainLoop = new List<Message>();
            _disconnetState = new DisconnetState();
            _sendState = new SendState();

            _tcpState = new TcpState();

            MessageHandlerUtil.Init(GetType());
        }

        protected TcpClientBase(TcpClient client, MessageFactory factory, BufferPoolProvider pool)
            : this(factory, pool)
        {
            _tcpState = new TcpState();
            _tcpState.tcpClient = client;
            _tcpState.netStream = client.GetStream();
        }

        public bool Connect(IPAddress ip, int port)
        {
            try
            {
                lock (_tcpState)
                {
                    if (_tcpState.tcpClient != null)
                    {
                        Disconnect(DisconnectReason.User, true);
                        lock (_connectState)
                        {
                            _connectState.invoke = false;
                        }
                    }

                    _tcpState.tcpClient = new TcpClient();
                    _tcpState.ip = ip;
                    _tcpState.port = port;
                }

                ThreadPool.QueueUserWorkItem(ConnectMainLoopThreaded, _tcpState);
                return true;
            }
            catch(Exception ex)
            {
                Debug.LogError(ex);
                return false;
            }
        }

        public void Disconnect(DisconnectReason reason, bool invokeNow = false)
        {
            lock (_tcpState)
            {
                if (_tcpState.netStream != null)
                {
                    _tcpState.netStream.Close();
                    _tcpState.netStream = null;
                }

                if (_tcpState.tcpClient != null)
                {
                    _tcpState.tcpClient.Close();
                    _tcpState.tcpClient = null;
                }

                _tcpState.connecting = false;

                if(_tcpState.bufferStream != null)
                {
                    BufferPoolProvider.BufferStreamPool.Dealloc(_tcpState.bufferStream);
                    _tcpState.bufferStream = null;
                }
            }

            lock(_sendState)
            {
                _sendState.sending = false;
            }

            if (invokeNow)
            {
                OnDisconnected(reason);
            }
            else
            {
                lock (_disconnetState)
                {
                    _disconnetState.invoke = true;
                    _disconnetState.reason = reason;
                }
            }
        }

        private void TryDisconnect(TcpState tcpState, TcpClient tcpClient, DisconnectReason reason, bool invokeNow = false)
        {
            var callDisconnect = false;
            lock (tcpState)
            {
                callDisconnect = tcpState.tcpClient != null && tcpState.tcpClient == tcpClient;
            }
            if (callDisconnect)
                Disconnect(reason, invokeNow);
        }

        public void MainLoop()
        {
            lock (_connectState)
            {
                if (_connectState.invoke)
                {
                    _connectState.invoke = false;
                    OnConnected();
                }
            }

            lock (_recvMsgs)
            {
                _recvMsgsMainLoop.AddRange(_recvMsgs);
                _recvMsgs.Clear();
            }
            for (int i = 0; i < _recvMsgsMainLoop.Count; i++)
            {
                OnReceiveMessage(_recvMsgsMainLoop[i]);
            }
            _recvMsgsMainLoop.Clear();

            lock (_disconnetState)
            {
                if(_disconnetState.invoke)
                {
                    _disconnetState.invoke = false;
                    OnDisconnected(_disconnetState.reason);
                }
            }
        }

        public ValueTuple<bool, uint> Request(Message msg)
        {
            ValueTuple<bool, uint> ret;
            if (!Connected)
            {
                Debug.Log("Can't Send When Disconnected");
                ret = new ValueTuple<bool, uint>(false, 0);
                return ret;
            }

            lock (_sendState)
            { 
                msg.SetRequestSerial(++_sendState.reqSerial);
                msg.SetResponseSerial(0);
                ret = new ValueTuple<bool, uint>(true, _sendState.reqSerial);
            }
            Send(msg);
            return ret;
        }

        public bool Response(uint reqSerial, Message msg)
        {
            if (!Connected)
            {
                Debug.Log("Can't Send When Disconnected");
                return false;
            }

            msg.SetRequestSerial(reqSerial);
            lock (_sendState)
            {
                msg.SetResponseSerial(++_sendState.rspSerial);
            }
            Send(msg);
            return true;
        }

        private void Send(Message msg)
        {
            lock (_sendState)
            {
                _sendState.sendMsgs.Add(msg);
                if (!_sendState.sending)
                {
                    _sendState.sending = true;
                    ThreadPool.QueueUserWorkItem(SendMainLoopThreaded, _tcpState);
                }
            }
        }

        public void BegineReceive()
        {
            lock(_tcpState)
            {
                _tcpState.bufferStream = BufferPoolProvider.BufferStreamPool.Alloc();
                _tcpState.recvState = RecvState.Header;
            }
            lock (_connectState)
            {
                _connectState.invoke = true;
            }
            ThreadPool.QueueUserWorkItem(ReceiveMainLoop, _tcpState);
        }

        private void ConnectMainLoopThreaded(object state)
        {
            TcpState tcpState = state as TcpState;
            TcpClient tcpClient;
            lock (tcpState)
            {
                if (tcpState.connecting)
                    return;
                if (tcpState.tcpClient == null)
                    return;
                if (tcpState.tcpClient != null && tcpState.tcpClient.Connected)
                    return;
                tcpState.connecting = true;
                tcpClient = tcpState.tcpClient;
            }

            try
            {
                tcpClient.Connect(tcpState.ip, tcpState.port);

                if(tcpClient.Connected)
                {
                    lock (tcpState)
                    {
                        tcpState.netStream = tcpClient.GetStream();
                        tcpState.bufferStream = BufferPoolProvider.BufferStreamPool.Alloc();
                        tcpState.recvState = RecvState.Header;
                    }
                    lock (_connectState)
                    {
                        _connectState.invoke = true;
                    }

                    ThreadPool.QueueUserWorkItem(ReceiveMainLoop, state);
                }
                else
                {
                    TryDisconnect(tcpState, tcpClient, DisconnectReason.Network);
                }
            }
            catch(Exception ex)
            {
                Debug.LogError(ex);
                TryDisconnect(tcpState, tcpClient, DisconnectReason.Network);
            }
            finally
            {
                lock (tcpState)
                {
                    tcpState.connecting = false;
                }
            }
        }

        private void ReceiveMainLoop(object state)
        {
            TcpState tcpState = state as TcpState;
            TcpClient tcpClient;
            NetworkStream netStream;
            BufferStream bufferStream;
            lock (tcpState)
            {
                if (tcpState.tcpClient == null)
                    return;
                tcpClient = tcpState.tcpClient;
                netStream = tcpState.netStream;
                bufferStream = tcpState.bufferStream;
            }

            try
            {
                if (tcpClient.Client.Poll(0, SelectMode.SelectRead))
                {
                    var buffer = BufferPoolProvider.BufferBasePool.Alloc();
                    if(buffer == null)
                    {
                        // 沒有buffer可用，等等吧
                        ThreadPool.QueueUserWorkItem(ReceiveMainLoop, state);
                        return;
                    }
                    try
                    {
                        do
                        {
                            buffer.Reset();
                            var readCount = Math.Min(tcpClient.Available, buffer.BufferSize);
                            var realReadCount = netStream.Read(buffer.Buffer, 0, readCount);
                            if (realReadCount == 0)
                            {
                                Disconnect(DisconnectReason.Remote);
                                return;
                            }
                            bufferStream.Write(buffer.Buffer, 0, realReadCount);
                        }
                        while (tcpClient.Available > 0);
                    }
                    finally
                    {
                        BufferPoolProvider.BufferBasePool.Dealloc(buffer);
                        bufferStream.Seek(0, System.IO.SeekOrigin.Begin);
                    }

                    lock (tcpState)
                    {
                        while (true)
                        {
                            if (tcpState.recvState == RecvState.Header)
                            {
                                if (bufferStream.CanReadHeader)
                                {
                                    var header = bufferStream.ReadHeaer();
                                    if (header.ContentSize == 0)
                                    {
                                        var msg = MessageFactory.CreateMessage(header, ZeroBuffer, 0);
                                        tcpState.msg = msg;
                                        tcpState.recvState = RecvState.Footer;
                                    }
                                    else
                                    {
                                        tcpState.header = header;
                                        tcpState.recvState = RecvState.Content;
                                    }
                                }
                                else
                                {
                                    break;
                                }
                            }

                            if (tcpState.recvState == RecvState.Content)
                            {
                                if (bufferStream.CanReadContent(tcpState.header.ContentSize))
                                {
                                    var msg = bufferStream.ReadMessage(MessageFactory, tcpState.header);
                                    tcpState.msg = msg;
                                    tcpState.recvState = RecvState.Footer;
                                }
                                else
                                {
                                    break;
                                }
                            }

                            if (tcpState.recvState == RecvState.Footer)
                            {
                                if (bufferStream.CanReadFooter)
                                {
                                    var footer = bufferStream.ReadFooter();
                                    tcpState.msg.Footer = footer;
                                    lock (_recvMsgs)
                                    {
                                        _recvMsgs.Add(tcpState.msg);
                                    }
                                    tcpState.recvState = RecvState.Header;
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }

                    bufferStream.ReleaseUnuseBuffer();

                    ThreadPool.QueueUserWorkItem(ReceiveMainLoop, state);
                }
                else
                {
                    ThreadPool.QueueUserWorkItem(ReceiveMainLoop, state);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                TryDisconnect(tcpState, tcpClient, DisconnectReason.Network);
            }
        }

        private void SendMainLoopThreaded(object state)
        {
            TcpState tcpState = state as TcpState;
            TcpClient tcpClient;
            NetworkStream netStream;
            lock (tcpState)
            {
                if (tcpState.tcpClient == null)
                    return;
                if (tcpState.netStream == null)
                    return;
                tcpClient = tcpState.tcpClient;
                netStream = tcpState.netStream;
            }

            var buffStream = BufferPoolProvider.BufferStreamPool.Alloc();
            try
            {
                if (tcpClient.Client.Poll(0, SelectMode.SelectWrite))
                {
                    bool continueSend;
                    do
                    {
                        buffStream.Clear();
                        lock (_sendState)
                        {
                            for (int i = 0; i < _sendState.sendMsgs.Count; i++)
                            {
                                var msg = _sendState.sendMsgs[i];
                                buffStream.Write(msg);
                            }
                            _sendState.sendMsgs.Clear();
                        }

                        buffStream.Seek(0, System.IO.SeekOrigin.Begin);
                        buffStream.CopyTo(netStream);

                        lock (_sendState)
                        {
                            continueSend = _sendState.sendMsgs.Count > 0;
                            if (!continueSend)
                            {
                                _sendState.sending = false;
                            }
                        }
                    }
                    while (continueSend);
                }
                else
                {
                    ThreadPool.QueueUserWorkItem(SendMainLoopThreaded, state);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                lock (_sendState)
                {
                    _sendState.sending = false;
                }
                TryDisconnect(tcpState, tcpClient, DisconnectReason.Network);
            }
            finally
            {
                BufferPoolProvider.BufferStreamPool.Dealloc(buffStream);
                buffStream = null;
            }
        }

        protected virtual void OnReceiveMessage(Message msg)
        {
            if(!MessageHandlerUtil.TryInvokeHandler(this, msg))
            {
                Debug.LogWarning($"missing message handler !! messageId={MessageNameConverter.Convert(msg.MessageId)}");
            }
        }

        protected abstract void OnConnected();
        protected abstract void OnDisconnected(DisconnectReason reason);
    }

    public abstract class TcpClientBaseForServer<T> : TcpClientBase
        where T : TcpClientBase
    {
        public TcpServerBase<T> Server { get; }

        public TcpClientBaseForServer(TcpServerBase<T> server, TcpClient client)
            : base(client, server.MessageFactory, server.BufferPoolProvider)
        {
            Server = server;
        }

        protected override void OnDisconnected(DisconnectReason reason)
        {
            Server.RemoveClient(this);
        }
    }
}
