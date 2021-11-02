namespace Core.Framework.Event.Property.PropertyObserve
{
    // 填完名稱記得在PropertyNameInfo下面填入對應的類別

    public enum PropertyKey
    {
        None,

        // Test
        //IntProperty,

        //IntProperty2,
        //FloatProperty,
        //HpPercentage,
        //StringProperty,
        //BoolProperty,
        //TimeSpanProperty,
        //DateTimeProperty,
        //IntProperty3,
        //IntProperty4,
        //IntProperty5,
        //IntProperty6,

        ////Time
        //LocalDateTime,

        //ServerBuildTime,    // server建置時間

        //// PlayerInfo
        //PlayerId,               // 玩家ID

        //PlayerName,             // 玩家名稱
        //PlayerLevel,            // 玩家等級
        //PlayerExp,              // 玩家經驗
        //PlayerVip,              // 玩家VIP等級
        //PlayerVipExp,           // 玩家VIP經驗
        //PlayerVipContinuousDays,// 玩家VIP連續登入天數
        //PlayerTotalPower,       // 玩家總實力
        //PlayerHasEnergy,        // 玩家當前能量
        //PlayerHasStamina,       // 玩家當前冒險補給
        //PlayerHasFood,          // 玩家當前食物量
        //PlayerHasWood,          // 玩家當前木頭量
        //PlayerHasStone,         // 玩家當前石頭量
        //PlayerHasGold,          // 玩家當前黃金量
        //PlayerCapitalPos,       // 玩家主堡座標(字串)
        //PlayerActionPoint,      // 玩家当前行动力
        //CurrentPos,             // 當前座標(字串)


        //// Population
        //VillageStillCanConcludeAmount,  //村莊還能締結數量
        //VillageQuestRewardAmount,       //村莊任務獎勵數量

        //// Mail
        //MailTotalUnreadAmount,          //郵件總未讀封數

        //// Hero
        //HeroRecruitCount,
        //HeroPracticeCount,
        //HeroTalentCount,
        //HeroSkillCount,

        //// Clan
        //ClanVerifyReqJoinAmount,      // 联盟请求加入验证

        //CircadianInDay, //日或夜
        //CircadianRhythmRate,    //日夜變化的轉換比例
        //CircadianRhythmTimeInSecond,    //日夜變化的時間 總時間 ParameterSetting.SunshineTime + ParameterSetting.NightTime
    }

    //public static partial class EnumConv
    //{
    //    /// <summary>
    //    /// Gets the name of the enum.
    //    /// </summary>
    //    /// <param name="value">The value.</param>
    //    /// <returns></returns>
    //    public static string GetEnumName(this PropertyKey value)
    //    {
    //        return Foundation.Utility.EnumCache<PropertyKey>.Instance.GetEnumName((int)value);
    //    }
    //}
}