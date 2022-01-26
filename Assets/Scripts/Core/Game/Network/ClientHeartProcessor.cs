//using Core.Framework.Network;
//using Core.Framework.Network.Data;
//using P5.Protobuf;
//using System;
//using System.Diagnostics;

//namespace Core.Game.Network
//{
//    public class ClientHeartProcessor : IMessagePreprocessor
//    {
//        private const int SendInterval = 1; //s
//        private bool mStartHeart;
//        private Stopwatch mTimer = new Stopwatch();

//        void IMessagePreprocessor.OnConnected(INetConnection connection)
//        {
//        }

//        void IMessagePreprocessor.OnDisconnected(INetConnection connection, DisconnectReason reason)
//        {
//            mStartHeart = false;
//        }

//        bool IMessagePreprocessor.PreprocessReceivedMessage(INetConnection connection, Message message, IConnectionListener listener)
//        {
//            if (message.MessageId == (int)ServerCmd.ClientHeart)
//            {
//                // TODO 
//            }
//            return false;
//        }

//        bool IMessagePreprocessor.PreprocessSendMessage(INetConnection connection, Message message)
//        {
//            if(message.MessageId == (int) ServerCmd.PlayGame)
//            {
//                mStartHeart = true;
//                mTimer.Reset();
//                mTimer.Start();
//            }
//            return false;
//        }

//        void IMessagePreprocessor.Update(INetConnection connection, IConnectionListener listener, float dt)
//        {
//            if(mStartHeart && mTimer.IsRunning && mTimer.Elapsed.TotalSeconds >= SendInterval)
//            {
//                mTimer.Reset();
//                mTimer.Start();

//                var req = new ClientHeart();
//                req.RandToken = GameConnection.Instance.CurrentSession.roleRandToken;
//                req.ReportClientRealTicks = DateTime.Now.Ticks;
//                var msg = GoogleProtocolBufMessage.Allocate((int)ServerCmd.ClientHeart, req);
//                connection.Send(msg);
//            }
//        }
//    }
//}