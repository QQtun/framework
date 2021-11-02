using UnityEngine;

//給Editor用的Class
public class ExcelDate
{
    public GameObject dataObj;
    public CommonData data;
}

public class CommonData
{
    public int X; //生成X軸 記得乘100 單位 unit*100
    public int Y;//生成Y軸 記得乘100 單位 unit*100

    public int ID; //ID號
}

public class MonsterDate : CommonData
{
    public string MonsterNote;//生成怪物描述資料
    public int Radius;//生怪半徑 記得乘100 單位 unit*100
    public int MonsterInsMax; // 該生成點，最多生成上限 單位Int
    public int Timeslot; // 多少秒生成1次 單位 豪秒

}

public class NPCData : CommonData
{
    public string NPCNote;//NPC描述資料
    public int Direction;//NPC角度  單位 int 0~359
}

public class TeleportData : CommonData
{
    public int Direction;//傳送門角度  單位 int 0~359

    public string TeleportNote;//傳送門描述資料
    public string TeleportPrefab;//傳送門物件
    public int ToMapCode; //前往的地圖編號 單位 int
    public int ToX;//出傳送門後角色的X座標 單位 unit*100
    public int ToY;//出傳送門後角色的Y座標 單位 unit*100

}
