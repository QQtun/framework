using Core.Framework.Utility;
using UnityEngine;

namespace Core.Game.Utility
{
    public static class DirectionUtil
    {
        // 16方向
        //       z
        // 
        //       0
        //　　　　｜　
        //　　　　｜　
        // 12 －－－－－4  x
        //　　　　｜
        //　　　　｜
        //　　　  8
        // 22.5度 一個方向


        // 8方向
        //       z
        // 
        //       0
        //　　　　｜　
        //　　　　｜　
        //  6 －－－－－2  x
        //　　　　｜
        //　　　　｜
        //　　　  4
        // 45度 一個方向

        public static int Dir8ToDir16(int dir)
        {
            return dir * 2;
        }

        public static int Dir16ToDir8(int dir)
        {
            return dir / 2;
        }

        public static int GetDir16FromAngle(float angle)
        {
            while (angle < 0)
                angle += 360;
            int dir = Mathf.FloorToInt(angle / 22.5f);
            return dir;
        }

        public static Quaternion GetQuaternionFromDir16(int dir)
        {
            float angle = dir * 22.5f;
            if (angle > 180)
                angle -= 360;
            Quaternion target = Quaternion.Euler(0, angle, 0);
            return target;
        }

        public static int GetDir16(Vector3 dir)
        {
            // SignedAngle 值 -180 ~ 180
            // to 在 from 的右邊 angle為正, 左邊為負
            var angle = Vector3.SignedAngle(Vector3.forward, dir, Vector3.up);
            return GetDir16FromAngle(angle);
        }

        public static Vector2 GetVector2FromDir16(int dir)
        {
            var q = GetQuaternionFromDir16(dir);
            var v3 = q * Vector3.forward;
            return v3.ToVector2IgnoreY();
        }
    }
}