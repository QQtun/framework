//using Core.Framework.Network;
//using Core.Framework.Network.Data;
//using Core.Game.Utility;
//using P5.Protobuf;
//using System;
//using System.Diagnostics;

//namespace Core.Game.Network
//{
//    public class SynchTimeProcessor : IMessagePreprocessor
//    {
//        private const int SendInterval = 2; //s
//        private bool mStartSyncTime = false;
//        private Stopwatch mTimer = new Stopwatch();

//        void IMessagePreprocessor.OnConnected(INetConnection connection)
//        {
//        }

//        void IMessagePreprocessor.OnDisconnected(INetConnection connection, DisconnectReason reason)
//        {
//            mStartSyncTime = false;
//        }

//        bool IMessagePreprocessor.PreprocessReceivedMessage(INetConnection connection, Message message, IConnectionListener listener)
//        {
//            if (message.MessageId == (int)ServerCmd.SyncTime)
//            {
//                var rsp = message.GetData<SyncTimeResponse>();
//                var nowTicks = DateTime.Now.Ticks;
//                var latency = (nowTicks - rsp.ClientTicks) / TimeSpan.TicksPerMillisecond;
//                ServerTimeUtil.AddLatency(latency);
//                var halfLatency = latency / 2;
//                ServerTimeUtil.ClientSubServerNowTick = nowTicks - (rsp.ServerTicks + halfLatency);
//            }
//            return false;
//        }

//        bool IMessagePreprocessor.PreprocessSendMessage(INetConnection connection, Message message)
//        {
//            if (message.MessageId == (int)ServerCmd.InitGame)
//            {
//                mStartSyncTime = true;
//                mTimer.Reset();
//                mTimer.Start();

//                SendTimeSync(connection);
//            }
//            return false;
//        }

//        void IMessagePreprocessor.Update(INetConnection connection, IConnectionListener listener, float dt)
//        {
//            if (mStartSyncTime && mTimer.IsRunning && mTimer.Elapsed.TotalSeconds >= SendInterval)
//            {
//                mTimer.Reset();
//                mTimer.Start();

//                SendTimeSync(connection);
//            }
//        }

//        private void SendTimeSync(INetConnection connection)
//        {
//            var req = new SyncTime();
//            req.ClientTicks = DateTime.Now.Ticks;
//            connection.Send(GoogleProtocolBufMessage.Allocate((int)ServerCmd.SyncTime, req));
//        }
//    }
//}