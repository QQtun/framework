using Core.Framework.Event;
using Core.Framework.Module;
using Core.Framework.Network;
using Core.Framework.Network.Data;
using Core.Game.Event;
using Core.Game.Network;
using P5.Protobuf;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Game.Module
{
    [Module]
    public class RoleDataModule : NetworkModule
    {
        private List<int> mDependencies = new List<int>()
        {
        };

        private List<int> mMsgIds = new List<int>()
        {
            (int) ServerCmd.RoleList,
            (int) ServerCmd.CreateRole,
            (int) ServerCmd.InitGame,
        };

        public List<RoleSimple> RoleList { get; private set; } = new List<RoleSimple>();

        public RoleDetail RoleDetail { get; private set; }


        public override int Id => (int)ModuleId.RoleDataModule;

        public override List<int> DependencyModules()
        {
            return mDependencies;
        }

        public override List<int> GetMessageIds()
        {
            return mMsgIds;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pauseStatus"></param>
        public override void OnApplicationPause(bool pauseStatus)
        {
        }

        public override void OnConnected()
        {
            //var req = new RoleList();
            //req.Userid = GameConnection.Instance.CurrentSession.userID;
            //req.ZoneId = GameConnection.Instance.CurrentSession.gameZoneID;
            //Send(ServerCmd.RoleList, req);
        }

        public override void OnDestroy()
        {
        }

        public override void OnDisconnected(DisconnectReason reason)
        {
        }

        public override void OnReceivedMessage(Message msg)
        {
            switch((ServerCmd)msg.MessageId)
            {
                case ServerCmd.RoleList:
                {
                    var rsp = msg.GetData<RoleListResponse>();
                    RoleList.Clear();
                    RoleList.AddRange(rsp.Roles);

                    EventSystem.Instance.SendStringKeyEvent(EventKey.OnRoleList);
                    break;
                }
                case ServerCmd.CreateRole:
                {
                    var rsp = msg.GetData<CreateRoleResponse>();
                    RoleList.Add(rsp.Role);
                    EventSystem.Instance.SendStringKeyEvent(EventKey.OnCreateRole);
                    break;
                }
                case ServerCmd.InitGame:
                {
                    var rsp = msg.GetData<InitGameResponse>();
                    RoleDetail = rsp.RoleDetail;
                    ModuleManager.Instance.StartGameInit();
                    EventSystem.Instance.SendStringKeyEvent(EventKey.OnInitGame);
                    break;
                }
            }
        }

        public override void OnStart()
        {
        }

        public override void OnGameInit()
        {
        }

        public override void OnUpdate()
        {
        }

        public void CreateRole()
        {
            //var req = new CreateRole();
            //req.UserId = GameConnection.Instance.CurrentSession.userID;
            //req.UserName = GameConnection.Instance.CurrentSession.userName;
            //req.ZoneId = GameConnection.Instance.CurrentSession.gameZoneID;
            //// TODO
            //req.Occupation = 1;
            //req.Sex = 1;
            //req.Name = "測試名稱";
            //req.PlatformId = SystemInfo.deviceModel;
            //req.ModleType.Add(1);
            //req.ModleType.Add(1);
            //req.ModleType.Add(1);
            //req.ModleType.Add(1);
            //Send(ServerCmd.CreateRole, req);
        }

        public void InitGame(int roleId)
        {
            //var req = new InitGame();
            //req.UserId = GameConnection.Instance.CurrentSession.userID;
            //req.RoleId = roleId;
            //Send(ServerCmd.InitGame, req);
        }
    }
}