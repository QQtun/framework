//using Core.Framework.Network;
//using Core.Framework.Network.Data;
//using Core.Framework.Network.Serializer;
//using Core.Framework.Res;
//using Core.Framework.Utility;
//using P5.Protobuf;
//using System;
//using System.Collections.Generic;
//using UnityEngine;
//using Google.Protobuf;

//namespace Core.Game.Network
//{
//    public class LoginParam
//    {
//        public string protoVer; // ?
//        public string userName;
//        public string password;
//    }

//    public class GameConnection : Singleton<GameConnection> , IConnectionListener
//    {
//        public delegate void OnConnect();
//        public delegate void OnReceiveMessage(Message msg);
//        public delegate void OnDisconnect(DisconnectReason reason);

//        public const string ConfigPath = "Assets/PublicAssets/Configs/ServerConfigs.asset";

//        private ServerConfigs mConfig;
//        private MessageFactory mFactory;
//        private ServerConnection mLoginConnection;
//        private ServerConnection mGameConnection;
//        private LoginConnectionListener mLoginListener;

//        private bool mDoLogin;
//        private ServerSet mLoginServerSet;
//        private ServerSet mGameServerSet;
//        private LoginParam mLoginParam;

//        private Session mSession;

//        private bool mLoginSuccessed;

//        public Session CurrentSession => mSession;
//        public MessageFactory MessageFactory => mFactory;

//        public bool CanSendToGame => mGameConnection != null && mGameConnection.IsConnected && mLoginSuccessed;

//        public event OnConnect OnConnected;
//        public event OnReceiveMessage OnReceivedMessage;
//        public event OnDisconnect OnDisconnected;

//        protected override void Init()
//        {
//            base.Init();
//            mLoginSuccessed = false;

//            MessageNameConverter.Delegate = new MessageID2String();
//            mLoginListener = new LoginConnectionListener();
//            mFactory = new MessageFactory();

//            var classList = new List<Type>();
//            foreach(var kv in CmdRecvClassMapping.Mapping)
//            {
//                mFactory.RegisterProtoBufMessage(kv.Key, kv.Value);
//                classList.Add(kv.Value);
//            }
//            // 初始化會用到的Protobuf結構
//            GoogleProtocolSerializer.Initialize(classList);

//            ResourceManager.Instance.LoadAssetAsync<ServerConfigs>(ConfigPath, (config) =>
//            {
//                mConfig = config;
//                mLoginConnection = new ServerConnection(mConfig.GetConfig("Login"), mFactory);
//                mLoginConnection.SetConnectionListener(mLoginListener);
//                mGameConnection = new ServerConnection(mConfig.GetConfig("Game"), mFactory);
//                mGameConnection.SetConnectionListener(this);
//                mGameConnection.AddMessagePreprocessor(new ClientHeartProcessor());
//                mGameConnection.AddMessagePreprocessor(new SynchTimeProcessor());
//            });
//        }

//        private void OnDestroy()
//        {
//            if (mLoginConnection.IsConnected)
//                mLoginConnection.Disconnect();
//            if (mGameConnection.IsConnected)
//                mGameConnection.Disconnect();
//        }

//        public void Login(ServerSet loginServerSet, ServerSet gameServerSet, LoginParam loginParam)
//        {
//            mLoginServerSet = loginServerSet;
//            mGameServerSet = gameServerSet;
//            mLoginParam = loginParam;
//            mDoLogin = true;
//        }

//        public void Send(Message msg)
//        {
//            if(mGameConnection.IsConnected && mLoginSuccessed)
//            {
//                mGameConnection.Send(msg);
//            }
//            else
//            {
//                LogUtil.Debug.LogError("GameServer Not Ready", LogUtil.LogTag.Network);
//            }
//        }

//        public void Send(ServerCmd msgId, Google.Protobuf.IMessage msg)
//        {
//            Send((int)msgId, msg);
//        }

//        public void Send(int msgId, Google.Protobuf.IMessage msg)
//        {
//            var protoMsg = GoogleProtocolBufMessage.Allocate(msgId, msg);
//            Send(protoMsg);
//        }

//        public void Send(int msgId, byte[] data)
//        {
//            var rawDataMsg = RawDataMessage.Allocate(msgId, data);
//            Send(rawDataMsg);
//        }

//        //public void Send(int msgId, string msg, bool encrypt = true)
//        //{
//        //    var strMsg = XMLStringMessage.Allocate(msgId, encrypt);
//        //    strMsg.Append(msg);
//        //    Send(strMsg);
//        //}

//        private void Update()
//        {
//            if(mDoLogin && mLoginConnection != null)
//            {
//                // 這邊挺醜的 看要不要讓mConfig改成外部塞入 或是同步讀取 
//                mDoLogin = false;
//                mLoginConnection.Connect(mLoginServerSet);
//            }

//            mLoginConnection?.Update();
//            mGameConnection?.Update();
//        }

