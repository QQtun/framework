using Core.Framework.Module;
using Core.Game.Logic;
using Core.Game.Module;
using Core.Game.Utility;
using Google.Protobuf;
using P5.Protobuf;
using System;
using UnityEngine;

namespace Core.Game.Network
{
    public static class SendMsgHelper
    {
        private static int RoleId
        {
            get
            {
                var roleModule = ModuleManager.Instance.GetModule<RoleDataModule>();
                return roleModule.RoleDetail.RoleId;
            }
        }

        private static void Send(ServerCmd msgId, IMessage req)
        {
            //GameConnection.Instance.Send(msgId, req);
        }

        public static void SendRolePosition(int map, Vector2Int pos)
        {
            var req = new RolePosition();
            req.MapCode = map;
            req.PosX = pos.x;
            req.PosY = pos.y;
            req.CurrentPosTicks = ServerTimeUtil.MillisecondNow;
            Send(ServerCmd.RolePosition, req);
        }

        public static void SendRunStatus(int status)
        {
            var req = new RoleRunStatus();
            req.RunStatus = status;
            Send(ServerCmd.RolePosition, req);
        }

        public static void SendMoveStart(int map, Vector2Int from, Vector2Int to)
        {
            var req = new MoveStart();
            req.MapCode = map;
            req.FromX = from.x;
            req.FromY = from.y;
            req.ToX = to.x;
            req.ToY = to.y;
            req.StartMoveTicks = ServerTimeUtil.MillisecondNow;
            Send(ServerCmd.MoveStart, req);
        }

        public static void SendMoveEnd(int map, ISceneObject obj)
        {
            var req = new MoveEnd();
            req.MapCode = map;
            req.RoleId = obj.Id;
            req.ToDirection = DirectionUtil.Dir16ToDir8(obj.Direction);
            req.ToX = obj.Coordinate.x;
            req.ToY = obj.Coordinate.y;
            req.ClientTicks = ServerTimeUtil.MillisecondNow;
            Send(ServerCmd.MoveEnd, req);
        }

        public static void SendLoadSpriteData(int id)
        {
            var req = new LoadSpriteData();
            req.OtherRoleId = id;
            Send(ServerCmd.LoadSpriteData, req);
        }

        public static void SendSpriteMagicCode(int magicCode, int targetId, Vector2Int targetPos)
        {
            var roleModule = ModuleManager.Instance.GetModule<RoleDataModule>();

            var req = new MagicCodeData();
            req.RoleId = roleModule.RoleDetail.RoleId;
            req.MapCode = roleModule.RoleDetail.MapCode;
            req.MagicCode = magicCode;
            req.TargetId = targetId;
            req.PosX = targetPos.x;
            req.PosY = targetPos.y;
            Send(ServerCmd.SpriteMagicCode, req);
        }

        public static void SendSpriteAction(float angleY, ActionType action, Vector2Int toPos, Vector2Int targetPos, int actionData)
        {
            var roleModule = ModuleManager.Instance.GetModule<RoleDataModule>();

            var req = new RoleAction();
            req.RoleId = roleModule.RoleDetail.RoleId;
            req.MapCode = roleModule.RoleDetail.MapCode;
            req.Direction = DirectionUtil.Dir16ToDir8(DirectionUtil.GetDir16FromAngle(angleY));
            req.YAngle = Mathf.RoundToInt(angleY);
            req.Action = (int)action;
            //req.MoveToX = moveTo.x;
            //req.MoveToY = moveTo.y;
            req.TargetX = targetPos.x;
            req.TargetY = targetPos.y;
            req.ToX = toPos.x;
            req.ToY = toPos.y;
            req.ActionData = actionData;
            Send(ServerCmd.SpriteAction, req);
        }

        public static void SendSpriteAttack(Vector2Int curPos, int magicCode, Vector2Int moveTo, int enemyId, Vector2Int enemyPos)
        {
            var roleModule = ModuleManager.Instance.GetModule<RoleDataModule>();

            var req = new RoleAttack();
            req.RoleId = roleModule.RoleDetail.RoleId;
            req.RolePosX = curPos.x;
            req.RolePosY = curPos.y;
            req.MagicCode = magicCode;
            req.MoveX = moveTo.x;
            req.MoveY = moveTo.y;
            req.EnemyId = enemyId;
            req.EnemyPosX = enemyPos.x;
            req.EnemyPosY = enemyPos.y;
            req.RealEnemyPosX = enemyPos.x;
            req.RealEnemyPosY = enemyPos.y;
            req.ClientTicks = ServerTimeUtil.MillisecondNow;
            req.DamagePower = 100;
            Send(ServerCmd.SpriteAttack, req);
        }
    }
}