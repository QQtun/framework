namespace Core.Game.Network
{
    public static class ErrorCode
    {
        public const int Success_No_Info = 0;
        public const int Success = 1;
        public const int Success_Bind = 2;

        public const int Error_Invalid_DBID = -1;
        public const int Error_Invalid_Index = -2;
        public const int Error_Config_Fault = -3;
        public const int Error_Data_Overdue = -4;
        public const int Error_Invalid_Operation = -5;
        public const int Error_Goods_Not_Enough = -6;
        public const int Error_Goods_Is_Using = -7;
        public const int Error_Goods_Not_Find = -8;
        public const int Error_JinBi_Not_Enough = -9;
        public const int Error_ZuanShi_Not_Enough = -10;
        public const int Error_Operation_Faild = -11;
        public const int Error_Operation_Denied = -12;
        public const int Error_Type_Not_Match = -13;
        public const int Error_MoneyType_Not_Select = -14;
        public const int Error_DB_Faild = -15;
        public const int Error_No_Residue_Degree = -16;
        public const int Error_BangZuan_Not_Enough = -17;
        public const int Error_Invalid_Params = -18;
        public const int Error_Level_Limit = -19;
        public const int Error_Not_Exist = -20;
        public const int Error_Denied_In_Current_Map = -21;
        public const int Error_Player_Count_Limit = -22;
        public const int Error_SpecJiFen_Not_Enough = -24;
        public const int Error_Cannot_Have_Wish_Txt = -25;
        public const int Error_Wish_Txt_Length_Limit = -26;
        public const int Error_Cannot_Wish_Self = -27;
        public const int Error_Wish_Player_Not_Exist = -28;
        public const int Error_Wish_Player_Not_Marry = -29;
        public const int Error_Wish_Type_Is_In_CD = -30;
        public const int Error_Wish_In_Balance_Time = -31;
        public const int Error_BagNum_Not_Enough = -100;
        public const int Error_Has_Get = -200;
        public const int Error_Faild_No_Message = -201;
        public const int Error_Has_Ownen_ShengBei = -300;
        public const int Error_Too_Far = -301;
        public const int Error_Other_Has_Get = -302;
        public const int Error_CaiJi_Break = -303;
        public const int Error_ZhanMeng_Not_In_ZhanMeng = -1000;
        public const int Error_ZhanMeng_Not_Exist = -1001;
        public const int Error_ZhanMeng_ShouLing_Only = -1002;
        public const int Error_ZhanMeng_Is_Unqualified = -1003;
        public const int Error_ZhanMeng_Has_Bid_OtherSite = -1004;
        public const int Error_ZhanMeng_ZhiWu_Not_Config = -1005;
        public const int Error_ZhanMeng_Not_Allowed_Change_LuoLanChengZhu = -1006;
        public const int Error_ZhanMeng_Not_Allowed_Change_ShengYuChengZhu = -1007;
        public const int Error_Not_In_valid_Time = -2001;
        public const int Error_Denied_In_Activity_Time = -2002;
        public const int Error_Time_Over = -2003;
        public const int Error_Time_Punish = -2004;
        public const int Error_Operate_Too_Fast = -2005;
        public const int Error_In_ZhanMeng_Time_Not_Enough = -2006;
        public const int Error_Is_Not_LuoLanChengZhu = -3001;
        public const int Error_Is_Not_Married = -3002;
        public const int Error_KuaFuFuBenNotExist = -4000;
        public const int Error_HasInQueue = -4001;
        public const int Error_HasInKuaFuFuBen = -4002;
        public const int Error_Invalid_GameType = -4003;
        public const int Error_Reach_Max_Level = -4004;
        public const int Error_ZhanMeng_Has_SignUp = -4005;
        public const int Error_DB_TimeOut = -10000;
        public const int Error_Server_Busy = -11000;
        public const int Error_Server_Not_Registed = -11001;
        public const int Error_Duplicate_Key_ServerId = -11002;
        public const int Error_Server_Internal_Error = -11003;
        public const int Error_Not_Implement = -11004;
        public const int Error_Connection_Disabled = -11005;
        public const int Error_Connection_Closing = -11006;
        public const int Error_Redirect_Orignal_Server = -11007;
        public const int Error_Token_Expired = -100;
        public const int Error_Version_Not_Match = -2;
        public const int Error_Server_Connections_Limit = -100;
        public const int Error_Version_Not_Match2 = -3;
        public const int Error_Token_Expired2 = -1;
        public const int Error_Connection_Closing2 = -2;
        public const int Error_Game_Over = -4006;
        public static string GetErrMsg(int errCode, bool hefuluolan = false, bool inKuafuhuodong = false)
        {
            string errMsg = "未知错误...";
            if (errCode < 0)
            {
                switch (errCode)
                {
                    case Error_Invalid_DBID:
                        errMsg = "无效的dbid";
                        break;
                    case Error_Invalid_Index:
                        errMsg = "无效的索引值";
                        break;
                    case Error_Config_Fault:
                        errMsg = "配置错误";
                        break;
                    case Error_Invalid_Operation:
                        errMsg = "无效操作";
                        break;
                    case Error_Goods_Not_Enough:
                        errMsg = "物品不足！";
                        break;
                    case Error_Goods_Is_Using:
                        errMsg = "物品在使用,不允许操作";
                        break;
                    case Error_Goods_Not_Find:
                        errMsg = "物品未找到！";
                        break;
                    case Error_ZuanShi_Not_Enough:
                        errMsg = "钻石不足";
                        break;
                    case Error_Operation_Faild:
                        errMsg = "操作失败";
                        break;
                    case Error_Type_Not_Match:
                        errMsg = "拒绝操作";
                        break;
                    case Error_MoneyType_Not_Select:
                        errMsg = "未选择消耗钱类型";
                        break;
                    case Error_DB_Faild:
                        errMsg = "数据库操作失败";
                        break;
                    case Error_No_Residue_Degree:
                        errMsg = "无剩余次数";
                        break;
                    case Error_BangZuan_Not_Enough:
                        errMsg = "绑钻不足";
                        break;
                    case Error_Invalid_Params:
                        errMsg = "错误的参数";
                        break;
                    case Error_Level_Limit:
                        errMsg = "等级不足,无法进入";
                        break;
                    case Error_Not_Exist:
                        errMsg = "不存在";
                        break;
                    case Error_BagNum_Not_Enough:
                        errMsg = "背包不足";
                        break;
                    case Error_JinBi_Not_Enough:
                        errMsg = "金币不足！";
                        break;
                    case Error_ZhanMeng_Not_In_ZhanMeng:
                        errMsg = "不在战盟中！";
                        break;
                    case Error_ZhanMeng_Not_Exist:
                        errMsg = "战盟不存在！";
                        break;
                    case Error_ZhanMeng_ShouLing_Only:
                        errMsg = "战盟首领才能进行此操作！";
                        break;
                    case Error_ZhanMeng_Is_Unqualified:
                        errMsg = "战盟没有参战资格！";
                        break;
                    case Error_ZhanMeng_Has_Bid_OtherSite:
                        errMsg = "战盟已经竞标了另一个名额！";
                        break;
                    case Error_ZhanMeng_ZhiWu_Not_Config:
                        errMsg = "角色的战盟职务未配置相应奖励！";
                        break;
                    case Error_Not_In_valid_Time:
                        if (hefuluolan)
                        {
                            errMsg = "活动未开启！";
                        }
                        else
                        {
                            errMsg = "竞技时间已结束！";
                        }
                        break;
                    case Error_Denied_In_Activity_Time:
                        if (inKuafuhuodong)
                            errMsg = "您已报名（地图1008、勇者战场),禁止参加此类活动";
                        else
                            errMsg = "活动时间禁止操作";
                        break;
                    case Error_ZhanMeng_Not_Allowed_Change_LuoLanChengZhu:
                        errMsg = "罗兰城主持有期间不能委任其他成员为战盟首领";
                        break;
                    case Error_KuaFuFuBenNotExist:
                        errMsg = "跨服副本不存在(已结束)！";
                        break;
                    case Error_HasInQueue:
                        errMsg = "已经在匹配队列中！";
                        break;
                    case Error_HasInKuaFuFuBen:
                        errMsg = "已经在副本中！";
                        break;
                    case Error_Server_Not_Registed:
                        errMsg = "活动暂未开放！";
                        break;
                    case Error_Server_Internal_Error:
                        errMsg = "服务器内部错误！";
                        break;
                    case Error_Has_Get:
                        errMsg = "已经领取！";
                        break;
                    case Error_Denied_In_Current_Map:
                        errMsg = "当前地图不允许此操作！";
                        break;
                    case Error_Time_Punish:
                        errMsg = "当前处于惩罚时间内，不能参加任何跨服副本！";
                        break;
                    case Error_Player_Count_Limit:
                        errMsg = "参与者数量已满！";
                        break;
                    case Error_Reach_Max_Level:
                        errMsg = "已经达到最高等级！";
                        break;
                    case Error_ZhanMeng_Has_SignUp:
                        errMsg = "战盟已经报名！";
                        break;
                    case Error_Game_Over:
                        errMsg = "战斗已结束！";
                        break;
                    case Error_Operate_Too_Fast:
                        errMsg = "您的操作太快，请稍后再试！";
                        break;
                    case Error_Operation_Denied:
                        errMsg = "条件不足！";
                        break;
                    case Error_ZhanMeng_Not_Allowed_Change_ShengYuChengZhu:
                        errMsg = "圣域领主持有期间不能委任其他成员为战盟首领";
                        break;
                    case Error_In_ZhanMeng_Time_Not_Enough:
                        errMsg = "加入战盟时间不足!";
                        break;
                    case Error_SpecJiFen_Not_Enough:
                        errMsg = "专享积分不足！";
                        break;
                    case Error_Cannot_Have_Wish_Txt:
                        errMsg = "不可添加祝福寄语";
                        break;
                    case Error_Wish_Txt_Length_Limit:
                        errMsg = "祝福寄语字符数超出限制";
                        break;
                    case Error_Cannot_Wish_Self:
                        errMsg = "被祝福玩家不能是自己";
                        break;
                    case Error_Wish_Player_Not_Exist:
                        errMsg = "被祝福玩家不存在";
                        break;
                    case Error_Wish_Player_Not_Marry:
                        errMsg = "被祝福玩家未结婚";
                        break;
                    case Error_Wish_Type_Is_In_CD:
                        errMsg = "其他玩家正在赠送,请稍后再试";
                        break;
                    case Error_Wish_In_Balance_Time:
                        errMsg = "赠送已结束，系统正在结算排名";
                        break;
                    default:
                        errMsg = "其他错误！错误码[" + errCode + "]";
                        break;
                }
            }
            return errMsg;
        }
    }
}