using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonEnum
{
    /// <summary>
    /// server,client共用的錯誤碼
    /// </summary>
    public enum CommonErrorCodeEnum
    {
        /// <summary>
        /// 無錯誤 預設為0  
        /// </summary>
        Success = 0,
        /// <summary>
        /// 資料庫取得資料失敗
        /// </summary>
        DBGetDataFailed = 10,
        /// <summary>
        /// 伺服器需等待
        /// </summary>
        ServerNeedWait = 11,
        /// <summary>
        /// 操作拒絕
        /// </summary>
        OperationDenied = 12,
        /// <summary>
        /// 玩家被禁止
        /// </summary>
        UserBan = 13,
        /// <summary>
        /// 用戶登入後, 判斷已經達到線路最大人數限制
        /// </summary>
        ServerBusy = 100,
        /// <summary>
        /// 版本不正確
        /// </summary>
        VersionNotMatch = 101,
        /// <summary>
        /// 登入token過期
        /// </summary>
        TokenExpired = 102,
        /// <summary>
        /// 已經連線過了
        /// </summary>
        AlreadyConnect = 103,
        /// <summary>
        /// 註冊user到DB失敗
        /// </summary>
        RegisterError = 104,
        /// <summary>
        /// 重新導向伺服器錯誤
        /// </summary>
        RedirectOrignalServerError = 105,
        /// <summary>
        /// 系統未開放
        /// </summary>
        SystemNotOpen = 106,
        /// <summary>
        /// 魔劍士不開放創角 (職業7)
        /// </summary>
        MagicSwordNotOpen = 1000,
        /// <summary>
        /// 非法角色名稱 (空白,非法字元)
        /// </summary>
        RoleNameInvalid = 1001,
        /// <summary>
        /// 角色名稱過長
        /// </summary>
        RoleNameTooLong = 1002,
        /// <summary>
        /// 處於創角限制時間中(會附帶剩餘等待秒數)
        /// </summary>
        CreateRoleLimitTime = 1003,
        /// <summary>
        /// 添加角色時，服務器角色已滿
        /// </summary>
        ServerRoleFull = 1004,
        /// <summary>
        /// 角色名稱在資料庫中為亂碼
        /// </summary>
        RoleNameChaosInDB = 1005,
        /// <summary>
        /// 角色名稱已存在
        /// </summary>
        RoleNameDuplicate = 1006,
        /// <summary>
        /// 創建魔劍士的條件不足
        /// </summary>
        MagicSwordConditionNotEnough = 1007,
        /// <summary>
        /// 超過創角數量上限(4個)
        /// </summary>
        CreateRoleNumLimit = 1008,
        /// <summary>
        /// 創建角色錯誤
        /// </summary>
        CreateRoleError = 1009,
        /// <summary>
        /// 名稱伺服器錯誤
        /// </summary>
        NameServerError = 1010,
        /// <summary>
        /// 創角參數有誤
        /// </summary>
        CreateRoleValueError = 1011,
    }
}
