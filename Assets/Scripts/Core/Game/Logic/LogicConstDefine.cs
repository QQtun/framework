namespace Core.Game.Logic
{
    public enum SceneObjectType
    {
        None = 0,
        Sprite = 1,
    }

    public enum SpriteType
    {
        None = -1,
        Leader = 0, // 主玩家
        Other,      // 其他玩家
        Monster,
        NPC,
        Pet,
        BiaoChe,
        JunQi,
        FakeRole,
        Platform,
        VirtualObj,
        GarbageBag,
        RoleGhost,
        Plant,
    }

    public enum ActionType
    {
        None = -1,
        Stand = 0,
        Walk = 1,
        Run = 2,
        Attack = 3,
        Injured = 4,
        Magic = 6,
        Rush = 10,
        RushEnd = 11,
    }

    public enum SpecialMagicIndex
    {
        Lash1 = 0,
        Lash2,
        Dodge1,
        Dodge2,
        Dodge3,
        Dodge4,
        Dodge0,
        Buff,
        Attack1,
        Attack2,
        Attack3,
        Attack4,
        Walk, // 這個是移動距離比較大的第一下普攻
        WitchTime,
        Awake1,
        Awake2,
        MagicCombo1,
        MagicCombo2,
        MagicCombo3,
        MagicCombo4,
        NewDodge,
        NewWitchTime,
        WitchTimeBack,
        NewWitchTimeBack,
    }

    public static class Define
    {
        public const float HandleUserInputInterval = 0.1f;
        public const float PathArrivedDistance = 50;
        public const float PositionScale = 100;
        public const int RunSpeed = 100;
        public const int WalkSpeed = 25;
        public const int RushEndSpeed = 30;
        public const float BasicModeSpeed = 6;
        public const float CantPassHeightDiff = 0.3f;
        public const float RushThreshold = 0.8f;
        public const int LoadMonsterPerFrame = 1;
        public const int AddSpriteFrameInterval = 5;
        public const float DeadDelay = 1;
        public const int OutOfAttackRange = 2000;
        public const float ContinuouslyAttackInterval = 1;
    }
}