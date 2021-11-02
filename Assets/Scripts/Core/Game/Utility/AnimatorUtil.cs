using UnityEditor;
using UnityEngine;

namespace Core.Game.Utility
{
    public static class AnimatorUtil
    {
        public static class State
        {
            public const string Idle = "Idle";
            public const string BattleIdle = "BattleIdle";
            public const string Run = "Run";
            public const string Walk = "Walk";
            public const string RushStart = "RushStart";
            public const string RushLoop = "RushLoop";
            public const string RushEnd = "RushEnd";
            public const string JumpStart = "JumpStart";
            public const string JumpLoop = "JumpLoop";
            public const string JumpEnd = "JumpEnd";
            public const string Attack1 = "Attack1";
            public const string Attack2 = "Attack2";
            public const string Attack3 = "Attack3";
            public const string Attack4 = "Attack4";
            //public const string Attack5 = "Attack5";
            public const string Dodge = "Dodge";
        }

        public static class Param
        {
            public const string SpeedInt = "speed";
            public const string IsBattling = "isBattling";
            public const string IsJumping = "isJumping";
            public const string IsRush = "isRush";
            public const string IsWalk = "isWalk";
            public const string Attack1Trigger = "attack1";
            public const string Attack2Trigger = "attack2";
            public const string Attack3Trigger = "attack3";
            public const string Attack4Trigger = "attack4";
            public const string JumpTrigger = "jump";
            public const string FallenTrigger = "fallen";
            public const string RunTrigger = "run";
            public const string IdleTrigger = "idle";
            public const string DodgeTrigger = "dodge";
            public const string EvadeTrigger = "evade";
        }

        public static string GetAttackTrigger(int attackNum)
        {
            if (attackNum == 1)
                return Param.Attack1Trigger;
            if (attackNum == 2)
                return Param.Attack2Trigger;
            if (attackNum == 3)
                return Param.Attack3Trigger;
            if (attackNum == 4)
                return Param.Attack4Trigger;

            LogUtil.Debug.LogError($"error attack num={attackNum} !!");
            return Param.Attack1Trigger;
        }

        public static string GetDodgeTrigger(uint magicId)
        {
            if (magicId == 114)
                return Param.EvadeTrigger;
            return Param.DodgeTrigger;
        }
    }
}