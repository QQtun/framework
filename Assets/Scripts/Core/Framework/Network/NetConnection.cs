//using Core.Framework.Network.Buffers;
//using Core.Framework.Network.Data;
//using Core.Framework.Utility;
//using LogUtil;
//using System;
//using System.Collections.Generic;
//using System.Net;
//using System.Net.Sockets;
//using System.Threading;

//namespace Core.Framework.Network
//{
//    public enum DisconnectReason
//    {
//        None,
//        User,
//        Remote,
//        ForGameServer,
//        Network,
//        TimeOut,
//        KickedByOthers,
//        LoginFailed,
//        ServerShutdown,
//        KeepAliveFail,
//        Banned,
//        RetryConnection,
//    }

//    /// <summary>
//    /// Author: chengdundeng
//    /// Date: 2019/11/28
//    /// Desc: 網路狀態監聽器, 配合NetConnection使用
//    /// </summary>
//    public interface IConnectionListener
//    {
//        void OnConnected();

//        void OnReceiveMessage(Message msg);

//        void OnDisconnected(DisconnectReason reason);
//    }

//    /// <summary>
//    /// Author: chengdundeng
//    /// Date: 2019/11/28
//    /// Desc: 網路連線介面
//    /// </summary>
//    public interface INetConnection
//    {
//        /// <summary>
//        /// Connect 名稱 方便偵錯
//        /// </summary>
//        string Name { get; }

//        /// <summary>
//        /// Connect 名稱+ID 方便偵錯
//        /// </summary>
//        string NameID { get; }

//        /// <summary>
//        /// Server Ip位置
//        /// </summary>
//        IPAddress Ip { get; }

//        /// <summary>
//        /// Server Port
//        /// </summary>
//        int Port { get; }

//        int BufferSize { get; }

//        int RetryCount { get; }

//        int ConnectTimeout { get; } // 單位: ms

//        int SendTimeout { get; } // 單位: ms

//        int ReceiveTimeout { get; } // 單位: ms

//        /// <summary>
//        /// Socket是否Connected
//        /// </summary>
//        bool IsConnected { get; }

//        /// <summary>
//        /// 本地IP位置
//        /// </summary>
//        EndPoint LocalEndPoint { get; }

//        /// <summary>
//        /// 訊息工廠
//        /// </summary>
//        MessageFactory MsgFactory { get; }

//        void SetConnectionListener(IConnectionListener listener);

//        /// <summary>
//        /// 建立連線
//        /// </summary>
//        /// <returns></returns>
//        bool Connect(ServerSet set);

//        /// <summary>
//        /// 客戶端主動斷線, 默認斷線原因為User
//        /// </summary>
//        void Disconnect();

//        /// <summary>
//        /// 客戶端主動斷線
//        /// </summary>
//        /// <param name="reason">斷線原因</param>
//        void Disconnect(DisconnectReason reason);

//        /// <summary>
//        /// 使用人必須Per Frame呼叫
//        /// </summary>
//        void Update();

//        void Send(Message message);
//    }

//    /// <summary>
//    /// Author: chengdundeng
//    /// Date: 2019/11/28
//    /// Desc: 網路連線，實作Connect, Send等相關功能
//    /// 須配合IConnectionListener去接收網路狀態與封包
//    /// 外部必須per frame呼叫Update函式
//    /// </summary>
//    public class NetConnection : INetConnection, IDisposable
//    {
//        private const int SocketReceivePollTime = 100000;
//        private const int SocketSendPollTime = 5000;

//        private enum State
//        {
//            None,
//            Connecting,
//            Connected,
//            Disconnecting,
//            Disconnected
//        }

//        private enum RecvState
//        {
//            None,
//            Header,
//            Content
//        }

//        private enum SendState
//        {
//            None,
//            Start,
//            Sending
//        }

//        private struct MessageWrap
//        {
//            public int socketId;
//            public Message message;
//            public int sequenceId;
//        }

//        private static readonly object SocketIdLock = new object();
//        private static readonly byte[] ZeroBuffer = new byte[0];

//        private static int sOcketId;

