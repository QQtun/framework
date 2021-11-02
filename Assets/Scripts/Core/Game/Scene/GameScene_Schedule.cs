using Core.Framework.Module;
using Core.Game.Module;
using Core.Game.Network;
using Core.Game.Utility;
using P5.Protobuf;
using UnityEngine;

namespace Core.Game.Scene
{
    public partial class GameScene
    {
        private const int UpdatePositionInterval = 2;
        private Vector2Int? mLastSendLeaderPos;
        private float mLastSendTime;

        private bool SendLeaderMoving(int key, float dt, object obj)
        {
            if (!mPlayGame)
                return true;

            if (Leader == null)
                return true;

            var curPos = Leader.Coordinate;
            bool isTimeUp = (Time.time - mLastSendTime) > UpdatePositionInterval;
            bool isMoving = false;
            if(mLastSendLeaderPos.HasValue)
            {
                var diff = curPos - mLastSendLeaderPos.Value;
                isMoving = Mathf.Abs(diff.x) > 200 || Mathf.Abs(diff.y) > 200;
            }
            if(!mLastSendLeaderPos.HasValue || isMoving || isTimeUp)
            {
                var roleModule = ModuleManager.Instance.GetModule<RoleDataModule>();

                mLastSendTime = Time.time;
                mLastSendLeaderPos = curPos;

                SendMsgHelper.SendRolePosition(roleModule.RoleDetail.MapCode, Leader.Coordinate);
            }

            return true;
        }
    }
}