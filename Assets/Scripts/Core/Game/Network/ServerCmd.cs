using P5.Protobuf;
using System;
using System.Collections.Generic;

namespace Core.Game.Network
{
    // 必須與 LuaScripts\Game\Network\ServerCmd.lua.txt 保持一致
    public enum ServerCmd
    {
        UserLogin = 10,
        GameLogin = 12,
        RoleList = 20,
        CreateRole = 21,
        InitGame = 80,
        PlayGame = 81,
        SyncTime = 100,
        ClientHeart = 101,
        SpriteMove = 102,
        MoveEnd = 103,
        RolePosition = 105,
        SpriteLeave = 107,
        MonsterDataNotify = 108,
        SpriteAction = 121,
        SpriteMagicCode = 123,
        SpriteAttack = 124,
        SpriteInjure = 125,
        SpriteHited = 128,
        LoadSpriteData = 165,
        MoveStart = 2661,
    }

    public class CmdRecvClassMapping
    {
        public static readonly Dictionary<int, Type> Mapping = new Dictionary<int, Type>()
        {
            {(int)ServerCmd.UserLogin, typeof(UserLoginOnResponse) },
            {(int)ServerCmd.GameLogin, typeof(GameLoginOnResponse) },
            {(int)ServerCmd.RoleList, typeof(RoleListResponse) },
            {(int)ServerCmd.CreateRole, typeof(CreateRoleResponse) },
            {(int)ServerCmd.InitGame, typeof(InitGameResponse) },
            {(int)ServerCmd.PlayGame, typeof(PlayGameResponse) },
            {(int)ServerCmd.SyncTime, typeof(SyncTimeResponse) },
            {(int)ServerCmd.ClientHeart, typeof(ClientHeartResponse) },
            {(int)ServerCmd.MonsterDataNotify, typeof(MonsterData) },
            {(int)ServerCmd.LoadSpriteData, typeof(LoadSpriteDataResponse) },
            {(int)ServerCmd.SpriteLeave, typeof(SpriteLeave) },
            {(int)ServerCmd.SpriteMove, typeof(SpriteNotifyOtherMoveData) },
            {(int)ServerCmd.MoveStart, typeof(MoveStart) },
            {(int)ServerCmd.MoveEnd, typeof(MoveEnd) },
            {(int)ServerCmd.SpriteAction, typeof(RoleAction) },
            {(int)ServerCmd.SpriteMagicCode, typeof(MagicCodeData) },
            {(int)ServerCmd.SpriteInjure, typeof(InjuredData) },
            {(int)ServerCmd.SpriteHited, typeof(RoleHited) },
        };
    }
}