//        private IConnectionListener mConnectionListener;
//        private ContentBuffer mContentBuffer;
//        private DisconnectReason mDisconnectReason;
//        private bool mDisposed;
//        private HeaderBuffer mHeaderBuffer;
//        private object mReceiveListLock = new object();
//        private LinkedList<MessageWrap> mReceivedMsgList = new LinkedList<MessageWrap>();
//        private List<Message> mSentCallbacks = new List<Message>();
//        private int mRetriedCount;
//        private SendBuffer mSendBuffer;
//        private object mSendListLock = new object();
//        private LinkedList<MessageWrap> mSendMessageList = new LinkedList<MessageWrap>();
//        private int mSocketId;
//        private Socket mSocket;
//        private FiniteState<State> mState = new FiniteState<State>(State.None);
//        private Queue<Message> mQueueCallbacks = new Queue<Message>();
//        private List<Message> mExecutingCalls = new List<Message>(100);
//        private Thread mNetworkThread;
//        private FiniteState<RecvState> mRecvState = new FiniteState<RecvState>(RecvState.None);
//        private FiniteState<SendState> mSendState = new FiniteState<SendState>(SendState.None);
//        private string mTag;
//        private int mSequenceSentId;
//        private int mSequenceRecvId;
//        private ReceiveBuffer mSocketBuffer;

//        private System.Diagnostics.Stopwatch mReceiveTimer = new System.Diagnostics.Stopwatch();
//        private System.Diagnostics.Stopwatch mSentTimer = new System.Diagnostics.Stopwatch();

//        public string Name { get; private set; }
//        public string NameID { get; private set; }
//        public IPAddress Ip { get; private set; }
//        public int Port { get; private set; }
//        public int BufferSize { get; private set; }
//        public int RetryCount { get; private set; }
//        public int ConnectTimeout { get; private set; }
//        public int SendTimeout { get; private set; }
//        public int ReceiveTimeout { get; private set; }

//        public DisconnectReason LastDisconnectReason => mDisconnectReason;

//        public bool IsConnected
//        {
//            get
//            {
//                if (mSocket == null) return false;
//                return mSocket.Connected;
//            }
//        }

//        public bool NoDelay { get; private set; }

//        public EndPoint LocalEndPoint
//        {
//            get
//            {
//                if (mSocket == null)
//                    return null;
//                return mSocket.LocalEndPoint;
//            }
//        }

//        public MessageFactory MsgFactory { get; private set; }

//        /// <summary>
//        ///
//        /// </summary>
//        /// <param name="name">Connect名稱 方便偵錯</param>
//        /// <param name="bufferSize"></param>
//        /// <param name="retryCount">斷線發生時 自動重連的重試次數</param>
//        /// <param name="connectTimeout"></param>
//        /// <param name="sendTimeout"></param>
//        /// <param name="receiveTimeout"></param>
//        /// <param name="noDelay"></param>
//        /// <param name="factory">訊息工廠</param>
//        public NetConnection(ServerConfig config, MessageFactory factory)
//        {
//            Name = config.name;
//            NameID = config.name + ":?";
//            BufferSize = config.bufferSize;
//            RetryCount = config.retryCount;
//            ConnectTimeout = config.connectTimeout;
//            SendTimeout = config.sendTimeout;
//            ReceiveTimeout = config.receiveTimeout;
//            NoDelay = config.noDelay;
//            MsgFactory = factory;

//            mRetriedCount = 0;
//            mSequenceSentId = 0;
//            mSequenceRecvId = 0;
//            mHeaderBuffer = new HeaderBuffer();
//            mContentBuffer = new ContentBuffer(BufferSize);
//            mSendBuffer = new SendBuffer(BufferSize);
//            mSocketBuffer = new ReceiveBuffer(BufferSize);
//        }

//        public void SetConnectionListener(IConnectionListener listner)
//        {
//            mConnectionListener = listner;
//        }

//        public bool Connect(ServerSet set)
//        {
//            if (mState.Current != State.None
//                && mState.Current != State.Disconnected)
//            {
//                Debug.LogErrorFormat("({0})can't connect now, current state: {1}",
//                    NameID, mState.Current, LogTag.Network);
//                return false;
//            }

//            if (!IPAddress.TryParse(set.ip, out var ip))
//            {
//                Debug.LogErrorFormat("({0})ip parse failed: {1}",
//                    NameID, set.ip, LogTag.Network);
//                return false;
//            }

//            Ip = ip;
//            Port = set.port;

//            mRetriedCount = 0;
//            mState.Transit(State.Connecting);

//            return true;
//        }

//        public void Disconnect()
//        {
//            Disconnect(DisconnectReason.User);
//        }

//        public void Disconnect(DisconnectReason reason)
//        {
//            if (mState.Current == State.Disconnected)
//                return;
//            mState.Transit(State.Disconnected);
//            mState.Tick();
//            mDisconnectReason = reason;
//            DoDisconnect(false);
//        }

