using Core.Framework.Module;
using Core.Framework.Utility;
using Core.Game.Logic;
using Core.Game.Module;
using UnityEngine;

namespace Core.Game.Utility
{
    public class GameUtil
    {
        private static int? sLayerMask = null;
        public static int GroundLayerMask
        {
            get
            {
                if(!sLayerMask.HasValue)
                {
                    sLayerMask = (1 << LayerMask.NameToLayer("Terrain"));
                    sLayerMask |= (1 << LayerMask.NameToLayer("Non-Barrier"));
                }
                return sLayerMask.Value;
            }
        }

        public static Vector3 GetGroundPos(Vector3 pos)
        {
            Vector3 startPos = new Vector3(pos.x, 150, pos.z);
            Vector3 end = new Vector3(pos.x, -50, pos.z);
            if(Physics.Linecast(startPos, end, out var hit, GroundLayerMask))
            {
                return hit.point;
            }
            return new Vector3(pos.x, 0, pos.z);
        }

        // 
        /// <summary>
        ///     檢查cur 到 target 是否有高度無法通過的點
        /// </summary>
        /// <param name="curY">Unity單位</param>
        /// <param name="curPos">伺服器單位</param>
        /// <param name="targetPos">伺服器單位</param>
        /// <param name="stopPos">伺服器單位</param>
        /// <returns>true 無法通過</returns>
        public static bool CheckHeightDiff(float curY, Vector2 curPos, Vector2 targetPos, out Vector2 stopPos)
        {
            var dir = targetPos - curPos;
            var len = Mathf.FloorToInt(dir.magnitude);
            float tX = (targetPos.x - curPos.x) / len;
            float tY = (targetPos.y - curPos.y) / len;
            for (int i = 1; i <= len; i++)
            {
                int posX = (int)(curPos.x + tX * i);
                int posY = (int)(curPos.y + tY * i);
                var checkPos = new Vector2(posX, posY);
                //var checkPos = Vector2.Lerp(curPos, checkLastPos, i / (float)len);
                var groundPos = GetGroundPos(checkPos.ToVector3() / Define.PositionScale);
                var diff = groundPos.y - curY;
                if (diff > Define.CantPassHeightDiff)
                {
                    // 高度往上超過0.3f 不能走
                    //stopPos = Vector2.Lerp(curPos, checkLastPos, (i - 1) / (float)len);
                    stopPos = new Vector2((int)(curPos.x + tX * (i - 1)), (int)(curPos.y + tY * (i - 1)));
                    return true;
                }
                curY = groundPos.y;
            }
            stopPos = Vector2.zero;
            return false;
        }

        public static bool IsHeader(int roleId)
        {
            var roleModule = ModuleManager.Instance.GetModule<RoleDataModule>();
            if (roleModule != null && roleModule.RoleDetail != null)
                return roleModule.RoleDetail.RoleId == roleId;
            return false;
        }
    }
}