//        void IConnectionListener.OnConnected()
//        {
//            LogUtil.Debug.Log("Game OnConnected");

//            var msg = GoogleProtocolBufMessage.Allocate((int)ServerCmd.GameLogin, new P5.Protobuf.GameLoginOn()
//            {
//                UserId = CurrentSession.userID,
//                UserName = CurrentSession.userName,
//                UserToken = CurrentSession.userToken,
//                RoleRandToken = -1,
//                VerSign = int.Parse(mLoginParam.protoVer),
//                UserIsAdult = CurrentSession.userIsAdult ? 1 : 0,
//                DeviceId = SystemInfo.deviceUniqueIdentifier,
//                InputNumber = 13,
//            });

//            mGameConnection.Send(msg);
//        }

//        void IConnectionListener.OnReceiveMessage(Message msg)
//        {
//            LogUtil.Debug.Log("Game OnReceiveMessage msgid=" + MessageNameConverter.Convert(msg.MessageId));

//            switch ((ServerCmd)msg.MessageId)
//            {
//                case ServerCmd.GameLogin:
//                {
//                    var rsp = msg.GetData<GameLoginOnResponse>();
//                    var randToken = rsp.TcpRandKey;

//                    if (randToken >= 0)
//                    {
//                        mLoginSuccessed = true;
//                        CurrentSession.roleRandToken = randToken;
//                        OnConnected?.Invoke();
//                        return;
//                    }
//                    else if (randToken == ErrorCode.Error_Token_Expired2)
//                    {
//                        // TODO 登陆游戏服务器时失败, 已经超过了口令最长有效时间, 请退出游戏重新进入...
//                    }
//                    else if (randToken == ErrorCode.Error_Connection_Closing2
//                        || randToken == ErrorCode.Error_Connection_Closing)
//                    {
//                        // TODO 登陆的用户名已经在线，请稍后重新刷新登陆
//                    }
//                    else if (randToken == ErrorCode.Error_Version_Not_Match2)
//                    {
//                        // TODO 登陆游戏服务器时失败, 客户端的版本太旧，请更新客户端后再重新登陆
//                    }
//                    else if (randToken == -10)
//                    {
//                        // TODO 登陆游戏服务器时失败, 你已经被游戏管理员禁止登陆
//                    }
//                    else if (randToken == ErrorCode.Error_Server_Connections_Limit)
//                    {
//                        // TODO 当前服务器在线爆满，您可登录其他服务器进行游戏！
//                    }
//                    else if (randToken == ErrorCode.Error_Connection_Disabled)
//                    {
//                        // TODO 游戏服务器正在维护中
//                    }
//                    mGameConnection.Disconnect(DisconnectReason.Network);
//                    break;
//                }
//                default:
//                {
//                    OnReceivedMessage?.Invoke(msg);
//                    break;
//                }
//            }
//        }

//        void IConnectionListener.OnDisconnected(DisconnectReason reason)
//        {
//            LogUtil.Debug.Log("Game OnDisconnected reason=" + reason);

//            mLoginSuccessed = false;
//            OnDisconnected?.Invoke(reason);
//        }

//        private class LoginConnectionListener : IConnectionListener
//        {
//            public void OnConnected()
//            {
//                LogUtil.Debug.Log("Login OnConnected");
//                var msg = GoogleProtocolBufMessage.Allocate((int)ServerCmd.UserLogin, new P5.Protobuf.UserLoginOn()
//                {
//                    UserName = Instance.mLoginParam.userName,
//                    UserPwd = Instance.mLoginParam.password,
//                    VerSign = int.Parse(Instance.mLoginParam.protoVer)
//                });

//                Instance.mLoginConnection.Send(msg);
//            }

//            public void OnDisconnected(DisconnectReason reason)
//            {
//                LogUtil.Debug.Log("Login OnDisconnected reason=" + reason);
//                if(reason != DisconnectReason.ForGameServer && reason != DisconnectReason.User)
//                    ((IConnectionListener)Instance).OnDisconnected(reason);
//            }

//            public void OnReceiveMessage(Message msg)
//            {
//                LogUtil.Debug.Log("Login OnReceiveMessage msgid=" + MessageNameConverter.Convert(msg.MessageId));
//                var rsp = msg.GetData<P5.Protobuf.UserLoginOnResponse>();
//                Instance.mSession = Instance.mSession ?? new Session();
//                Instance.mSession.userID = rsp.UserId;
//                Instance.mSession.userName = rsp.UserName;
//                Instance.mSession.userToken = rsp.UserToken;
//                Instance.mSession.userIsAdult = Convert.ToInt32(rsp.IsAdult) == 1;
//                Instance.mLoginConnection.Disconnect(DisconnectReason.ForGameServer);
//                Instance.mGameConnection.Connect(Instance.mGameServerSet);
//            }
//        }
//    }
//}