//        public void Send(Message message)
//        {
//            if (!IsConnected)
//            {
//                Debug.LogWarningFormat("({0})should not send message when disconnected! id: {1}",
//                    NameID, MessageNameConverter.Convert(message.MessageId), LogTag.Network);
//                return;
//            }

//            lock (mSendListLock)
//            {
//                mSendMessageList.AddLast(new MessageWrap()
//                {
//                    socketId = mSocketId,
//                    message = message,
//                    sequenceId = ++mSequenceSentId,
//                });
//            }
//        }

//        public void Dispose()
//        {
//            Dispose(true);
//        }

//        protected virtual void Dispose(bool final)
//        {
//            if (!mDisposed)
//            {
//                Disconnect();
//                mDisposed = true;
//            }
//        }

//        public void Update()
//        {
//            if (mDisposed)
//                return;
//            switch (mState.Tick())
//            {
//                case State.None:
//                {
//                    break;
//                }
//                case State.Connecting:
//                {
//                    if (mState.Entering)
//                    {
//                        DoConnect();
//                        if (mNetworkThread == null)
//                        {
//                            mNetworkThread = new Thread(NetworkThreadMainProcess);
//                            mNetworkThread.Start();
//                        }
//                    }
//                    break;
//                }
//                case State.Connected:
//                {
//                    if (mState.Entering)
//                    {
//                        lock (mSendListLock)
//                        {
//#if DEBUG
//                            Debug.LogFormat("({0})clear sendMessageList", NameID, LogTag.Network);
//                            foreach (MessageWrap sendMessageWrap in mSendMessageList)
//                            {
//                                Debug.LogFormat("({0})clear sendMessageList, messageId: {1}",
//                                    NameID, MessageNameConverter.Convert(sendMessageWrap.message.MessageId), LogTag.Network);
//                                sendMessageWrap.message.Release();
//                            }
//#endif
//                            mSendMessageList.Clear();
//                        }
//                        if (mConnectionListener != null)
//                        {
//                            mRetriedCount = 0;
//                            mConnectionListener.OnConnected();
//                        }

//                        lock (mReceiveListLock)
//                        {
//                            foreach (MessageWrap messageWrap in mReceivedMsgList)
//                            {
//                                messageWrap.message.Release();
//                            }
//                            mReceivedMsgList.Clear();
//                        }
//                    }

//                    ExecuteOnSend();
//                    DispatchReceivedMessage();
//                    break;
//                }
//                case State.Disconnecting:
//                {
//                    mState.Transit(State.Disconnected);
//                    break;
//                }
//                case State.Disconnected:
//                {
//                    if (mState.Entering)
//                    {
//                        DoDisconnect(true);
//                    }
//                    break;
//                }
//                default:
//                    throw new ArgumentOutOfRangeException();
//            }
//        }

//        private void DoConnect()
//        {
//            if (Ip == null)
//                return;

//            try
//            {
//                lock (SocketIdLock)
//                {
//                    mSocketId = sOcketId++;
//                }
//                NameID = string.Format("{0}:{1}", Name, mSocketId);
//                mSocket = new Socket(Ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
//                mSocket.NoDelay = NoDelay;
//                mSocket.SendTimeout = SendTimeout;
//                mSocket.ReceiveTimeout = ReceiveTimeout;

//                Debug.LogFormat("({0})NetConnection DoConnect, ip: {1}, port: {2}", NameID, Ip, Port, LogTag.Network);
//            }
//            catch (Exception e)
//            {
//                Debug.LogErrorFormat("({0})NetConnection DoConnect failed !!! e: {1}", NameID, e, LogTag.Network);
//                GoToDisconnect();
//            }
//        }

//        // from socket thread
//        private static void OnConnectedAsync(IAsyncResult result)
//        {
//            var connection = result.AsyncState as NetConnection;
//            if (connection == null || connection.mSocket == null)
//                return;

//            try
//            {
//                var socket = connection.mSocket;
//                socket.EndConnect(result);

//                Debug.LogFormat("({0})NetConnection OnConnectedAsync", connection.NameID, LogTag.Network);
//                connection.GoToConnected();
//            }
//            catch (Exception e)
//            {
//                Debug.LogErrorFormat("({0})NetConnection EndConnect failed !!! e: {1}", connection.NameID, e, LogTag.Network);
//                connection.GoToDisconnect();
//            }
//        }

