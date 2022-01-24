using Core.Framework.Network.Buffers;
using Core.Framework.Network.Data;
using LogUtil;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;

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

        private class SendState
        {
            public bool sending;
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

        private class TcpState
        {
            public IPAddress ip;
            public int port;

            public TcpClient tcpClient;
            public NetworkStream netStream;

            public HeaderBuffer headerBuffer;
            public ContentState contentState = new ContentState();
            public FooterBuffer footerBuffer;
            public Message msg;
        }

        private TcpState _tcpState;

        private ConnectState _connectState;

        private List<Message> _recvMsgs;
        private List<Message> _recvMsgsMainLoop;

        private DisconnetState _disconnetState;
        private SendState _sendState;

        private object[] _onReceObjectArrayCache = new object[1];

        public MessageFactory MessageFactory { get; }
        private BufferPool BufferPool { get; }
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

        protected TcpClientBase(MessageFactory factory, BufferPool pool)
        {
            MessageFactory = factory;
            BufferPool = pool;

            _connectState = new ConnectState();
            _recvMsgs = new List<Message>();
            _recvMsgsMainLoop = new List<Message>();
            _disconnetState = new DisconnetState();
            _sendState = new SendState();

            _tcpState = new TcpState();

            MessageHandlerUtil.Init(GetType());
        }

        protected TcpClientBase(TcpClient client, MessageFactory factory, BufferPool pool)
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
                    if (_tcpState.tcpClient != null && _tcpState.tcpClient.Connected)
                    {
                        Disconnect(DisconnectReason.User, true);
                        lock (_connectState)
                        {
                            _connectState.invoke = false;
                        }
                    }
                }

                lock (_tcpState)
                {
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

        public bool Send(Message msg)
        {
            if(!Connected)
            {
                Debug.Log("Can't Send When Disconnected");
                return false;
            }

            lock(_sendState)
            {
                _sendState.sendMsgs.Add(msg);
                if (!_sendState.sending)
                {
                    _sendState.sending = true;
                    ThreadPool.QueueUserWorkItem(SendMainLoopThreaded, _tcpState);
                }
            }
            return true;
        }

        public void BegineReceive()
        {
            ThreadPool.QueueUserWorkItem(ReceiveHeaderMainLoopThreaded, _tcpState);
        }

        private void ConnectMainLoopThreaded(object state)
        {
            TcpState tcpState = state as TcpState;
            TcpClient tcpClient;
            lock (tcpState)
            {
                if (tcpState.tcpClient == null)
                    return;
                tcpClient = tcpState.tcpClient;
            }

            try
            {
                lock (tcpState)
                {
                    tcpClient.Connect(tcpState.ip, tcpState.port);
                    tcpState.netStream = tcpClient.GetStream();

                    lock (_connectState)
                    {
                        _connectState.invoke = true;
                    }
                }
                ThreadPool.QueueUserWorkItem(ReceiveHeaderMainLoopThreaded, state);
            }
            catch(Exception ex)
            {
                Debug.LogError(ex);
                TryDisconnect(tcpState, tcpClient, DisconnectReason.Network);
            }
        }

        private void ReceiveHeaderMainLoopThreaded(object state)
        {
            TcpState tcpState = state as TcpState;
            TcpClient tcpClient;
            lock(tcpState)
            {
                if (tcpState.tcpClient == null)
                    return;
                tcpClient = tcpState.tcpClient;
            }

            try
            {
                lock (tcpState)
                {

                    if (tcpClient.Client.Poll(0, SelectMode.SelectRead))
                    {
                        if (tcpState.headerBuffer == null)
                            tcpState.headerBuffer = BufferPool.HeaderBufferPool.Alloc();

                        var readCount = Math.Min(tcpClient.Available, tcpState.headerBuffer.RemainingBufferSize);

                        var realReadCount = tcpState.netStream.Read(
                            tcpState.headerBuffer.Buffer, tcpState.headerBuffer.WriteStartPosition, readCount);
                        if (realReadCount == 0)
                        {
                            Disconnect(DisconnectReason.Remote);
                            return;
                        }

                        tcpState.headerBuffer.IncreaseReceivedSize(realReadCount);
                        if (tcpState.headerBuffer.IsEnough)
                        {
                            var header = tcpState.headerBuffer.CreateHeader();
                            BufferPool.HeaderBufferPool.Dealloc(tcpState.headerBuffer);
                            tcpState.headerBuffer = null;
                            if (header.ContentSize == 0)
                            {
                                var msg = MessageFactory.CreateMessage(header, ZeroBuffer, 0);
                                tcpState.msg = msg;
                                ThreadPool.QueueUserWorkItem(ReceiveFooterMainLoopThreaded, state);
                            }
                            else
                            {
                                tcpState.contentState.header = header;
                                tcpState.contentState.receiveSize = 0;

                                ThreadPool.QueueUserWorkItem(ReceiveContentMainLoopThreaded, state);
                            }
                        }
                        else
                        {
                            ThreadPool.QueueUserWorkItem(ReceiveHeaderMainLoopThreaded, state);
                        }
                    }
                    else
                    {
                        ThreadPool.QueueUserWorkItem(ReceiveHeaderMainLoopThreaded, state);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                TryDisconnect(tcpState, tcpClient, DisconnectReason.Network);
            }
        }

        private void ReceiveContentMainLoopThreaded(object state)
        {
            TcpState tcpState = state as TcpState;
            TcpClient tcpClient;
            lock (tcpState)
            {
                if (tcpState.tcpClient == null)
                    return;
                tcpClient = tcpState.tcpClient;
            }

            try
            {
                lock (tcpState)
                {
                    if (tcpClient.Client.Poll(0, SelectMode.SelectRead))
                    {
                        if (tcpState.contentState.LastBuffer == null
                            || tcpState.contentState.LastBuffer.RemainingBufferSize == 0)
                        {
                            tcpState.contentState.buffers.Add(BufferPool.ContentBufferPool.Alloc());
                        }

                        var recvSize = Math.Min(tcpState.contentState.LastBuffer.RemainingBufferSize,
                            tcpState.contentState.header.ContentSize - tcpState.contentState.receiveSize);
                        recvSize = Math.Min(tcpClient.Available, recvSize);
                        
                        var realReadCount = tcpState.netStream.Read(
                            tcpState.contentState.LastBuffer.Buffer, tcpState.contentState.LastBuffer.WriteStartPosition, recvSize);
                        if (realReadCount == 0)
                        {
                            Disconnect(DisconnectReason.Remote);
                            return;
                        }

                        tcpState.contentState.LastBuffer.IncreaseReceivedSize(realReadCount);
                        tcpState.contentState.receiveSize += realReadCount;

                        if (tcpState.contentState.header.ContentSize == tcpState.contentState.receiveSize)
                        {
                            var msg = MessageFactory.CreateMessage(tcpState.contentState.header, tcpState.contentState.buffers);
                            tcpState.msg = msg;

                            foreach (var buf in tcpState.contentState.buffers)
                            {
                                BufferPool.ContentBufferPool.Dealloc(buf);
                            }
                            tcpState.contentState.buffers.Clear();

                            ThreadPool.QueueUserWorkItem(ReceiveFooterMainLoopThreaded, state);
                        }
                        else
                        {
                            ThreadPool.QueueUserWorkItem(ReceiveContentMainLoopThreaded, state);
                        }
                    }
                    else
                    {
                        ThreadPool.QueueUserWorkItem(ReceiveContentMainLoopThreaded, state);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                TryDisconnect(tcpState, tcpClient, DisconnectReason.Network);
            }
        }

        private void ReceiveFooterMainLoopThreaded(object state)
        {
            TcpState tcpState = state as TcpState;
            TcpClient tcpClient;
            lock (tcpState)
            {
                if (tcpState.tcpClient == null)
                    return;
                tcpClient = tcpState.tcpClient;
            }

            try
            {
                lock (tcpState)
                {
                    if (tcpClient.Client.Poll(0, SelectMode.SelectRead))
                    {
                        if (tcpState.footerBuffer == null)
                            tcpState.footerBuffer = BufferPool.FooterBufferPool.Alloc();

                        var readCount = Math.Min(tcpClient.Available, tcpState.footerBuffer.RemainingBufferSize);

                        var realReadCount = tcpState.netStream.Read(
                            tcpState.footerBuffer.Buffer, tcpState.footerBuffer.WriteStartPosition, readCount);
                        if (realReadCount == 0)
                        {
                            Disconnect(DisconnectReason.Remote);
                            return;
                        }

                        tcpState.footerBuffer.IncreaseReceivedSize(realReadCount);

                        if (tcpState.footerBuffer.IsEnough)
                        {
                            var footer = tcpState.footerBuffer.CreateFooter();
                            tcpState.msg.Footer = footer;
                            lock (_recvMsgs)
                            {
                                _recvMsgs.Add(tcpState.msg);
                            }

                            BufferPool.FooterBufferPool.Dealloc(tcpState.footerBuffer);
                            tcpState.footerBuffer = null;

                            ThreadPool.QueueUserWorkItem(ReceiveHeaderMainLoopThreaded, state);
                        }
                        else
                        {
                            ThreadPool.QueueUserWorkItem(ReceiveFooterMainLoopThreaded, state);
                        }
                    }
                    else
                    {
                        ThreadPool.QueueUserWorkItem(ReceiveFooterMainLoopThreaded, state);
                    }
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

            try
            {
                if (tcpClient.Client.Poll(0, SelectMode.SelectWrite))
                {
                    var sendBuffer = BufferPool.SendBufferPool.Alloc();

                    lock (_sendState)
                    {
                        int index = 0;
                        for (; index < _sendState.sendMsgs.Count; index++)
                        {
                            var msg = _sendState.sendMsgs[index];
                            if (!sendBuffer.WriteToBuffer(msg))
                            {
                                break;
                            }
                        }
                        _sendState.sendMsgs.RemoveRange(0, index);
                    }

                    netStream.Write(sendBuffer.Buffer, sendBuffer.SentSize, sendBuffer.UnsendSize);

                    BufferPool.SendBufferPool.Dealloc(sendBuffer);
                    sendBuffer = null;

                    lock (_sendState)
                    {
                        if (_sendState.sendMsgs.Count > 0)
                        {
                            ThreadPool.QueueUserWorkItem(SendMainLoopThreaded, state);
                        }
                        else
                        {
                            _sendState.sending = false;
                        }
                    }
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
        }

        protected virtual void OnReceiveMessage(Message msg)
        {
            var handleInfo = MessageHandlerUtil.GetHandlerMethod(GetType(), msg.MessageId);
            if (handleInfo.HasValue)
            {
                _onReceObjectArrayCache[0] = msg.GetData(handleInfo.Value.paramType);
                handleInfo.Value.methodInfo.Invoke(this, _onReceObjectArrayCache);
            }
        }

        protected abstract void OnConnected();
        protected abstract void OnDisconnected(DisconnectReason reason);
    }

    public class MessageHandlerAttribute : Attribute
    {
        public int MessageId { get; }

        public MessageHandlerAttribute(int messageID)
        {
            MessageId = messageID;
        }
    }

    public static class MessageHandlerUtil
    {
        public struct HandlerInfo
        {
            public MethodInfo methodInfo;
            public Type paramType;
        }

        private static Dictionary<Type, Dictionary<int, HandlerInfo>> s_typeToHandlersDic 
            = new Dictionary<Type, Dictionary<int, HandlerInfo>>();

        public static void Init(Type type)
        {
            if (type == null)
                return;
            if (s_typeToHandlersDic.ContainsKey(type))
                return;

            var handlers = new Dictionary<int, HandlerInfo>();
            foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                var attr = method.GetCustomAttribute<MessageHandlerAttribute>();
                if (attr != null)
                {
                    if(handlers.TryGetValue(attr.MessageId, out var _))
                    {
                        Debug.LogError($"duplicate message handler method={method.Name} MessageId={attr.MessageId}!!");
                    }
                    else
                    {
                        var paramArray = method.GetParameters();
                        if (paramArray.Length == 1)
                        {
                            var parameter = paramArray[0];
                            if (typeof(Google.Protobuf.IMessage).IsAssignableFrom(parameter.ParameterType))
                            {
                                handlers.Add(attr.MessageId, new HandlerInfo() { methodInfo= method, paramType = parameter.ParameterType });
                            }
                        }
                    }
                }
            }
            if (handlers.Count > 0)
            {
                lock (s_typeToHandlersDic)
                {
                    s_typeToHandlersDic[type] = handlers;
                }
            }
        }

        public static HandlerInfo? GetHandlerMethod(Type handlerType, int messageId)
        {
            Dictionary<int, HandlerInfo> handlerDic;
            lock(s_typeToHandlersDic)
            {
                if (!s_typeToHandlersDic.TryGetValue(handlerType, out handlerDic))
                {
                    return null;
                }
            }
            if (!handlerDic.TryGetValue(messageId, out var m))
            {
                return null;
            }
            return m;
        }
    }
}
