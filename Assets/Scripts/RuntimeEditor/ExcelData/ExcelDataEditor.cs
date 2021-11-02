using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


public class ExcelDataEditor : MonoBehaviour
{

    [HideInInspector]
    //Data Key
    public string[] mapListKey = new string[] { "Monster", "NPC", "Teleport" };

    #region  Editor用
    public Dictionary<string, List<ExcelDate>> mapList = new Dictionary<string, List<ExcelDate>>();

    [HideInInspector]
    public int dataTypeNum;//當前選擇的類型

    #endregion

    public void Init()
    {
        //初始化
        for (int i = 0; i < mapListKey.Length; i++)
        {
            if (mapList.ContainsKey(mapListKey[i]) == false)
            {
                mapList.Add(mapListKey[i], new List<ExcelDate>());
            }
        }
    }

    void OnDrawGizmos()
    {
        DrawObj();
    }


    /// <summary>
    /// (Editor用)添加資料+3D物件
    /// </summary>
    /// <param name="key">key值</param>
    /// <param name="data">資料數據</param>
    /// <param name="parentObj">放置3D的父物件</param>
    /// <param name="objPos">放置位置</param>
    public void AddInputData(string key, ExcelDate data, GameObject parentObj, Vector3 objPos)
    {
        if (mapList.ContainsKey(key))
        {
            //新增時預設一個ID
            data.data.ID = mapList[key].Count + 1;

            //Editor資料
            GameObject obj = InsDataObj(key, parentObj, objPos);
            obj.name = key + data.data.ID;
            data.dataObj = obj;

            mapList[key].Add(data);
        }
        else
        {
            Debug.LogError("不存在的Data Key");
        }
    }


    public void DeleteData(string key, int mDataObjNum)
    {
        //刪除資料
        GameObject.DestroyImmediate(mapList[key][mDataObjNum].dataObj);
        mapList[key].RemoveAt(mDataObjNum);
    }


    /// <summary>
    /// (Editor用)返回該列表內容的 名字 + ID  
    /// </summary>
    public string[] GetListAllObjName(string key)
    {
        string[] data = new string[mapList[key].Count];

        for (var i = 0; i < mapList[key].Count; i++)
        {
            data[i] = key + " : " + mapList[key][i].data.ID;
        }

        return data;
    }


    /// <summary>
    /// (Editor用)輸出Excel文件
    /// 因為擔心xlsm 會有問題 所以 多個佔存文件xlsx
    /// </summary>
    /// <param name="tempPath">輸出佔存文件 (xlsx)</param>
    /// <param name="exportPath">輸出文件 (xlsm)</param>
    public void SaveExcelData(string tempPath, string exportPath)
    {

        //Temp文件
        foreach (var item in mapList)
        {
            using (ExcelPackage package = new ExcelPackage(new FileInfo(tempPath + "_" + item.Key + ".xlsx")))
            {
                SaveExcel(package, item.Key);
                SaveTempExcel(package, item.Key);

                try
                {
                    package.Save();//保存excel
                }
                catch (IOException e)
                {
                    Debug.LogError("請檢查是否有將Excel文件關閉");
                    Debug.LogError(e);
                }
                catch (System.InvalidOperationException e)
                {
                    Debug.LogError("請檢查是否有將Excel文件關閉");
                    Debug.LogError(e);
                }
                catch (Exception e)
                {
                    Debug.LogError("請通知程式人員 : " + e);
                }
            }
        }


        foreach (var item in mapList)
        {
            //正式資料
            using (ExcelPackage package = new ExcelPackage(new FileInfo(exportPath + "_" + item.Key + ".xlsm")))
            {

                SaveExcel(package, item.Key);

                //如果Excel沒有Vba 補上去，避免Excel認為是損毀檔
                if (package.Workbook.VbaProject == null)
                {
                    package.Workbook.CreateVBAProject();
                }

                try
                {
                    package.Save();//保存excel
                }
                catch (IOException e)
                {
                    Debug.LogError("請檢查是否有將Excel文件關閉");
                    Debug.LogError(e);
                }
                catch (System.InvalidOperationException e)
                {
                    Debug.LogError("請檢查是否有將Excel文件關閉");
                    Debug.LogError(e);
                }
                catch (Exception e)
                {
                    Debug.LogError("請通知程式人員 : " + e);
                }
            }
        }

    }