//        private void GoToConnected()
//        {
//            if (IsConnected)
//            {
//                mSequenceSentId = 0;
//                mSequenceRecvId = 0;
//                mHeaderBuffer.Reset();
//                mContentBuffer.Reset();
//                mSendBuffer.Reset();
//                mRecvState.Transit(RecvState.Header);
//                mSendState.Transit(SendState.Start);
//                mState.Transit(State.Connected);
//            }
//            else
//            {
//                Debug.LogErrorFormat("({0})NetConnection EndConnect but not connected !!!", NameID, LogTag.Network);
//                GoToDisconnect();
//            }
//        }

//        private void DoDisconnect(bool retry)
//        {
//            if (mSocket == null)
//                return;

//            lock (mSocket)
//            {
//                try
//                {
//                    Debug.LogFormat("({0})NetConnection DoDisconnect", NameID, LogTag.Network);

//                    if (mSocket.Connected)
//                    {
//                        mSocket.Shutdown(SocketShutdown.Both);
//                    }
//                }
//                catch (Exception e)
//                {
//                    Debug.LogErrorFormat("({0})NetConnection Shutdown failed !!! e: {1}", NameID, e, LogTag.Network);
//                }
//                finally
//                {
//                    mSocket.Close();
//                    mSocket = null;

//                    if (mNetworkThread != null)
//                    {
//                        if (!mNetworkThread.Join(50))
//                        {
//                            mNetworkThread.Interrupt();
//                        }
//                        mNetworkThread = null;
//                    }
//                }

//                lock (mReceiveListLock)
//                {
//                    foreach (MessageWrap messageWrap in mReceivedMsgList)
//                    {
//                        messageWrap.message.Release();
//                    }
//                    mReceivedMsgList.Clear();
//                }
//                Debug.LogFormat("({0})NetConnection DoDisconnect End", NameID, LogTag.Network);
//            }

//            if (retry && mRetriedCount < RetryCount)
//            {
//                if (mConnectionListener != null)
//                    mConnectionListener.OnDisconnected(DisconnectReason.RetryConnection);

//                mRetriedCount++;
//                mState.Transit(State.Connecting);
//                Debug.LogFormat("({0})NetConnection Retry Connecting Time: {1}", NameID, mRetriedCount, LogTag.Network);
//            }
//            else
//            {
//                if (mConnectionListener != null)
//                    mConnectionListener.OnDisconnected(mDisconnectReason);
//            }
//        }

//        // main thread or netwokr thread
//        private void GoToDisconnect(DisconnectReason reason = DisconnectReason.Network)
//        {
//            if (mDisposed)
//                return;

//            if (mState.Current != State.Disconnecting
//                && mState.Current != State.Disconnected)
//            {
//                mDisconnectReason = reason;
//                mState.Transit(State.Disconnecting);
//                mState.Tick();
//            }
//        }

//        private void DispatchReceivedMessage()
//        {
//            lock (mReceiveListLock)
//            {
//                var node = mReceivedMsgList.First;
//                while (node != null)
//                {
//                    MessageWrap msg = node.Value;
//                    var nextNode = node.Next;
//                    mReceivedMsgList.Remove(node);
//                    node = nextNode;

//                    try
//                    {
//                        if (mConnectionListener != null)
//                        {
//                            if (msg.message != null)
//                            {
//                                mConnectionListener.OnReceiveMessage(msg.message);
//                                msg.message.Release();
//                            }
//                            else
//                            {
//                                Debug.LogWarning("NULL MESSAGE? on Message seq id:" + msg.sequenceId);
//                            }
//                        }
//                    }
//                    catch (Exception e)
//                    {
//                        Debug.LogErrorFormat("({0})handle message failed !!! e: {1}",
//                            NameID, e.Message, LogTag.Network);
//                        throw;
//                    }
//                }
//            }
//        }

//        // NetworkThread
//        private void NetworkThreadMainProcess()
//        {
//            State networkState = mState.Current;
//            while (networkState != State.Disconnected)
//            {
//                try
//                {
//                    switch (networkState)
//                    {
//                        case State.None:
//                        {
//                            break;
//                        }
//                        case State.Connecting:
//                        {
//                            NetworkThreadOnConnectingProcess();
//                            break;
//                        }
//                        case State.Connected:
//                        {
//                            NetworkThreadOnConnectedProcess();
//                            break;
//                        }
//                        case State.Disconnecting:
//                        {
//                            break;
//                        }
//                        case State.Disconnected:
//                        {
//                            break;
//                        }
//                    }
//                    networkState = mState.Current;

