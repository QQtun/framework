using Core.Framework.Network.Buffers;
using Core.Framework.Network.Data;
using LogUtil;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Core.Framework.Network
{
    public abstract class TcpClientBase
    {
        private static readonly byte[] ZeroBuffer = new byte[0];

        private class ContentState
        {
            public Header header;
            public int receiveSize;
            public List<ContentBuffer> buffers = new List<ContentBuffer>();

            public ContentBuffer LastBuffer
            {
                get
                {
                    if (buffers.Count == 0)
                        return null;
                    return buffers[buffers.Count - 1];
                }
            }
        }

        private class DisconnetState
        {
            public bool invoke;
            public DisconnectReason reason;
        }

        private class SendState
        {
            public bool sending;
            public List<Message> sendMsgs = new List<Message>();
        }

        private class ConnectState
        {
            public bool invoke;
        }

        private class TcpState
        {
            public TcpClient tcpClient;
            public NetworkStream netStream;
            public Message msg;
        }

        protected TcpClient tcpClient;
        protected NetworkStream netStream;

        private ConnectState _connectState;
        private HeaderBuffer _headerBuffer;
        private ContentState _contentState;
        private FooterBuffer _footerBuffer;

        private List<Message> _recvMsgs;
        private List<Message> _recvMsgsMainLoop;

        private DisconnetState _disconnetState;
        private SendState _sendState;
        private SendBuffer _sendBuffer;

        public MessageFactory MessageFactory { get; }
        public ContentBufferPool ContentBufferPool { get; }
        public EndPoint RemoteEndPoint => tcpClient?.Client.RemoteEndPoint ?? null;
        public bool Connected => tcpClient?.Connected ?? false;

        protected TcpClientBase(MessageFactory factory, ContentBufferPool pool)
        {
            MessageFactory = factory;
            ContentBufferPool = pool;

            _connectState = new ConnectState();
            _headerBuffer = new HeaderBuffer();
            _contentState = new ContentState();
            _footerBuffer = new FooterBuffer();
            _recvMsgs = new List<Message>();
            _recvMsgsMainLoop = new List<Message>();
            _disconnetState = new DisconnetState();
            _sendState = new SendState();
            _sendBuffer = new SendBuffer(pool.BufferSize + Header.HeaderSize + Footer.FooterSize);
        }

        protected TcpClientBase(TcpClient client, MessageFactory factory, ContentBufferPool pool)
            : this(factory, pool)
        {
            tcpClient = client;
            netStream = tcpClient.GetStream();
        }

        public bool Connect(IPAddress ip, int port)
        {
            try
            {
                if (tcpClient != null)
                {
                    lock(tcpClient)
                    {
                        if(tcpClient.Connected)
                            Disconnect(DisconnectReason.User);
                    }
                }

                lock(_connectState)
                {
                    _connectState.invoke = false;
                }

                tcpClient = new TcpClient();
                var state = new TcpState();
                state.tcpClient = tcpClient;
                tcpClient.BeginConnect(ip, port, OnConnectAsync, state);

                return true;
            }
            catch(Exception ex)
            {
                Debug.LogError(ex);
                return false;
            }
        }

        public void Disconnect(DisconnectReason reason)
        {
            if (tcpClient != null)
            {
                lock (tcpClient)
                {
                    netStream.Close();
                    netStream = null;
                    tcpClient.Close();
                    tcpClient = null;
                }
                lock(_disconnetState)
                {
                    _disconnetState.invoke = true;
                    _disconnetState.reason = reason;
                }
            }
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

        public void Send(Message msg)
        {
            lock(_sendState)
            {
                _sendState.sendMsgs.Add(msg);
                if (!_sendState.sending)
                    BegineSend();
            }
        }

        public void BegineReceive()
        {
            var state = new TcpState();
            state.tcpClient = tcpClient;
            state.netStream = netStream;
            BegineReceiveHeader(true, state);
        }

        private void BegineSend()
        {
            try
            {
                lock(_sendState)
                {
                    _sendBuffer.Reset();
                    int index = 0;
                    for(;index< _sendState.sendMsgs.Count;index++)
                    {
                        var msg = _sendState.sendMsgs[index];
                        if (!_sendBuffer.WriteToBuffer(msg))
                        {
                            break;
                        }
                    }
                    _sendState.sendMsgs.RemoveRange(0, index);
                    _sendState.sending = true;
                }

                netStream.BeginWrite(_sendBuffer.Buffer, _sendBuffer.SentSize, _sendBuffer.UnsendSize, OnSentAsync, netStream);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                Disconnect(DisconnectReason.Network);
            }
        }

        private void OnSentAsync(IAsyncResult ar)
        {
            try
            {
                var netStream = (NetworkStream)ar.AsyncState;
                netStream.EndWrite(ar);

                lock (_sendState)
                {
                    if (_sendState.sendMsgs.Count > 0)
                    {
                        BegineSend();
                    }
                    else
                    {
                        _sendState.sending = false;
                    }
                }
            }
            catch (ObjectDisposedException ex)
            {
                Debug.Log(ex);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                Disconnect(DisconnectReason.Network);
            }
        }

        private void OnConnectAsync(IAsyncResult ar)
        {
            try
            {
                var state = (TcpState) ar.AsyncState;
                state.tcpClient.EndConnect(ar);

                if(state.tcpClient.Connected)
                {
                    netStream = state.tcpClient.GetStream();
                    state.netStream = netStream;
                    lock (_connectState)
                    {
                        _connectState.invoke = true;
                    }
                    BegineReceiveHeader(true, state);
                }
                else
                {
                    Disconnect(DisconnectReason.Network);
                }
            }
            catch (ObjectDisposedException ex)
            {
                Debug.Log(ex);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                Disconnect(DisconnectReason.Network);
            }
        }

        private void BegineReceiveHeader(bool reset, TcpState state)
        {
            if(reset)
            {
                _headerBuffer.Reset();
            }

            try
            {
                netStream.BeginRead(_headerBuffer.Buffer, _headerBuffer.WriteStartPosition, _headerBuffer.RemainingBufferSize, OnReceiveHeaderAsync, state);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                Disconnect(DisconnectReason.Network);
            }
        }

        private void OnReceiveHeaderAsync(IAsyncResult ar)
        {
            try
            {
                var state = (TcpState)ar.AsyncState;
                var netStream = state.netStream;
                var readSize = netStream.EndRead(ar);
                _headerBuffer.IncreaseReceivedSize(readSize);

                if (_headerBuffer.IsEnough)
                {
                    var header = _headerBuffer.CreateHeader();
                    if(header.ContentSize == 0)
                    {
                        var msg = MessageFactory.CreateMessage(header, ZeroBuffer, 0);
                        state.msg = msg;
                        BegineReceiveFooter(true, state);
                    }
                    else
                    {
                        _contentState.header = header;
                        _contentState.receiveSize = 0;

                        BegineReceiveContent(state);
                    }
                }
                else
                {
                    BegineReceiveHeader(false, state);
                }
            }
            catch(ObjectDisposedException ex)
            {
                Debug.Log(ex);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                Disconnect(DisconnectReason.Network);
            }
        }

        private void BegineReceiveContent(TcpState state)
        {
            try
            {
                if(_contentState.LastBuffer == null
                    || _contentState.LastBuffer.RemainingBufferSize == 0)
                {
                    _contentState.buffers.Add(ContentBufferPool.Alloc());
                }

                var recvSize = Math.Min(_contentState.LastBuffer.RemainingBufferSize,
                    _contentState.header.ContentSize - _contentState.receiveSize);
                netStream.BeginRead(_contentState.LastBuffer.Buffer, _contentState.LastBuffer.WriteStartPosition, recvSize, OnReceiveContentAsync, state);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                Disconnect(DisconnectReason.Network);
            }
        }

        private void OnReceiveContentAsync(IAsyncResult ar)
        {
            try
            {
                var state = (TcpState)ar.AsyncState;
                var netStream = state.netStream;
                var readSize = netStream.EndRead(ar);
                _contentState.LastBuffer.IncreaseReceivedSize(readSize);
                _contentState.receiveSize += readSize;

                if (_contentState.header.ContentSize == _contentState.receiveSize)
                {
                    var msg = MessageFactory.CreateMessage(_contentState.header, _contentState.buffers);
                    state.msg = msg;

                    foreach (var buf in _contentState.buffers)
                    {
                        ContentBufferPool.Dealloc(buf);
                    }
                    _contentState.buffers.Clear();

                    BegineReceiveFooter(true, state);
                }
                else
                {
                    BegineReceiveContent(state);
                }
            }
            catch (ObjectDisposedException ex)
            {
                Debug.Log(ex);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                Disconnect(DisconnectReason.Network);
            }
        }

        private void BegineReceiveFooter(bool reset, TcpState state)
        {
            if (reset)
            {
                _footerBuffer.Reset();
            }

            try
            {
                netStream.BeginRead(_footerBuffer.Buffer, _footerBuffer.WriteStartPosition, _footerBuffer.RemainingBufferSize, OnReceiveFooterAsync, state);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                Disconnect(DisconnectReason.Network);
            }
        }

        private void OnReceiveFooterAsync(IAsyncResult ar)
        {
            try
            {
                var state = (TcpState)ar.AsyncState;
                var netStream = state.netStream;
                var msg = state.msg;
                var readSize = netStream.EndRead(ar);
                _footerBuffer.IncreaseReceivedSize(readSize);

                if (_footerBuffer.IsEnough)
                {
                    var footer = _footerBuffer.CreateFooter();
                    msg.Footer = footer;
                    lock (_recvMsgs)
                    {
                        _recvMsgs.Add(msg);
                    }

                    BegineReceiveHeader(true, state);
                }
                else
                {
                    BegineReceiveFooter(false, state);
                }
            }
            catch (ObjectDisposedException ex)
            {
                Debug.Log(ex);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                Disconnect(DisconnectReason.Network);
            }
        }

        protected abstract void OnConnected();
        protected abstract void OnReceiveMessage(Message msg);
        protected abstract void OnDisconnected(DisconnectReason reason);
    }
}