    /// <summary>
    /// 各資料表存檔方式
    /// 因各資料表格式不同所以要分開存放
    /// </summary>
    private void SaveExcel(ExcelPackage package, string Key)
    {
        ExcelWorksheet worksheet;//表單

        switch (Key) //TODO:寫輸出數據Excel方式就在這
        {
            case "Monster":
            {
                //確定是否有表格
                if (package.Workbook.Worksheets[Key] == null)
                {
                    worksheet = package.Workbook.Worksheets.Add(Key);//新增表單

                    //第一橫排 數據
                    worksheet.Cells[1, 1].Value = "怪物ID";
                    worksheet.Cells[1, 2].Value = "X座標";
                    worksheet.Cells[1, 3].Value = "Y座標";

                    worksheet.Cells[1, 4].Value = "怪物描述資料";
                    worksheet.Cells[1, 5].Value = "生怪半徑";
                    worksheet.Cells[1, 6].Value = "最多生成上限";
                    worksheet.Cells[1, 7].Value = "多少毫秒生成1次";


                    //第二橫排 數據
                    worksheet.Cells[2, 1].Value = "MonsterID";
                    worksheet.Cells[2, 2].Value = "X";
                    worksheet.Cells[2, 3].Value = "Y";

                    worksheet.Cells[2, 4].Value = "MonsterNote";
                    worksheet.Cells[2, 5].Value = "Radius";
                    worksheet.Cells[2, 6].Value = "MonsterInsMax";
                    worksheet.Cells[2, 7].Value = "Timeslot";
                }
                else
                {
                    worksheet = package.Workbook.Worksheets[Key];
                    ResteExcelData(worksheet);
                }

                //寫入資料 因Excel插件是從 1 開始數 要小心 
                for (int i = 0; i < mapList[Key].Count; i++)
                {
                    MonsterDate data = (MonsterDate)mapList[Key][i].data;

                    int row = i + 3;//直排 開始位置是 第三格

                    worksheet.Cells[row, 1].Value = data.ID;

                    worksheet.Cells[row, 2].Value = data.X;
                    worksheet.Cells[row, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[row, 2].Style.Fill.BackgroundColor.SetColor(
                        System.Drawing.Color.Red);

                    worksheet.Cells[row, 3].Value = data.Y;
                    worksheet.Cells[row, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[row, 3].Style.Fill.BackgroundColor.SetColor(
                        System.Drawing.Color.Red);

                    worksheet.Cells[row, 4].Value = data.MonsterNote;
                    worksheet.Cells[row, 5].Value = data.Radius;
                    worksheet.Cells[row, 6].Value = data.MonsterInsMax;
                    worksheet.Cells[row, 7].Value = data.Timeslot;
                }

                break;
            }
            case "NPC":
            {
                //確定是否有表格
                if (package.Workbook.Worksheets[Key] == null)
                {
                    worksheet = package.Workbook.Worksheets.Add(Key);//新增表單

                    //第一橫排 數據
                    worksheet.Cells[1, 1].Value = "NPCID";
                    worksheet.Cells[1, 2].Value = "X座標";
                    worksheet.Cells[1, 3].Value = "Y座標";

                    worksheet.Cells[1, 4].Value = "NPC描述資料";
                    worksheet.Cells[1, 5].Value = "NPC角度";

                    //第二橫排 數據
                    worksheet.Cells[2, 1].Value = "NPCID";
                    worksheet.Cells[2, 2].Value = "X";
                    worksheet.Cells[2, 3].Value = "Y";

                    worksheet.Cells[2, 4].Value = "NPCNote";
                    worksheet.Cells[2, 5].Value = "Direction";
                }
                else
                {
                    worksheet = package.Workbook.Worksheets[Key];
                    ResteExcelData(worksheet);
                }

                //寫入資料 因Excel插件是從 1 開始數 要小心 
                for (int i = 0; i < mapList[Key].Count; i++)
                {
                    NPCData data = (NPCData)mapList[Key][i].data;

                    int row = i + 3;//直排 開始位置是 第三格

                    worksheet.Cells[row, 1].Value = data.ID;

                    worksheet.Cells[row, 2].Value = data.X;
                    worksheet.Cells[row, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[row, 2].Style.Fill.BackgroundColor.SetColor(
                        System.Drawing.Color.Red);

                    worksheet.Cells[row, 3].Value = data.Y;
                    worksheet.Cells[row, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[row, 3].Style.Fill.BackgroundColor.SetColor(
                        System.Drawing.Color.Red);

                    worksheet.Cells[row, 4].Value = data.NPCNote;
                    worksheet.Cells[row, 5].Value = data.Direction;
                }
            }
            break;
            case "Teleport":
            {
                //確定是否有表格
                if (package.Workbook.Worksheets[Key] == null)
                {
                    worksheet = package.Workbook.Worksheets.Add(Key);//新增表單

                    //第一橫排 數據
                    worksheet.Cells[1, 1].Value = "傳送門ID";
                    worksheet.Cells[1, 2].Value = "X座標";
                    worksheet.Cells[1, 3].Value = "Y座標";


                    worksheet.Cells[1, 4].Value = "傳送門角度";
                    worksheet.Cells[1, 5].Value = "傳送門資料";
                    worksheet.Cells[1, 6].Value = "傳送門物件";
                    worksheet.Cells[1, 7].Value = "前往的地圖編號";
                    worksheet.Cells[1, 8].Value = "出傳送門後角色的X座標";
                    worksheet.Cells[1, 9].Value = "出傳送門後角色的Y座標";

                    //第二橫排 數據
                    worksheet.Cells[1, 1].Value = "TeleportID";
                    worksheet.Cells[2, 2].Value = "X";
                    worksheet.Cells[2, 3].Value = "Y";

                    worksheet.Cells[1, 4].Value = "Direction";
                    worksheet.Cells[1, 5].Value = "TeleportNote";
                    worksheet.Cells[1, 6].Value = "TeleportPrefab";
                    worksheet.Cells[1, 7].Value = "ToMapCode";
                    worksheet.Cells[1, 8].Value = "ToX";
                    worksheet.Cells[1, 9].Value = "ToY";
                }
                else
                {
                    worksheet = package.Workbook.Worksheets[Key];
                    ResteExcelData(worksheet);
                }

                //寫入資料 因Excel插件是從 1 開始數 要小心 
                for (int i = 0; i < mapList[Key].Count; i++)
                {
                    TeleportData data = (TeleportData)mapList[Key][i].data;

                    int row = i + 3;//直排 開始位置是 第二格

                    worksheet.Cells[row, 1].Value = data.ID;

                    worksheet.Cells[row, 2].Value = data.X;
                    worksheet.Cells[row, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[row, 2].Style.Fill.BackgroundColor.SetColor(
                        System.Drawing.Color.Red);

                    worksheet.Cells[row, 3].Value = data.Y;
                    worksheet.Cells[row, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[row, 3].Style.Fill.BackgroundColor.SetColor(
                        System.Drawing.Color.Red);

                    worksheet.Cells[row, 4].Value = data.Direction;
                    worksheet.Cells[row, 5].Value = data.TeleportNote;
                    worksheet.Cells[row, 6].Value = data.TeleportPrefab;
                    worksheet.Cells[row, 7].Value = data.ToMapCode;
                    worksheet.Cells[row, 8].Value = data.ToX;
                    worksheet.Cells[row, 9].Value = data.ToY;
                }
            }
            break;
        }

    }


    /// <summary>
    /// (Editor用)存放佔存數據
    /// </summary>
    private void SaveTempExcel(ExcelPackage package, string key)
    {
        ExcelWorksheet worksheet = null;//表單

        int index = 0;

        string type = "";

        switch (key)//TODO:Editor用資料
        {
            case "Monster":
            {
                //確定是否有表格
                if (package.Workbook.Worksheets["TempMonster"] == null)
                {
                    worksheet = package.Workbook.Worksheets.Add("TempMonster");//新增表單

                    worksheet.Cells[1, 1].Value = "類別";
                    worksheet.Cells[1, 2].Value = "ID";
                    worksheet.Cells[1, 3].Value = "Ｘ";
                    worksheet.Cells[1, 4].Value = "Ｙ";
                    worksheet.Cells[1, 5].Value = "Ｚ";

                    //因Excel輸出是兩列，所以要多一列
                    worksheet.Cells[2, 1].Value = "類別";
                    worksheet.Cells[2, 2].Value = "ID";
                    worksheet.Cells[2, 3].Value = "Ｘ";
                    worksheet.Cells[2, 4].Value = "Ｙ";
                    worksheet.Cells[2, 5].Value = "Ｚ";
                }
                else
                {
                    worksheet = package.Workbook.Worksheets["TempMonster"];
                    ResteExcelData(worksheet);
                }
                type = "Monster";
                break;
            }
            case "NPC":
            {
                //確定是否有表格
                if (package.Workbook.Worksheets["TempNPC"] == null)
                {
                    worksheet = package.Workbook.Worksheets.Add("TempNPC");//新增表單

                    worksheet.Cells[1, 1].Value = "類別";
                    worksheet.Cells[1, 2].Value = "ID";
                    worksheet.Cells[1, 3].Value = "Ｘ";
                    worksheet.Cells[1, 4].Value = "Ｙ";
                    worksheet.Cells[1, 5].Value = "Ｚ";

                    //因Excel輸出是兩列，所以要多一列
                    worksheet.Cells[2, 1].Value = "類別";
                    worksheet.Cells[2, 2].Value = "ID";
                    worksheet.Cells[2, 3].Value = "Ｘ";
                    worksheet.Cells[2, 4].Value = "Ｙ";
                    worksheet.Cells[2, 5].Value = "Ｚ";
                }
                else
                {
                    worksheet = package.Workbook.Worksheets["TempNPC"];
                    ResteExcelData(worksheet);
                }
                type = "NPC";
                break;
            }
            case "Teleport":
            {
                //確定是否有表格
                if (package.Workbook.Worksheets["TempTeleport"] == null)
                {
                    worksheet = package.Workbook.Worksheets.Add("TempTeleport");//新增表單

                    worksheet.Cells[1, 1].Value = "類別";
                    worksheet.Cells[1, 2].Value = "ID";
                    worksheet.Cells[1, 3].Value = "Ｘ";
                    worksheet.Cells[1, 4].Value = "Ｙ";
                    worksheet.Cells[1, 5].Value = "Ｚ";

                    //因Excel輸出是兩列，所以要多一列
                    worksheet.Cells[2, 1].Value = "類別";
                    worksheet.Cells[2, 2].Value = "ID";
                    worksheet.Cells[2, 3].Value = "Ｘ";
                    worksheet.Cells[2, 4].Value = "Ｙ";
                    worksheet.Cells[2, 5].Value = "Ｚ";
                }
                else
                {
                    worksheet = package.Workbook.Worksheets["TempTeleport"];
                    ResteExcelData(worksheet);
                }
                type = "Teleport";
            }
            break;
        }


        foreach (var itemList in mapList[key])
        {
            int row = index + 3;//直排 開始位置是 第三格
            worksheet.Cells[row, 1].Value = type;
            worksheet.Cells[row, 2].Value = itemList.data.ID;
            worksheet.Cells[row, 3].Value = itemList.dataObj.transform.position.x;
            worksheet.Cells[row, 4].Value = itemList.dataObj.transform.position.y;
            worksheet.Cells[row, 5].Value = itemList.dataObj.transform.position.z;
            index++;
        }

    }

    private void ResteExcelData(ExcelWorksheet worksheet)
    {
        //確認總共有幾列
        int rowCount = worksheet.Dimension.Rows;

        //因刪除一排後，他會自己往上重整，所以重複刪同一列就好
        for (int i = 3; i <= rowCount; i++)
        {
            worksheet.DeleteRow(3);
        }
    }


    /// <summary>
    ///  (Editor用)讀檔取Temp的editor存檔
    /// </summary>
    /// <param name="temppath">暫存資料</param>
    /// <param name="exportpath">輸出位置</param>
    /// <param name="parentObj">放置3D物件的父物件</param>
    public void LoadExcelData(string temppath, string exportpath, GameObject parentObj)
    {
        //清除掉
        mapList.Clear();
        Init();

        foreach (var item in mapList)
        {
            //資料
            using (ExcelPackage package = new ExcelPackage(new FileInfo(exportpath + "_" + item.Key + ".xlsm")))
            {
                ExcelPackage temppackage = new ExcelPackage(new FileInfo(temppath + "_" + item.Key + ".xlsx"));

                LoadExcel(temppackage, package, item.Key, parentObj);

            }
        }


    }

    private void LoadExcel(ExcelPackage temp, ExcelPackage export, string Key, GameObject parentObj)
    {
        ExcelWorksheet worksheet;//資料表
        ExcelWorksheet editorWorksheet;//Temp表

        int rowCount = 0;


        //基本資料
        switch (Key) //TODO:寫讀取數據Excel方式就在這
        {
            case "Monster":
            {
                //確定是否有表格
                if (export.Workbook.Worksheets[Key] == null)
                {
                    Debug.LogError(Key + "表格找不到");
                    break;
                }
                else
                {
                    worksheet = export.Workbook.Worksheets[Key];
                }

                //確定是否Editor資料表格
                if (temp.Workbook.Worksheets["TempMonster"] == null)
                {
                    Debug.LogError("Temp 表格找不到");
                    return;
                }
                else
                {
                    editorWorksheet = temp.Workbook.Worksheets["TempMonster"];
                }

                rowCount = worksheet.Dimension.Rows;

                //讀取資料
                for (int row = 3; row <= rowCount; row++)
                {
                    #region 資料讀入
                    MonsterDate data = new MonsterDate();

                    //因不接受轉型，只能先轉String 再做下一步
                    if (int.TryParse(worksheet.Cells[row, 1].Value.ToString(), out data.ID) == false)
                    {
                        Debug.LogError("data.ID  無法讀取 強制停止");
                        break;
                    }

                    if (int.TryParse(worksheet.Cells[row, 2].Value.ToString(), out data.X) == false)
                    {
                        Debug.LogError("data.X  無法讀取 強制停止");
                        break;
                    }

                    if (int.TryParse(worksheet.Cells[row, 3].Value.ToString(), out data.Y) == false)
                    {
                        Debug.LogError("data.Y  無法讀取 強制停止");
                        break;
                    }


                    data.MonsterNote =
                        worksheet.Cells[row, 4].Value != null ? worksheet.Cells[row, 4].Value.ToString() : "";

                    data.Radius =
                        worksheet.Cells[row, 5].Value != null ? int.Parse(worksheet.Cells[row, 5].Value.ToString()) : 0;

                    data.MonsterInsMax =
                        worksheet.Cells[row, 6].Value != null ? int.Parse(worksheet.Cells[row, 6].Value.ToString()) : 0;

                    data.Timeslot =
                        worksheet.Cells[row, 7].Value != null ? int.Parse(worksheet.Cells[row, 7].Value.ToString()) : 0;

                    #endregion

                    #region editor資料
                    Vector3 pos = new Vector3(
                       data.X / 100.0f,
                       float.Parse(editorWorksheet.Cells[row, 4].Value.ToString()),
                       data.Y / 100.0f
                       );
                    GameObject obj = InsDataObj("Monster", parentObj, pos);
                    obj.name = "Monster" + editorWorksheet.Cells[row, 2].Value.ToString();
                    #endregion

                    mapList[Key].Add(new ExcelDate() { dataObj = obj, data = data });
                }

            }
            break;
            case "NPC":
            {
                //確定是否有表格
                if (export.Workbook.Worksheets[Key] == null)
                {
                    Debug.LogError(Key + "表格找不到");
                    break;
                }
                else
                {
                    worksheet = export.Workbook.Worksheets[Key];
                }

                //確定是否Editor資料表格
                if (temp.Workbook.Worksheets["TempNPC"] == null)
                {
                    Debug.LogError("Temp 表格找不到");
                    return;
                }
                else
                {
                    editorWorksheet = temp.Workbook.Worksheets["TempNPC"];
                }

                rowCount = worksheet.Dimension.Rows;

                //讀取資料
                for (int row = 3; row <= rowCount; row++)
                {
                    #region 資料讀入
                    NPCData data = new NPCData();


                    //因不接受轉型，只能先轉String 再做下一步
                    if (int.TryParse(worksheet.Cells[row, 1].Value.ToString(), out data.ID) == false)
                    {
                        Debug.LogError("data.ID  無法讀取 強制停止");
                        break;
                    }

                    if (int.TryParse(worksheet.Cells[row, 2].Value.ToString(), out data.X) == false)
                    {
                        Debug.LogError("data.X  無法讀取 強制停止");
                        break;
                    }

                    if (int.TryParse(worksheet.Cells[row, 3].Value.ToString(), out data.Y) == false)
                    {
                        Debug.LogError("data.Y  無法讀取 強制停止");
                        break;
                    }


                    data.NPCNote =
                        worksheet.Cells[row, 4].Value != null ? worksheet.Cells[row, 4].Value.ToString() : "";

                    data.Direction =
                        worksheet.Cells[row, 5].Value != null ? int.Parse(worksheet.Cells[row, 5].Value.ToString()) : 0;

                    #endregion

                    #region editor資料
                    Vector3 pos = new Vector3(
                       data.X / 100.0f,
                       float.Parse(editorWorksheet.Cells[row, 4].Value.ToString()),
                       data.Y / 100.0f
                       );
                    GameObject obj = InsDataObj("NPC", parentObj, pos);
                    obj.name = "NPC" + editorWorksheet.Cells[row, 2].Value.ToString();
                    #endregion

                    obj.transform.rotation = Quaternion.Euler(0, data.Direction, 0);

                    mapList[Key].Add(new ExcelDate() { dataObj = obj, data = data });
                }
            }
            break;
            case "Teleport":
            {
                //確定是否有表格
                if (export.Workbook.Worksheets[Key] == null)
                {
                    Debug.LogError(Key + "表格找不到");
                    break;
                }
                else
                {
                    worksheet = export.Workbook.Worksheets[Key];
                }

                //確定是否Editor資料表格
                if (temp.Workbook.Worksheets["TempTeleport"] == null)
                {
                    Debug.LogError("Temp 表格找不到");
                    return;
                }
                else
                {
                    editorWorksheet = temp.Workbook.Worksheets["TempTeleport"];
                }

                rowCount = worksheet.Dimension.Rows;

                //讀取資料
                for (int row = 3; row <= rowCount; row++)
                {
                    #region 資料讀入
                    TeleportData data = new TeleportData();

                    if (int.TryParse(worksheet.Cells[row, 1].Value.ToString(), out data.ID) == false)
                    {
                        Debug.LogError("data.ID  無法讀取 強制停止");
                        break;
                    }

                    if (int.TryParse(worksheet.Cells[row, 2].Value.ToString(), out data.X) == false)
                    {
                        Debug.LogError("data.X  無法讀取 強制停止");
                        break;
                    }

                    if (int.TryParse(worksheet.Cells[row, 3].Value.ToString(), out data.Y) == false)
                    {
                        Debug.LogError("data.Y  無法讀取 強制停止");
                        break;
                    }

                    data.Direction =
                        worksheet.Cells[row, 4].Value != null ? int.Parse(worksheet.Cells[row, 4].Value.ToString()) : 0;

                    data.TeleportNote =
                        worksheet.Cells[row, 5].Value != null ? worksheet.Cells[row, 5].Value.ToString() : "";

                    data.TeleportPrefab =
                        worksheet.Cells[row, 6].Value != null ? worksheet.Cells[row, 6].Value.ToString() : "";

                    data.ToMapCode =
                        worksheet.Cells[row, 7].Value != null ? int.Parse(worksheet.Cells[row, 7].Value.ToString()) : 0;

                    data.ToX =
                        worksheet.Cells[row, 8].Value != null ? int.Parse(worksheet.Cells[row, 8].Value.ToString()) : 0;

                    data.ToY =
                        worksheet.Cells[row, 9].Value != null ? int.Parse(worksheet.Cells[row, 9].Value.ToString()) : 0;
                    #endregion

                    #region editor資料
                    Vector3 pos = new Vector3(
                       data.X / 100.0f,
                       float.Parse(editorWorksheet.Cells[row, 4].Value.ToString()),
                       data.Y / 100.0f
                       );
                    GameObject obj = InsDataObj("Teleport", parentObj, pos);
                    obj.name = "Teleport" + editorWorksheet.Cells[row, 2].Value.ToString();
                    #endregion

                    obj.transform.rotation = Quaternion.Euler(0, data.Direction, 0);

                    mapList[Key].Add(new ExcelDate() { dataObj = obj, data = data });
                }
            }
            break;
        }

    }


    /// <summary>
    /// (Editor用)添加物件
    /// </summary>
    /// <param name="type"></param>
    /// <param name="parentObj"></param>
    /// <param name="objPos"></param>
    public GameObject InsDataObj(string type, GameObject parentObj, Vector3 objPos)
    {
        GameObject obj = DataObj(type);
        obj.transform.parent = parentObj.transform;
        obj.transform.position = objPos;
        return obj;
    }

    /// <summary>
    /// (Editor用)生成放置物件
    /// </summary>
    private GameObject DataObj(string type)
    {
        //生成一個有指向性的物件(因為有旋轉問題)
        GameObject point = null;
        GameObject pointDirection;

        //放置的物件內容
        switch (type)
        {
            case "Monster": //怪物
            {
                //生成模型
                point = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                point.transform.localScale /= 5;
            }
            break;
            case "NPC": //NPC
            {
                //生成模型(帶指向性)
                point = GameObject.CreatePrimitive(PrimitiveType.Cube);
                pointDirection = GameObject.CreatePrimitive(PrimitiveType.Cube);
                pointDirection.transform.parent = point.transform;
                pointDirection.transform.position = new Vector3(0, 0, 0.5f);
                pointDirection.transform.localScale = new Vector3(0.5f, 0.5f, 1);

                var tempMaterial = new Material(Shader.Find("Unlit/Color"));
                tempMaterial.color = Color.red;
                pointDirection.GetComponent<MeshRenderer>().material = tempMaterial;

                point.transform.localScale /= 5;

            }
            break;
            case "Teleport": //傳送門
            {
                //生成模型(帶指向性)
                point = GameObject.CreatePrimitive(PrimitiveType.Cube);
                pointDirection = GameObject.CreatePrimitive(PrimitiveType.Cube);
                pointDirection.transform.parent = point.transform;
                pointDirection.transform.position = new Vector3(0, 0, 0.5f);
                pointDirection.transform.localScale = new Vector3(0.5f, 0.5f, 1);

                var tempMaterial = new Material(Shader.Find("Unlit/Color"));
                tempMaterial.color = Color.red;
                pointDirection.GetComponent<MeshRenderer>().material = tempMaterial;

                point.transform.localScale /= 5;
            }
            break;
        }

        return point;
    }


    #region  繪圖

    private void DrawObj()
    {
        switch (dataTypeNum)//TODO:跟Draw有關的
        {
            case 0://怪物
            {
                foreach (var item in mapList["Monster"])
                {
                    MonsterDate mdata = (MonsterDate)item.data;
                    float r = mdata.Radius / 100.0f;
                    Gizmos.color = Color.green;

                    //畫線
                    for (int i = 0; i < 360; i++)
                    {
                        //計算x、z座標，乘上半徑r
                        float x = Mathf.Cos((360 * (i + 1) / 360) * Mathf.Deg2Rad) * r;
                        float z = Mathf.Sin((360 * (i + 1) / 360) * Mathf.Deg2Rad) * r;
                        float xLast = Mathf.Cos((360 * (i) / 360) * Mathf.Deg2Rad) * r;
                        float zLast = Mathf.Sin((360 * (i) / 360) * Mathf.Deg2Rad) * r;

                        //上一點座標
                        Vector3 lastPoint = item.dataObj.transform.position + new Vector3(xLast, 0, zLast);
                        //現在座標
                        Vector3 nowPoint = item.dataObj.transform.position + new Vector3(x, 0, z);

                        //畫線
                        Gizmos.DrawLine(lastPoint, nowPoint);
                    }
                }
            }
            break;
            case 1: //NPC
            {
                //暫無Draw需求
            }
            break;
            case 2://傳送門
            {
                //暫無Draw需求
            }
            break;
        }
    }

    #endregion

}