//                    if (networkState != State.Disconnected)
//                        Thread.Sleep(16); // 1000ms / 60frame
//                }
//                catch (ThreadInterruptedException e)
//                {
//                    Debug.LogWarningFormat("({0})NetworkThreadMainProcess [{2}] ThreadInterruptedException e: {1}",
//                        NameID, e, networkState, LogTag.Network);
//                    break;
//                }
//            }
//        }

//        // NetworkThread
//        private void NetworkThreadOnConnectingProcess()
//        {
//            if (mSocket == null)
//                return;

//            if (mSocket.Connected)
//                return;

//            try
//            {
//                IAsyncResult result = mSocket.BeginConnect(Ip, Port, null, this);
//                var handler = result.AsyncWaitHandle;
//                bool success = handler.WaitOne(ConnectTimeout);

//                if (success && result.IsCompleted)
//                {
//                    OnConnectedAsync(result);
//                }
//                else
//                {
//                    Debug.LogWarningFormat("({0})NetworkThreadOnConnectingProcess Timeout {1} secs.",
//                        NameID, ConnectTimeout, LogTag.Network);
//                    GoToDisconnect(DisconnectReason.TimeOut);
//                }
//                handler.Close();
//            }
//            catch (Exception e)
//            {
//                Debug.LogWarningFormat("({0})NetworkThreadOnConnectingProcess Exception: {1}",
//                    NameID, e, LogTag.Network);

//                GoToDisconnect(DisconnectReason.TimeOut);
//            }
//        }

//        // NetworkThread
//        private void NetworkThreadOnConnectedProcess()
//        {
//            if (mSocket == null)
//                return;

//            if (!mSocket.Connected)
//            {
//                GoToDisconnect();
//                return;
//            }

//            lock (mSocket)
//            {
//                NetworkThreadWriteProcess();
//            }

//            if (mSocket == null)
//                return;

//            if (!mSocket.Connected)
//                return;

//            lock (mSocket)
//            {
//                NetworkThreadReadProcess();
//            }

//            if (mSocket == null)
//                return;

//            if (!mSocket.Connected)
//                return;

//            lock (mSocket)
//            {
//                if (mSocket.Poll(SocketSendPollTime, SelectMode.SelectError))
//                {
//                    GoToDisconnect();
//                }
//            }
//        }

//        // NetworkThread
//        private void NetworkThreadReadProcess()
//        {
//            if (mSocket.Poll(SocketReceivePollTime, SelectMode.SelectRead))
//            {
//                SocketError error = SocketError.Success;
//                int receivedSize = 0;
//                bool hasError = false;
//                int blockProcessed = 0;
//                bool hasMoreData = false;

//                Message msgTmp = null;

//                do
//                {
//                    var segment = mSocketBuffer.GetWriteSegment();
//                    hasMoreData = mSocket.Available > segment.Count;

//                    try
//                    {
//                        int readSize = Math.Max(0, Math.Min(mSocket.Available, segment.Count));
//                        receivedSize = mSocket.Receive(segment.Array, segment.Offset, readSize, SocketFlags.None, out error);
//                    }
//                    catch (SocketException e)
//                    {
//                        // socket出錯
//                        Debug.LogErrorFormat("({0})NetConnection Receive Header socket failed !!! ErrorCode: {1}, e: {2}",
//                            NameID, e.ErrorCode, e, LogTag.Network);
//                        GoToDisconnect();
//                        hasError = true;
//                    }
//                    catch (ThreadInterruptedException e)
//                    {
//                        Debug.LogWarningFormat("({0})NetConnection Receive Header ThreadInterrupted !! e: {1}",
//                            NameID, e, LogTag.Network);
//                        hasError = true;
//                    }
//                    catch (Exception e)
//                    {
//                        // ignored
//                        Debug.LogWarningFormat("({0})NetConnection Receive Data happened some error !!! e: {1}",
//                            NameID, e, LogTag.Network);
//                    }

//                    if (error != SocketError.Success || hasError)
//                    {
//                        return;
//                    }

//                    if (receivedSize > 0)
//                    {
//                        mSocketBuffer.IncreaseReceivedSize(receivedSize);
//                    }

//                    bool handleNextData = mSocketBuffer.ReceivedSize > 0;

