using System.Collections.Generic;

namespace Core.Framework.Event.Property.PropertyObserve
{
    public class PropertyNameInfo
    {
        internal static readonly PropertyKeyComparer s_propertyKeyComparer = new PropertyKeyComparer();
        private static PropertyNameInfo s_instance;
        protected PropertyNameInfo()
        {
            NameToTypeDic = new Dictionary<PropertyKey, PropertyType>(s_propertyKeyComparer);

            //// Test
            //AddNameToTypeMapping(PropertyKey.IntProperty, PropertyType.Int);
            //AddNameToTypeMapping(PropertyKey.IntProperty2, PropertyType.Int);
            //AddNameToTypeMapping(PropertyKey.FloatProperty, PropertyType.Float);
            //AddNameToTypeMapping(PropertyKey.HpPercentage, PropertyType.Float);
            //AddNameToTypeMapping(PropertyKey.StringProperty, PropertyType.String);
            //AddNameToTypeMapping(PropertyKey.BoolProperty, PropertyType.Bool);
            //AddNameToTypeMapping(PropertyKey.TimeSpanProperty, PropertyType.TimeSpan);
            //AddNameToTypeMapping(PropertyKey.DateTimeProperty, PropertyType.String);
            //AddNameToTypeMapping(PropertyKey.IntProperty3, PropertyType.Quaternion);
            //AddNameToTypeMapping(PropertyKey.IntProperty4, PropertyType.Vector2);
            //AddNameToTypeMapping(PropertyKey.IntProperty5, PropertyType.Vector3);
            //AddNameToTypeMapping(PropertyKey.IntProperty6, PropertyType.Color);

            ////Time
            //AddNameToTypeMapping(PropertyKey.LocalDateTime, PropertyType.DateTime);
            //AddNameToTypeMapping(PropertyKey.ServerBuildTime, PropertyType.DateTime);

            //// PlayerInfo
            //AddNameToTypeMapping(PropertyKey.PlayerId, PropertyType.ULong);
            //AddNameToTypeMapping(PropertyKey.PlayerName, PropertyType.String);
            //AddNameToTypeMapping(PropertyKey.PlayerLevel, PropertyType.Int);
            //AddNameToTypeMapping(PropertyKey.PlayerExp, PropertyType.Int);
            //AddNameToTypeMapping(PropertyKey.PlayerVip, PropertyType.Int);
            //AddNameToTypeMapping(PropertyKey.PlayerVipExp, PropertyType.Int);
            //AddNameToTypeMapping(PropertyKey.PlayerVipContinuousDays, PropertyType.Int);
            //AddNameToTypeMapping(PropertyKey.PlayerTotalPower, PropertyType.Long);
            //AddNameToTypeMapping(PropertyKey.PlayerHasEnergy, PropertyType.Int);
            //AddNameToTypeMapping(PropertyKey.PlayerHasStamina, PropertyType.Int);
            //AddNameToTypeMapping(PropertyKey.PlayerHasFood, PropertyType.Long);
            //AddNameToTypeMapping(PropertyKey.PlayerHasWood, PropertyType.Long);
            //AddNameToTypeMapping(PropertyKey.PlayerHasStone, PropertyType.Long);
            //AddNameToTypeMapping(PropertyKey.PlayerHasGold, PropertyType.Long);
            //AddNameToTypeMapping(PropertyKey.PlayerCapitalPos, PropertyType.String);
            //AddNameToTypeMapping(PropertyKey.PlayerActionPoint, PropertyType.Long);
            //AddNameToTypeMapping(PropertyKey.CurrentPos, PropertyType.String);

            //// Population
            //AddNameToTypeMapping(PropertyKey.VillageStillCanConcludeAmount, PropertyType.Int);
            //AddNameToTypeMapping(PropertyKey.VillageQuestRewardAmount, PropertyType.Int);

            //// Mail
            //AddNameToTypeMapping(PropertyKey.MailTotalUnreadAmount, PropertyType.Int);

            //// Hero
            //AddNameToTypeMapping(PropertyKey.HeroRecruitCount, PropertyType.Int);
            //AddNameToTypeMapping(PropertyKey.HeroPracticeCount, PropertyType.Int);
            //AddNameToTypeMapping(PropertyKey.HeroTalentCount, PropertyType.Int);
            //AddNameToTypeMapping(PropertyKey.HeroSkillCount, PropertyType.Int);

            //// Clan
            //AddNameToTypeMapping(PropertyKey.ClanVerifyReqJoinAmount, PropertyType.Int);

            //AddNameToTypeMapping(PropertyKey.CircadianInDay, PropertyType.Bool);
            //AddNameToTypeMapping(PropertyKey.CircadianRhythmRate, PropertyType.Float);
            //AddNameToTypeMapping(PropertyKey.CircadianRhythmTimeInSecond, PropertyType.TimeSpan);
        }

        static public PropertyNameInfo Instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = new PropertyNameInfo();
                }
                return s_instance;
            }
        }

        public Dictionary<PropertyKey, PropertyType> NameToTypeDic { get; private set; }
        private void AddNameToTypeMapping(PropertyKey key, PropertyType type)
        {
            if (NameToTypeDic.ContainsKey(key))
            {
                LogUtil.Debug.LogErrorFormat("propertyName( {0} ) existed in NameToType Map!!!! Please check setting!!!!", key);
                return;
            }

            NameToTypeDic.Add(key, type);
        }

        internal class PropertyKeyComparer : IEqualityComparer<PropertyKey>
        {
            public bool Equals(PropertyKey x, PropertyKey y)
            {
                return x == y;
            }

            public int GetHashCode(PropertyKey obj)
            {
                return (int)obj;
            }
        }
    }
}