//                    int msgProcessed = 0;
//                    while (handleNextData)
//                    {
//                        int blockSize = 0;
//                        switch (mRecvState.Tick())
//                        {
//                            case RecvState.None:
//                            {
//                                handleNextData = false;
//                                break;
//                            }
//                            case RecvState.Header:
//                            {
//                                blockSize = Math.Min(mHeaderBuffer.RemainingBufferSize, mSocketBuffer.ReceivedSize);
//                                if (blockSize > 0 && mSocketBuffer.ReceivedSize >= blockSize)
//                                {
//                                    Array.Copy(mSocketBuffer.Buffer, mSocketBuffer.ReadStartPosition, mHeaderBuffer.Buffer,
//                                        mHeaderBuffer.WriteStartPosition, blockSize);
//                                    mSocketBuffer.FreeReadBlock(blockSize);
//                                    mHeaderBuffer.IncreaseReceivedSize(blockSize);
//                                    blockProcessed += blockSize;
//                                }

//                                if (mHeaderBuffer.IsEnough)
//                                {
//                                    Header header = mHeaderBuffer.CreateHeader();
//                                    if (header.ContentSize == 0)
//                                    {
//                                        try
//                                        {
//                                            // 沒有內容的封包
//                                            Message message = MsgFactory.CreateMessage(header, ZeroBuffer, 0);

//                                            if (message != null)
//                                            {
//                                                lock (mReceiveListLock)
//                                                {
//                                                    var messageWrap = new MessageWrap()
//                                                    {
//                                                        socketId = mSocketId,
//                                                        message = message,
//                                                        sequenceId = ++mSequenceRecvId,
//                                                    };
//                                                    mReceivedMsgList.AddLast(messageWrap);

//#if DEBUG
//                                                    Debug.LogFormat("({0})Receive [{3}] message.Id: {1}, name: {2}, size = 0",
//                                                        NameID, message.MessageId, MessageNameConverter.Convert(message.MessageId), messageWrap.sequenceId, LogTag.Network);
//#endif
//                                                }
//                                            }
//                                        }
//                                        catch (Exception e)
//                                        {
//                                            Debug.LogErrorFormat("({0})NetConnection Receive Header CreateMessage failed !!!! msgId: {1}, e: {2}",
//                                                NameID, mContentBuffer.Header.MessageId, e, LogTag.Network);
//                                        }

//                                        // 收下一個Header
//                                        mHeaderBuffer.Reset();
//                                        msgProcessed++;
//                                    }
//                                    else
//                                    {
//                                        // 開始收Content
//                                        mHeaderBuffer.Reset();

//                                        mReceiveTimer.Start();
//                                        mContentBuffer.Reset();
//                                        mContentBuffer.Header = header;
//                                        mRecvState.Transit(RecvState.Content);
//                                    }
//                                }
//                                else
//                                {
//                                    handleNextData = false;
//                                }
//                                break;
//                            }
//                            case RecvState.Content:
//                            {
//                                blockSize = Math.Min(mContentBuffer.UnreceivedContentSize, mSocketBuffer.ReceivedSize);
//                                if (blockSize > 0 && mSocketBuffer.ReceivedSize >= blockSize)
//                                {
//                                    Array.Copy(mSocketBuffer.Buffer, mSocketBuffer.ReadStartPosition, mContentBuffer.Buffer, mContentBuffer.WriteStartPosition, blockSize);
//                                    mSocketBuffer.FreeReadBlock(blockSize);
//                                    mContentBuffer.IncreaseReceivedSize(blockSize);
//                                    blockProcessed += blockSize;
//                                }

//                                if (mContentBuffer.IsEnough)
//                                {
//                                    try
//                                    {
//                                        Message message =
//                                            MsgFactory.CreateMessage(mContentBuffer.Header, mContentBuffer);

//                                        msgTmp = message;
//                                        if (message != null)
//                                        {
//                                            lock (mReceiveListLock)
//                                            {
//                                                var messageWrap = new MessageWrap()
//                                                {
//                                                    socketId = mSocketId,
//                                                    message = message,
//                                                    sequenceId = ++mSequenceRecvId,
//                                                };
//                                                mReceivedMsgList.AddLast(messageWrap);

//#if DEBUG
//                                                //Debug.LogFormat("({0})Receive [{3}] message.Id: {1}, name: {2}, size = {4}",
//                                                //    NameID, message.MessageId, MessageNameConverter.Convert(message.MessageId), messageWrap.sequenceId, message.Header.TotalSize, LogTag.Network);
//#endif
//                                            }
//                                        }

//                                        mReceiveTimer.Stop();
//                                        msgProcessed++;
//                                    }
//                                    catch (Exception e)
//                                    {
//                                        Debug.LogErrorFormat(
//                                            "({0})NetConnection Receive Content CreateMessage failed !!!! msgId: {1}, e: {2}",
//                                            NameID, mContentBuffer.Header.MessageId, e, LogTag.Network);
//                                    }

//                                    // 開始收Header
//                                    mHeaderBuffer.Reset();
//                                    mContentBuffer.Reset();
//                                    mRecvState.Transit(RecvState.Header);
//                                }
//                                else
//                                {
//                                    handleNextData = false;
//                                }
//                                break;
//                            }
//                        }
//                    }

//                    if (msgTmp != null && (blockProcessed > 0 || receivedSize > 0))
//                    {
//#if DEBUG
//                        //Debug.LogFormat("({0})NetConnection CacheBuffer {1} bytes, process {2} bytes, inner process {3} messages, last @ state = {4} ({5}), retreive {6} bytes",
//                        //    NameID, mSocketBuffer.ReceivedSize, blockProcessed, msgProcessed, mRecvState.Current, MessageNameConverter.Convert(msgTmp.MessageId), receivedSize,
//                        //     LogTag.Network);
//#endif
//                        if (mSocketBuffer.ReceivedSize == 0)
//                        {
//                            mSocketBuffer.Reset();
//                        }
//                    }
//                }
//                while (hasMoreData);

//                if (receivedSize == 0)
//                {
//                    // server close connect (assume)
//                    Debug.LogErrorFormat("({0})NetConnection Receive Header Warning!!! receivedSize is ZERO",
//                        NameID, LogTag.Network);
//                    GoToDisconnect();
//                }
//            }
//        }

//        // NetworkThread
//        private void NetworkThreadWriteProcess()
//        {
//            if (mSocket.Poll(SocketSendPollTime, SelectMode.SelectWrite))
//            {
//                switch (mSendState.Tick())
//                {
//                    case SendState.None:
//                    {
//                        break;
//                    }
//                    case SendState.Start:
//                    {
//                        mSendBuffer.Reset();
//                        lock (mSendListLock)
//                        {
//                            bool needSend = false;
//                            LinkedListNode<MessageWrap> node = mSendMessageList.First;
//                            while (node != null)
//                            {
//                                var messageWrap = node.Value;
//                                var message = messageWrap.message;

//                                if (messageWrap.socketId != mSocketId)
//                                {
//                                    Debug.LogWarningFormat(
//                                        "{0} not this socket message, currentSocketId: {1}, socket id: {2}, messageId: {3}",
//                                        NameID, mSocketId, messageWrap.socketId,
//                                        MessageNameConverter.Convert(message.MessageId), LogTag.Network);
//                                    var cur = node;
//                                    node = cur.Next;
//                                    cur.Value.message.Release();
//                                    mSendMessageList.Remove(cur);
//                                    continue;
//                                }

//                                if (!mSendBuffer.WriteToBuffer(message))
//                                {
//                                    // 裝不下了
//                                    break;
//                                }
//#if DEBUG
//                                Debug.LogFormat("({0})Send [{3}] message.Id: {1}, name: {2}, size = {4}",
//                                    NameID, message.MessageId, MessageNameConverter.Convert(message.MessageId), messageWrap.sequenceId, message.Header.TotalSize, LogTag.Network);
//#endif

//                                if (message != null)
//                                {
//                                    if (message.OnSend != null)
//                                    {
//                                        lock (mSentCallbacks)
//                                        {
//                                            mSentCallbacks.Add(message);
//                                            // retain message because callback need it
//                                            message.Retain();
//                                        }
//                                    }
//                                }

//                                LinkedListNode<MessageWrap> tmp = node;
//                                node = tmp.Next;
//                                mSendMessageList.Remove(tmp);
//                                message.Release();

//                                needSend = true;
//                            }

//                            if (needSend)
//                            {
//                                mSendState.Transit(SendState.Sending);
//                            }
//                        }
//                        break;
//                    }
//                    case SendState.Sending:
//                    {
//                        if (!mSendBuffer.IsAllSent)
//                        {
//                            try
//                            {
//                                //Debug.LogFormat(
//                                //    "({0})NetConnection Send Before, total size: {1}, offset: {2}, size: {3}",
//                                //    NameID, _sendBuffer.DataSize, _sendBuffer.SentSize,
//                                //    _sendBuffer.UnsendSize, LogTag.Network);

//                                lock (mSentCallbacks)
//                                {
//                                    if (mSentCallbacks.Count > 0)
//                                    {
//                                        var timestamp = DateTime.UtcNow;
//                                        for (int i = 0; i < mSentCallbacks.Count; ++i)
//                                        {
//                                            var message = mSentCallbacks[i];
//                                            lock (mQueueCallbacks)
//                                            {
//                                                mQueueCallbacks.Enqueue(message);
//                                            }
//                                            message.UpdateTimestamp(timestamp);
//                                        }

//                                        mSentCallbacks.Clear();
//                                    }
//                                }

//                                mSentTimer.Start();
//                                SocketError errorCode;
//                                int sentSize = mSocket.Send(
//                                    mSendBuffer.Buffer, mSendBuffer.SentSize,
//                                    mSendBuffer.UnsendSize, 0, out errorCode);

//#if DEBUG
//                                //Debug.LogFormat("({0})Send Buffer {1} bytes, buf: offset {2} / sent {3}, ret = {4}",
//                                //    NameID, sentSize, mSendBuffer.SentSize, mSendBuffer.UnsendSize, errorCode, LogTag.Network);
//#endif

//                                if (errorCode != SocketError.Success)
//                                {
//                                    Debug.LogErrorFormat(
//                                        "({0})NetConnection Send failed !!! errorCode: {1}",
//                                        NameID, errorCode, LogTag.Network);
//                                    GoToDisconnect();
//                                }
//                                else
//                                {
//                                    mSendBuffer.IncreaseSentSize(sentSize);

//                                    //Debug.LogFormat(
//                                    //    "({0})NetConnection Send After, total size: {1}, sendedSize: {2}, unsendSize: {3}",
//                                    //    NameID, _sendBuffer.DataSize, _sendBuffer.SentSize,
//                                    //    _sendBuffer.UnsendSize, LogTag.Network);

//                                    if (mSendBuffer.IsAllSent)
//                                    {
//                                        // send complete
//                                        mSentTimer.Stop();
//                                        mSendState.Transit(SendState.Start);
//#if DEBUG
//                                        //Debug.LogFormat("Sent Time: {0} msec", mSentTimer.ElapsedMilliseconds, LogTag.Network);
//#endif
//                                    }
//                                    else
//                                    {
//                                        // 沒送完, 繼續送
//                                    }
//                                }
//                            }
//                            catch (SocketException e)
//                            {
//                                Debug.LogErrorFormat(
//                                    "({0})NetConnection Send failed !!! ErrorCode: {1}, e: {2}",
//                                    NameID, e.ErrorCode, e, LogTag.Network);
//                                GoToDisconnect();
//                            }
//                            catch (ThreadInterruptedException e)
//                            {
//                                Debug.LogWarningFormat("({0})NetConnection Send ThreadInterrupted !! e: {1}",
//                                    NameID, e, LogTag.Network);
//                            }
//                            catch (Exception e)
//                            {
//                                // ignored
//                                Debug.LogWarningFormat(
//                                    "({0})NetConnection Send happened some error !!! e:{1}",
//                                    NameID, e, LogTag.Network);
//                            }
//                        }
//                        else
//                        {
//                            mSendState.Transit(SendState.Start);
//                        }
//                        break;
//                    }
//                }
//            }
//        }

//        // main thread
//        private void ExecuteOnSend()
//        {
//            // 交換完就unlock _queueCallbacks, network thread那邊才執行, 減少lock住的時間
//            lock (mQueueCallbacks)
//            {
//                if (mQueueCallbacks.Count > 0)
//                {
//                    //Debug.Log("Add CallBack " + _queueCallbacks.Count);
//                    mExecutingCalls.AddRange(mQueueCallbacks);
//                    mQueueCallbacks.Clear();
//                }
//            }

//            if (mExecutingCalls.Count > 0)
//            {
//                for (int i = 0; i < mExecutingCalls.Count; ++i)
//                {
//                    var message = mExecutingCalls[i];
//                    //Debug.LogFormat("Check CallBack {0}, {1}", message.MessageId, message.OnSend);
//                    if (message != null && message.OnSend != null)
//                    {
//                        message.OnSend(message);
//                        message.Release();
//                    }

//                    // release item
//                    mExecutingCalls[i] = null;
//                }

//                mExecutingCalls.Clear();
//            }
//        }
//    }
//}