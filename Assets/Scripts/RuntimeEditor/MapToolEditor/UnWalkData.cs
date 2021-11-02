using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System;
using System.Xml;
using System.Linq;
using Core.Framework.Map;
using UnityEditor;

/// <summary>
/// 區域類型
/// </summary>
public enum AreanType
{
    Obstruction, //障礙區
    Safe,       //安全區
    Boundary,   //邊界區域

    Room_inside,//室內
    Room_outside, //室外
    Room_other, //其他
    Other,

    Height,     //高度區域

    Room_wall, //牆

    Water,      //水
    Cement, // 水泥地
    Soil, // 土地
    Macadam, //碎石地
    Lawn, //樹葉草地
    Metal, //金屬
    Slurry, //泥地泥漿
    Snowfield, // 雪地
    Woodenfloor, // 木製地闆
    Mute,//靜音區域

}

/// <summary>
/// 不可行走區域
/// </summary>
public class UnWalkData
{
    private int mLastNum = 0;
    public int LastPointNum
    {
        get { return mLastNum; }
        set { mLastNum = value; }
    }

    private string mAreaName;

    public string AreaName
    {
        get
        {
            if (areaType == AreanType.Obstruction)
            {
                return "障礙區" + areaId;
            }
            else if (areaType == AreanType.Safe)
            {
                return "安全區" + areaId;
            }
            else if (areaType == AreanType.Boundary)
            {
                return "邊界" + areaId;
            }
            else if (areaType == AreanType.Room_inside)
                return "室內" + areaId;
            else if (areaType == AreanType.Room_outside)
                return "室外" + areaId;
            else if (areaType == AreanType.Room_other)
                return "寢室其他" + areaId;
            else if (areaType == AreanType.Height)
                return "高度區" + areaId;
            else if (areaType == AreanType.Room_wall)
                return "牆壁" + areaId;
            else if (areaType == AreanType.Water)
                return "水域" + areaId;
            else if (areaType == AreanType.Soil)
                return "土地" + areaId;
            else if (areaType == AreanType.Woodenfloor)
                return "木製地闆" + areaId;
            else if (areaType == AreanType.Snowfield)
                return "雪地" + areaId;
            else if (areaType == AreanType.Slurry)
                return "泥地泥漿" + areaId;
            else if (areaType == AreanType.Metal)
                return "金屬地" + areaId;
            else if (areaType == AreanType.Macadam)
                return "碎石地" + areaId;
            else if (areaType == AreanType.Lawn)
                return "草地" + areaId;
            else if (areaType == AreanType.Cement)
                return "水泥地" + areaId;
            else if (areaType == AreanType.Mute)
                return "靜音" + areaId;
            else
            {
                return "Arean" + areaId;
            }

        }

    }

    public AreanType areaType;

    public int areaId = -1;

    // 區域的所有頂點
    public List<GameObject> points = new List<GameObject>();

    // 頂點的名字
    public List<string> pointNames = new List<string>();

    /// <summary>
    /// add new point
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public void AddPoint(GameObject point)
    {
        Debug.Log(point);
        if (point)
        {
            AddPoint(point, point.name);
        }
    }

    /// <summary>
    /// add new point with name
    /// </summary>
    /// <param name="point"></param>
    /// <param name="pointName"></param>
    /// <returns></returns>
    public void AddPoint(GameObject point, string pointName)
    {
        Debug.Log(points);
        if (point)
        {
            points.Add(point);
            pointNames.Add(pointName);
            LastPointNum++;
        }
    }

    public void InsertPoint(int preIndex, GameObject point)
    {
        if (point != null && preIndex >= 0 && preIndex < points.Count)
        {
            points.Insert(preIndex, point);
            pointNames.Insert(preIndex, point.name);
            LastPointNum++;
        }
    }

    public bool CheckLineCross()
    {
        //Debug.Log("TODO MORE.");
        Vector2 p1s, p1e, p2s, p2e;

        int iPointCount = points.Count;

        // 如果點的列表中只有兩個點, 可以添加新點.
        if (iPointCount <= 2)
        {
            return true;
        }

        for (int i = 0; i < iPointCount - 2; i++)
        {
            p1s.x = points[i].transform.position.x;
            p1s.y = points[i].transform.position.z;
            p1e.x = points[i + 1].transform.position.x;
            p1e.y = points[i + 1].transform.position.z;

            for (int j = i + 2; j < iPointCount; j++)
            {
                if (j != iPointCount - 1)
                {
                    p2s.x = points[j].transform.position.x;
                    p2s.y = points[j].transform.position.z;
                    p2e.x = points[j + 1].transform.position.x;
                    p2e.y = points[j + 1].transform.position.z;
                }
                else
                {
                    p2s.x = points[j].transform.position.x;
                    p2s.y = points[j].transform.position.z;
                    p2e.x = points[0].transform.position.x;
                    p2e.y = points[0].transform.position.z;

                    if (0 == i)
                    {
                        continue;
                    }
                }

                if (SGMath.CheckCross(p1s, p1e, p2s, p2e))
                {
                    return false;
                }

            }
        }
        return true;
    }

    /// <summary>
    /// delete current point
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public bool DelPoint(int index)
    {
        if (points.Count == 0)
            return false;
        if (index < points.Count)
        {
            if (points[index] != null)
                GameObject.DestroyImmediate(points[index]);

            points.RemoveAt(index);
            pointNames.RemoveAt(index);
            return true;
        }
        return false;
    }


    /// <summary>
    /// 獲得所有頂點的名字，用於顯示
    /// </summary>
    /// <returns></returns>
    public string[] GetAllPointNames()
    {
        return pointNames.ToArray();
    }
}

/// <summary>
/// 不可行走區域管理類
/// </summary>
public class UnWalkDataManager
{
    private const int ServerNavScale = 1;

    private static byte sUnWalkFlag = 1;
    private static byte sUnWalkFlag_Bound = 2;
    private static byte sUnWalkFlag_OutSide = 3;
    private static byte sFlag_RoomInside = 4;
    private static byte sFlag_RoomOutside = 5;
    private static byte sFlag_RoomOther = 6;
    private static byte sFlag_RoomWall = 7;
    private static byte sFlag_Water = 8;
    private static byte sFlag_Soil = 9;
    private static byte sFlag_Macadam = 10;
    private static byte sFlag_Lawn = 11;
    private static byte sFlag_Metal = 12;
    private static byte sFlag_Slurry = 13;
    private static byte sFlag_Snowfield = 14;
    private static byte sFlag_Cement = 15;
    private static byte sFlag_Woodenfloor = 16;
    private static byte sFlag_Mute = 18;


    public static Color unWalk_Color = SGMath.HexToColorEx(0xff0e4e65);
    public static Color unWalk_Bound_Color = Color.black;
    public static Color unWalk_OutSide_Color = Color.clear;
    public static Color walk_Color = SGMath.HexToColorEx(0xff279cc8);

    //服務器使用的世界格子
    public byte[,] dicObstruction;

    //客戶端用的顯示各自
    public byte[,] dicObstructionClient;

    public byte[,] dicObstructionHeight;

    public byte[,] dicWater;


    // file info
    private const string EditVersion = "REGION_EDIT_01";// "EditVersion_01";

    // data variables
    public List<UnWalkData> allAreas = new List<UnWalkData>();
    public List<string> areasName = new List<string>();

    //文件編號
    public List<int> mSaveDataNum = new List<int>();

    // save last area num
    public static int lastAreaNum = 0;

    public static int gridSizeX = 100;
    public static int gridSizeY = 100;

    public string[] GetAllAreasName()
    {
        return areasName.ToArray();
    }

    public string[] GetAllPointsName(int areaIndex)
    {
        string[] emptyName = { };
        if (allAreas == null)
            return emptyName;
        if (areaIndex >= allAreas.Count || allAreas.Count <= 0)
        {
            //LogManager.Log("區域索引[" + areaIndex + "]不對");
            return emptyName;
        }
        UnWalkData walkData = allAreas[areaIndex];
        return walkData.GetAllPointNames();
    }


    /// <summary>
    /// 添加新不可行走區域
    /// </summary>
    /// <returns></returns>
    public void AddNewArea(AreanType at)
    {
        //if (isObstruction)
        {
            AddNewArea(++lastAreaNum, at);
        }
    }

    public void AddNewArea(int areaId, AreanType at)
    {
        UnWalkData walkData = new UnWalkData();
        walkData.areaId = areaId;
        walkData.areaType = at;

        allAreas.Add(walkData);
        areasName.Add(walkData.AreaName);
    }

    /// <summary>
    /// 刪除不可行走區域
    /// </summary>
    /// <param name="index">區域索引</param>
    /// <returns></returns>
    public void DelArea(int index)
    {
        if (allAreas.Count <= 0)
            return;
        if (index <= allAreas.Count)
        {
            UnWalkData walkData = allAreas[index];

            // 需要刪除遊戲裏麵的所有頂點對象
            foreach (GameObject obj in walkData.points)
            {
                GameObject.DestroyImmediate(obj);
            }

            allAreas.RemoveAt(index);
            areasName.RemoveAt(index);
        }
    }

    /// <summary>
    /// 添加一個新的不可行走區域頂點
    /// </summary>
    /// <param name="areaIndex">區域索引</param>
    /// <param name="obj">頂點對象</param>
    /// <returns></returns>
    public void AddPoint(int areaIndex, GameObject obj)
    {
        if (areaIndex < allAreas.Count)
        {
            UnWalkData walkData = allAreas[areaIndex];
            string pointName = "區域" + (areaIndex + 1) + "->點" + (walkData.LastPointNum);
            obj.name = pointName;
            walkData.AddPoint(obj, pointName);
        }
        else
            Debug.Log("區域索引[" + areaIndex + "]不對，請選擇區域先");
    }

    public void InsertPoint(int areaIndex, int prePointIndex, GameObject obj)
    {
        if (areaIndex < allAreas.Count)
        {
            UnWalkData walkData = allAreas[areaIndex];
            string pointName = "區域" + (areaIndex + 1) + "->點" + (walkData.LastPointNum);
            obj.name = pointName;
            walkData.InsertPoint(prePointIndex, obj);
        }
        else
            Debug.Log("區域索引[" + areaIndex + "]不對，請選擇區域先");
    }

    /// <summary>
    /// 根據索引刪除頂點
    /// </summary>
    /// <param name="areaIndex">不可行走區域索引</param>
    /// <param name="pointIndex">頂點索引</param>
    /// <returns></returns>
    public void DelPoint(int areaIndex, int pointIndex)
    {
        if (areaIndex < allAreas.Count)
        {
            UnWalkData walkData = allAreas[areaIndex];
            if (walkData.points.Count <= 0)
            {
                Debug.Log("頂點已經全部刪除");
            }
            else if (pointIndex < walkData.points.Count)
            {
                walkData.DelPoint(pointIndex);
            }
            else
                Debug.LogError("頂點索引[" + pointIndex + "]不對，請選擇正確的頂點");
        }
        else
            Debug.LogError("區域索引[" + areaIndex + "]不對，請選擇區域先");
    }

    public void DelAllAreas()
    {
        int allNum = allAreas.Count;
        for (int i = 0; i < allNum; i++)
        {
            DelArea(0);
        }
    }

    public void CheckAllPoints()
    {
        foreach (UnWalkData area in allAreas)
        {
            int pointNum = area.points.Count;
            for (int i = 0; i < pointNum; i++)
            {
                if (area.points[i] == null)
                {
                    area.DelPoint(i);

                    i--;
                    pointNum--;
                }
            }
        }
    }

    /// <summary>
    /// save whole unwalkable area to file
    /// </summary>
    /// <param name="filePath">unwalkable file path</param>
    /// <returns></returns>
    public void SaveUnwalkArea(string filePath, string terrainParam, bool useClientGrid)
    {
        UTF8Encoding utf8 = new UTF8Encoding();

        if (!Directory.Exists(Path.GetDirectoryName(filePath)))
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

        FileStream fs = File.Create(filePath);
        BinaryWriter binWriter = new BinaryWriter(fs);

        // write version
        binWriter.Write(utf8.GetBytes(EditVersion));
        // write terrain size
        binWriter.Write(terrainParam);
        // write count
        binWriter.Write(allAreas.Count);

        foreach (UnWalkData walkData in allAreas)
        {
            //save id
            binWriter.Write(walkData.areaId);
            //為了和以前的格式保存一直，這裏需要存入一個區域等級的標記
            byte flag = (byte)walkData.areaType;
            binWriter.Write(flag);
            //save point count
            binWriter.Write(walkData.points.Count);

            foreach (GameObject point in walkData.points)
            {
                Vector3 pos = point.transform.position;
                //save x z info
                binWriter.Write(pos.x);
                binWriter.Write(pos.z);
                //為了和以前的格式保存一直，這裏需要存入y
                binWriter.Write(pos.y);
            }
        }

        if (useClientGrid)
        {
            binWriter.Write(1);
        }
        else
        {
            binWriter.Write(0);
        }

        binWriter.Close();
        fs.Close();

        Debug.Log("保存數據成功!");
    }

    //獲取場景的高度（包括地形和建築）
    float GetSceneHeight(float x, float z)
    {
        float SceneHeight = -1000.0f;
        Ray ray = new Ray();//構造射線
        ray.direction = -Vector3.up;
        ray.origin = new Vector3(x, 1000.0f, z);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity))//排除actor
        {
            SceneHeight = hitInfo.point.y;
        }
        return SceneHeight;
    }


    /// <summary>
    /// load unwalkable data
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public void LoadUnwalkArea(string filePath, GameObject parentPoint, out string terrainParam, out bool useClientGrid)
    {
        useClientGrid = false;
        terrainParam = "";
        if (parentPoint == null)
        {
            Debug.LogError("父節點不能為空");
            return;
        }
        float pointHeight = parentPoint.transform.position.y;
        // check file exist
        if (!File.Exists(filePath))
            return;

        DelAllAreas();

        // open file
        FileStream fs = File.Open(filePath, FileMode.Open);
        BinaryReader binReader = new BinaryReader(fs);

        try
        {
            // read version
            string currVersion = new string(binReader.ReadChars(EditVersion.Length));
            if (currVersion == EditVersion)
            {
                //read terrain size
                terrainParam = binReader.ReadString();
                // read areas count
                int areaCount = binReader.ReadInt32();

                for (int i = 0; i < areaCount; i++)
                {
                    // read id
                    int areaId = binReader.ReadInt32();
                    byte areaFlag = binReader.ReadByte();
                    AddNewArea(areaId, (AreanType)areaFlag);

                    //read point count
                    int pointCount = binReader.ReadInt32();
                    //Debug.Log(string.Format("areaid:{0},point:{1}",areaId,pointCount));
                    for (int j = 0; j < pointCount; j++)
                    {
                        // read pos
                        float x = binReader.ReadSingle();
                        //float z = binReader.ReadSingle();
                        float z = binReader.ReadSingle();
                        //為了和以前的格式保存一直，這裏需要y
                        float y = binReader.ReadSingle();

                        // auto generate point gameobject
                        GameObject point = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        float height = GetSceneHeight(x, z);
                        // point.transform.position = new UnityEngine.Vector3(x, height, z);
                        point.transform.position = new UnityEngine.Vector3(x, y, z);
                        point.transform.parent = parentPoint.transform;
                        point.transform.localScale /= 5;

                        AddPoint(allAreas.Count - 1, point);
                    }
                }

                int num = binReader.ReadInt32();
                if (num == 1)
                {
                    useClientGrid = true;
                }
            }
            else
            {
                Debug.LogError("version not match");
            }
        }
        catch (EndOfStreamException e)
        {
            Debug.LogError(e.Message);
        }
        finally
        {
            binReader.Close();
            fs.Close();
        }

        Debug.Log("加載數據成功!");
    }

    /// <summary>
    /// 讀取資料夾內，所有符合 場景名 +_+ 編號 +"_" + unWalkFileName 得文件，並將編號放入到 mSaveDataNum
    /// </summary>
    /// <param name="filePath">資料夾路徑</param>
    /// <param name="sceneName">場景名</param>
    public void LoadUnwalkAreaList(string filePath, string sceneName, string unWalkFileName)
    {
        mSaveDataNum.Clear();

        //找附檔名為unwalk的資料
        foreach (string fname in Directory.GetFiles(filePath, "*" + unWalkFileName))
        {
            // Debug.Log(fname.Contains(sceneName) ? fname : "");

            //確定他是否為現在這個場景
            if (fname.Contains(sceneName))
            {
                //找存檔是否有引號()
                if (fname.Contains("_"))
                {
                    string[] numArray = fname.Split('_');
                    // Debug.Log(numArray);
                    //抓最後一個底線，前面的數字，所以是numArray.Length - 2
                    string num = numArray[numArray.Length - 2];
                    mSaveDataNum.Add(Convert.ToInt16(num));
                    // Debug.Log(numArray);
                    // Debug.Log(num);
                    // mSaveDataNum.Add(num);
                }
            }
        }

        Debug.Log("加載Data名字成功!");
    }

    /// <summary>
    /// 返回文件編號陣列
    /// </summary>
    /// <returns></returns>
    public string[] GetAllDataNumber()
    {
        mSaveDataNum.Sort();
        int[] numberArray = mSaveDataNum.ToArray();
        string[] dataArray = new string[numberArray.Length];

        for (int i = 0; i < numberArray.Length; i++)
        {
            dataArray[i] = numberArray[i].ToString();
        }
        return dataArray;
    }

    public void ComputeNav(int width, int height, bool minimap, bool useClientGrid)
    {
        dicObstruction = new byte[width * ServerNavScale, height * ServerNavScale]; //間接指嚮，節省內存空間
        dicObstructionClient = new byte[width * ServerNavScale, height * ServerNavScale]; //間接指嚮，節省內存空間
        dicObstructionHeight = new byte[width * ServerNavScale, height * ServerNavScale]; //間接指嚮，節省內存空間

        List<Polygon> polys = new List<Polygon>();
        List<Polygon> Heightpolys = new List<Polygon>();
        foreach (UnWalkData walkData in allAreas)
        {
            if (walkData.areaType == AreanType.Obstruction)
            {
                Polygon poly = new Polygon();
                for (int i = 0; i < walkData.points.Count; i++)
                {
                    GameObject point = walkData.points[i];

                    Vector2 pos = new Vector2();
                    pos.x = point.transform.position.x;
                    pos.y = point.transform.position.z;
                    //pos *= ServerNavScale;

                    poly.allPoints.Add(pos);
                }
                polys.Add(poly);
                CalculateNavInfo(dicObstruction, poly, sUnWalkFlag, width, false, useClientGrid);
                CalculateNavInfo(dicObstructionClient, poly, sUnWalkFlag, width, false, true);
            }
            else if (walkData.areaType == AreanType.Boundary)
            {
                Polygon poly = new Polygon();
                for (int i = 0; i < walkData.points.Count; i++)
                {
                    GameObject point = walkData.points[i];

                    Vector2 pos = new Vector2();
                    pos.x = point.transform.position.x;
                    pos.y = point.transform.position.z;

                    poly.allPoints.Add(pos);
                }
                poly.CalcBoundaryGrid();
                polys.Add(poly);
                ClaculateBoundInfo(dicObstruction, poly);
            }
            else if (walkData.areaType == AreanType.Height)
            {
                Polygon poly = new Polygon();
                for (int i = 0; i < walkData.points.Count; i++)
                {
                    GameObject point = walkData.points[i];

                    Vector2 pos = new Vector2();
                    pos.x = point.transform.position.x;
                    pos.y = point.transform.position.z;
                    //pos *= ServerNavScale;

                    poly.allPoints.Add(pos);
                }
                Heightpolys.Add(poly);
                CalculateNavInfo(dicObstructionHeight, poly, sUnWalkFlag, width, false, true);
            }
        }
    }

    /// <summary>
    /// 輸出導航文件
    /// </summary>
    /// <param name="filePath">路徑</param>
    /// <param name="width">地圖寬</param>
    /// <param name="height">地圖高</param>
    /// <param name="minimap">是否繪製小地圖</param>
    /// <param name="useClientGrid">是否使用大阻擋</param>
    public void ExportNavForServer(string filePath, int width, int height, bool minimap, bool useClientGrid)
    {
        //新增陣列空間
        dicObstruction = new byte[width * ServerNavScale, height * ServerNavScale]; //間接指嚮，節首內存空間
        dicObstructionClient = new byte[width * ServerNavScale, height * ServerNavScale]; //間接指嚮，節首內存空間
        dicObstructionHeight = new byte[width * ServerNavScale, height * ServerNavScale]; //間接指嚮，節首內存空間

        List<Polygon> polys = new List<Polygon>();
        List<Polygon> Heightpolys = new List<Polygon>();

        //以Area區域當單位，進行計算
        foreach (UnWalkData walkData in allAreas)
        {
            //當Type為碰撞區時
            if (walkData.areaType == AreanType.Obstruction)
            {
                //將Area區域裡的 標記點(圓球) 放入Poly
                Polygon poly = new Polygon();
                for (int i = 0; i < walkData.points.Count; i++)
                {
                    GameObject point = walkData.points[i];

                    Vector2 pos = new Vector2();
                    pos.x = point.transform.position.x;
                    pos.y = point.transform.position.z;
                    //pos *= ServerNavScale;

                    poly.allPoints.Add(pos);
                }
                polys.Add(poly);

                //計算數據
                CalculateNavInfo(dicObstruction, poly, sUnWalkFlag, width, false, useClientGrid);
                CalculateNavInfo(dicObstructionClient, poly, sUnWalkFlag, width, false, true);
            }
            else if (walkData.areaType == AreanType.Boundary)
            {
                Polygon poly = new Polygon();
                for (int i = 0; i < walkData.points.Count; i++)
                {
                    GameObject point = walkData.points[i];

                    Vector2 pos = new Vector2();
                    pos.x = point.transform.position.x;
                    pos.y = point.transform.position.z;

                    poly.allPoints.Add(pos);
                }
                poly.CalcBoundaryGrid();
                polys.Add(poly);
                ClaculateBoundInfo(dicObstruction, poly);
            }
            else if (walkData.areaType == AreanType.Height)
            {
                Polygon poly = new Polygon();
                for (int i = 0; i < walkData.points.Count; i++)
                {
                    GameObject point = walkData.points[i];

                    Vector2 pos = new Vector2();
                    pos.x = point.transform.position.x;
                    pos.y = point.transform.position.z;
                    //pos *= ServerNavScale;

                    poly.allPoints.Add(pos);
                }
                Heightpolys.Add(poly);
                CalculateNavInfo(dicObstructionHeight, poly, sUnWalkFlag, width, false, true);
            }
        }

        //由於客戶端使用導航網格尋路，會貼邊走，所以服務器刪除拐點處的阻擋信息
        //foreach (Polygon poly in polys)
        //{
        //    for (int i = 0; i < poly.allPoints.Count; i++)
        //    {
        //        Vector2 ptStart = poly.allPoints[i];
        //        int x = (int)Math.Floor(ptStart.x / 2) * 2;
        //        int y = (int)Math.Floor(ptStart.y / 2) * 2;
        //        dicObstruction[y * width + x] = 0;
        //        if ((y + 1) * width + x + 1 >= 0)
        //            dicObstruction[(y + 1) * width + x + 1] = 0;
        //        if (y * width + x - 1 >= 0)
        //            dicObstruction[y * width + x - 1] = 0;
        //        if ((y - 1) * width + x >= 0)
        //            dicObstruction[(y - 1) * width + x] = 0;
        //    }
        //}

        //輸出小地圖
        if (minimap)
            ExportMinimap(dicObstruction, width, height);

        //Save文件
        SaveToNavMapFile(dicObstruction, dicObstructionClient, dicObstructionHeight, filePath, width, height, polys, Heightpolys, useClientGrid);
    }

    public void ExportMinimap(byte[,] _array, int width, int height)
    {
        Scene scene = SceneManager.GetActiveScene();

        Texture2D texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                var o = _array[x * ServerNavScale, y * ServerNavScale];
                var color = walk_Color;
                if (o == sUnWalkFlag)
                {
                    color = unWalk_Color;
                }
                else if (o == sUnWalkFlag_Bound)
                {
                    color = unWalk_OutSide_Color;//UnWalk_Bound_Color;
                }
                else if (o == sUnWalkFlag_OutSide)
                {
                    color = unWalk_OutSide_Color;
                }
                {
                    texture.SetPixel(x, y, color);
                }
            }
        }
        byte[] bytes = texture.EncodeToPNG();
        FileStream file = File.Open(Application.dataPath + "/MiniMap" + "/" + scene.name + ".png", FileMode.Create);
        BinaryWriter writer = new BinaryWriter(file);
        writer.Write(bytes);
        file.Close();
        Texture2D.DestroyImmediate(texture);
        texture = null;
    }

    /// <summary>
    /// 導出水域
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="useClientGrid"></param>
    public void ExportWaterForServer(string filePath, int width, int height, double deep)
    {
        dicWater = new byte[width * ServerNavScale, height * ServerNavScale]; //間接指嚮，節省內存空間
        foreach (UnWalkData walkData in allAreas)
        {
            if (walkData.areaType == AreanType.Water)
            {
                Polygon poly = new Polygon();
                for (int i = 0; i < walkData.points.Count; i++)
                {
                    GameObject point = walkData.points[i];

                    Vector2 pos = new Vector2();
                    pos.x = point.transform.position.x;
                    pos.y = point.transform.position.z;
                    //pos *= ServerNavScale;

                    poly.allPoints.Add(pos);
                }
                CalculateNavInfo(dicWater, poly, sFlag_Water, width);
            }
        }
        SaveWaterFile(dicWater, filePath, width, height, deep);
    }

    /// <summary>
    /// 導出草地
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public void ExportLawnForServer(string filePath, int width, int height)
    {
        var GMArean = new byte[width * ServerNavScale, height * ServerNavScale]; //間接指嚮，節省內存空間
        ExportGMTerrainForServer(GMArean, filePath, width, height, "Lawn", sFlag_Lawn, AreanType.Lawn);
    }

    /// <summary>
    /// 導出土地
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public void ExportSoilForServer(string filePath, int width, int height)
    {
        var GMArean = new byte[width * ServerNavScale, height * ServerNavScale]; //間接指嚮，節省內存空間
        ExportGMTerrainForServer(GMArean, filePath, width, height, "Soil", sFlag_Soil, AreanType.Soil);
    }

    /// <summary>
    /// 導出碎石地
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public void ExportMacadamForServer(string filePath, int width, int height)
    {
        var GMArean = new byte[width * ServerNavScale, height * ServerNavScale]; //間接指嚮，節省內存空間
        ExportGMTerrainForServer(GMArean, filePath, width, height, "Macadam", sFlag_Macadam, AreanType.Macadam);
    }

    /// <summary>
    /// 導出雪地
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public void ExportSnowFieldForServer(string filePath, int width, int height)
    {
        var GMArean = new byte[width * ServerNavScale, height * ServerNavScale]; //間接指嚮，節省內存空間
        ExportGMTerrainForServer(GMArean, filePath, width, height, "Snowfield", sFlag_Snowfield, AreanType.Snowfield);
    }

    /// <summary>
    /// 導出木製地闆
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public void ExportWoodenfloorForServer(string filePath, int width, int height)
    {
        var GMArean = new byte[width * ServerNavScale, height * ServerNavScale]; //間接指嚮，節省內存空間
        ExportGMTerrainForServer(GMArean, filePath, width, height, "Woodenfloor", sFlag_Woodenfloor, AreanType.Woodenfloor);
    }

    /// <summary>
    /// 導出泥地泥漿
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public void ExportslurryForServer(string filePath, int width, int height)
    {
        var GMArean = new byte[width * ServerNavScale, height * ServerNavScale]; //間接指嚮，節省內存空間
        ExportGMTerrainForServer(GMArean, filePath, width, height, "Slurry", sFlag_Slurry, AreanType.Slurry);
    }

    /// <summary>
    /// 導出金屬
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public void ExportMetalForServer(string filePath, int width, int height)
    {
        var GMArean = new byte[width * ServerNavScale, height * ServerNavScale]; //間接指嚮，節省內存空間
        ExportGMTerrainForServer(GMArean, filePath, width, height, "Metal", sFlag_Metal, AreanType.Metal);
    }

    /// <summary>
    /// 導出水泥地
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public void ExportCementForServer(string filePath, int width, int height)
    {
        var GMArean = new byte[width * ServerNavScale, height * ServerNavScale]; //間接指嚮，節省內存空間
        ExportGMTerrainForServer(GMArean, filePath, width, height, "Cement", sFlag_Cement, AreanType.Cement);
    }

    public void ExportMuteForClient(string filePath, int width, int height)
    {
        var GMArean = new byte[width * ServerNavScale, height * ServerNavScale]; //間接指嚮，節省內存空間
        ExportGMTerrainForServer(GMArean, filePath, width, height, "Mute", sFlag_Mute, AreanType.Mute);
    }

    /// <summary>
    /// 導出通用地形
    /// </summary>
    /// <param name="_array"></param>
    /// <param name="filePath"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="terrainName"></param>
    /// <param name="flag"></param>
    /// <param name="type"></param>
    public void ExportGMTerrainForServer(byte[,] _array, string filePath, int width, int height, string terrainName, byte flag, AreanType type)
    {
        foreach (UnWalkData walkData in allAreas)
        {
            if (walkData.areaType == type)
            {
                Polygon poly = new Polygon();
                for (int i = 0; i < walkData.points.Count; i++)
                {
                    GameObject point = walkData.points[i];

                    Vector2 pos = new Vector2();
                    pos.x = point.transform.position.x;
                    pos.y = point.transform.position.z;
                    poly.allPoints.Add(pos);
                }
                poly.CalcBoundaryGrid();
                ClaculateBoundInfo(_array, poly, flag);
                CalculateNavInfo(_array, poly, flag, width);

            }
        }
        SaveGMTerrainFile(_array, filePath, width, height, terrainName, flag);
    }

    /// <summary>
    /// 導出安全區
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public void ExportSafeRegionForServer(string filePath, int width, int height, bool useClientGrid)
    {
        var safeArray = new byte[width * ServerNavScale, height * ServerNavScale]; //間接指嚮，節省內存空間
        var safeArrayClient = new byte[width * ServerNavScale, height * ServerNavScale]; //間接指嚮，節省內存空間

        List<Polygon> polys = new List<Polygon>();
        foreach (UnWalkData walkData in allAreas)
        {
            if (walkData.areaType == AreanType.Safe)
            {
                Polygon poly = new Polygon();
                for (int i = 0; i < walkData.points.Count; i++)
                {
                    GameObject point = walkData.points[i];

                    Vector2 pos = new Vector2();
                    pos.x = point.transform.position.x;
                    pos.y = point.transform.position.z;
                    //pos *= ServerNavScale;

                    poly.allPoints.Add(pos);
                }
                polys.Add(poly);
                CalculateNavInfo(safeArray, poly, sUnWalkFlag, width);
                CalculateNavInfo(safeArrayClient, poly, sUnWalkFlag, width, false, true);
            }
        }
        SaveToNavMapFile(safeArray, safeArrayClient, null, filePath, width, height, polys, null, useClientGrid);

    }

    public void ExportRoomRegionForServer(string filePath, int width, int height)
    {
        var roomRegion = new byte[width * ServerNavScale, height * ServerNavScale]; //間接指嚮，節省內存空間

        List<Polygon> polys = new List<Polygon>();
        foreach (UnWalkData walkData in allAreas)
        {
            byte flag = byte.MaxValue;
            if (walkData.areaType == AreanType.Room_inside)
            {
                flag = sFlag_RoomInside;
            }
            else if (walkData.areaType == AreanType.Room_other)
            {
                flag = sFlag_RoomOther;
            }
            else if (walkData.areaType == AreanType.Room_outside)
            {
                flag = sFlag_RoomOutside;
            }
            else if (walkData.areaType == AreanType.Room_wall)
            {
                flag = sFlag_RoomWall;
            }

            if (flag != byte.MaxValue)
            {
                Polygon poly = new Polygon();
                for (int i = 0; i < walkData.points.Count; i++)
                {
                    GameObject point = walkData.points[i];

                    Vector2 pos = new Vector2();
                    pos.x = point.transform.position.x;
                    pos.y = point.transform.position.z;

                    poly.allPoints.Add(pos);
                }
                polys.Add(poly);
                CalculateNavInfo(roomRegion, poly, flag, width);
            }
        }
        SaveRoomSideFile(roomRegion, filePath, width, height);
    }

    /// <summary>
    /// 導出區域信息
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public void ExportRegionForServer(string filePath, int width, int height)
    {

        XmlDocument xmlDoc = new XmlDocument();
        XmlNode node = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", "");
        xmlDoc.AppendChild(node);
        XmlElement root = xmlDoc.CreateElement("root");
        xmlDoc.AppendChild(root);

        //保存路徑
        xmlDoc.Save(filePath);
        Debug.Log("保存" + filePath + "成功!");
    }
    public void ExportMaskForServer(string filePath, int width, int height)
    {
        XmlDocument xmlDoc = new XmlDocument();
        XmlNode node = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", "");
        xmlDoc.AppendChild(node);
        XmlElement root = xmlDoc.CreateElement("Map");
        xmlDoc.AppendChild(root);

        var w = width * 100;
        var h = height * 100;
        var size = (int)100 / ServerNavScale;
        root.SetAttribute("Width", w.ToString());
        root.SetAttribute("Height", h.ToString());
        root.SetAttribute("SectWith", w.ToString());
        root.SetAttribute("SectHeight", w.ToString());

        XmlElement image = xmlDoc.CreateElement("Images");
        root.AppendChild(image);

        XmlElement section = xmlDoc.CreateElement("Sections");
        root.AppendChild(section);

        //保存路徑
        xmlDoc.Save(filePath);
        Debug.Log("保存" + filePath + "成功!");
    }
    private void SaveToNavMapFile(byte[,] _array, byte[,] _arrayClient, byte[,] _arrayHeight, string filePath, int width, int height, List<Polygon> polys, List<Polygon> Heightpolys, bool useClientGrid)
    {
        XmlDocument xmlDoc = new XmlDocument();
        XmlNode node = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", "");
        xmlDoc.AppendChild(node);
        XmlElement root = xmlDoc.CreateElement("Item");
        xmlDoc.AppendChild(root);

        var w = width * 100;
        var h = height * 100;
        var size = (int)100 / ServerNavScale;
        root.SetAttribute("SceneName", SceneManager.GetActiveScene().name);
        root.SetAttribute("ID", "Obstruction");
        root.SetAttribute("MapWidth", w.ToString());
        root.SetAttribute("MapHeight", h.ToString());
        root.SetAttribute("NodeSize", size.ToString());

        string values = "";
        for (int y = 0; y < _array.GetUpperBound(1); y++)
        {
            for (int x = 0; x < _array.GetUpperBound(0); x++)
            {
                if (_array[x, y] != 0)
                {
                    if (string.IsNullOrEmpty(values))
                    {
                        values = string.Format("{0}_{1}", x, y);

                    }
                    else
                    {
                        values += string.Format(",{0}_{1}", x, y);
                    }
                }
            }
        }
        //if (!string.IsNullOrEmpty(values))
        {
            root.SetAttribute("Value", values);
        }

        if (!useClientGrid)
        {
            string values2 = "";
            for (int y = 0; y < _arrayClient.GetUpperBound(1); y++)
            {
                for (int x = 0; x < _arrayClient.GetUpperBound(0); x++)
                {
                    if (_arrayClient[x, y] != 0)
                    {
                        if (string.IsNullOrEmpty(values2))
                        {
                            values2 = string.Format("{0}_{1}", x, y);

                        }
                        else
                        {
                            values2 += string.Format(",{0}_{1}", x, y);
                        }
                    }
                }
            }
            //if (!string.IsNullOrEmpty(values))
            {
                root.SetAttribute("ValueClient", values2);
            }
        }

        if (_arrayHeight != null)
        {
            string values2 = "";
            for (int y = 0; y < _arrayHeight.GetUpperBound(1); y++)
            {
                for (int x = 0; x < _arrayHeight.GetUpperBound(0); x++)
                {
                    if (_arrayHeight[x, y] != 0)
                    {
                        if (string.IsNullOrEmpty(values2))
                        {
                            values2 = string.Format("{0}_{1}", x, y);

                        }
                        else
                        {
                            values2 += string.Format(",{0}_{1}", x, y);
                        }
                    }
                }
            }
            //if (!string.IsNullOrEmpty(values))
            {
                root.SetAttribute("ValueHeight", values2);
            }
        }

        //存阻擋區域
        string polystr = "";
        if (!useClientGrid)
        {
            foreach (var poly in polys)
            {
                if (!string.IsNullOrEmpty(polystr))
                {
                    polystr += ",";
                }
                //foreach (var pt in poly.allPoints)
                for (int i = 0; i < poly.allPoints.Count; i++)
                {
                    var pt = poly.allPoints[i];
                    if (i == poly.allPoints.Count - 1)
                    {
                        polystr += string.Format("{0}_{1}", pt.x, pt.y);
                    }
                    else
                    {
                        polystr += string.Format("{0}_{1}_", pt.x, pt.y);
                    }
                }
            }
        }

        root.SetAttribute("Polys", polystr);

        polystr = "";
        if (Heightpolys != null)
        {
            foreach (var poly in Heightpolys)
            {
                if (!string.IsNullOrEmpty(polystr))
                {
                    polystr += ",";
                }
                //foreach (var pt in poly.allPoints)
                for (int i = 0; i < poly.allPoints.Count; i++)
                {
                    var pt = poly.allPoints[i];
                    if (i == poly.allPoints.Count - 1)
                    {
                        polystr += string.Format("{0}_{1}", pt.x, pt.y);
                    }
                    else
                    {
                        polystr += string.Format("{0}_{1}_", pt.x, pt.y);
                    }
                }
            }
        }

        root.SetAttribute("HeightPolys", polystr);


        //保存路徑
        xmlDoc.Save(filePath);
        Debug.Log("保存" + filePath + "成功!");
    }

    /// <summary>
    /// 保存室內室外區域
    /// </summary>
    /// <param name="_array"></param>
    /// <param name="filePath"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    private void SaveRoomSideFile(byte[,] _array, string filePath, int width, int height)
    {
        XmlDocument xmlDoc = new XmlDocument();
        XmlNode node = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", "");
        xmlDoc.AppendChild(node);

        XmlElement root = xmlDoc.CreateElement("Item");
        xmlDoc.AppendChild(root);

        var w = width * 100;
        var h = height * 100;
        var size = (int)100 / ServerNavScale;
        root.SetAttribute("ID", "Room");
        root.SetAttribute("MapWidth", w.ToString());
        root.SetAttribute("MapHeight", h.ToString());
        root.SetAttribute("NodeSize", size.ToString());

        string values_outside = "";
        string values_inside = "";
        string values_other = "";
        string values_wall = "";
        for (int y = 0; y < _array.GetUpperBound(1); y++)
        {
            for (int x = 0; x < _array.GetUpperBound(0); x++)
            {
                if (_array[x, y] == sFlag_RoomInside)
                {
                    if (string.IsNullOrEmpty(values_inside))
                        values_inside = string.Format("{0}_{1}", x, y);
                    else
                        values_inside += string.Format(",{0}_{1}", x, y);
                }
                else if (_array[x, y] == sFlag_RoomOther)
                {
                    if (string.IsNullOrEmpty(values_other))
                        values_other = string.Format("{0}_{1}", x, y);
                    else
                        values_other += string.Format(",{0}_{1}", x, y);
                }
                else if (_array[x, y] == sFlag_RoomOutside)
                {
                    if (string.IsNullOrEmpty(values_outside))
                        values_outside = string.Format("{0}_{1}", x, y);
                    else
                        values_outside += string.Format(",{0}_{1}", x, y);
                }
                else if (_array[x, y] == sFlag_RoomWall)
                {
                    if (string.IsNullOrEmpty(values_wall))
                        values_wall = string.Format("{0}_{1}", x, y);
                    else
                        values_wall += string.Format(",{0}_{1}", x, y);
                }
            }
        }
        root.SetAttribute("ValueInside", values_inside);
        root.SetAttribute("ValueOutside", values_outside);
        root.SetAttribute("ValueOther", values_other);
        root.SetAttribute("ValueWall", values_wall);

        xmlDoc.Save(filePath);
        Debug.Log("保存" + filePath + "成功!");
    }
    private void SaveWaterFile(byte[,] _array, string filePath, int width, int height, double deep)
    {
        XmlDocument xmlDoc = new XmlDocument();
        XmlNode node = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", "");
        xmlDoc.AppendChild(node);

        XmlElement root = xmlDoc.CreateElement("Item");
        xmlDoc.AppendChild(root);

        var w = width * 100;
        var h = height * 100;
        var d = deep * 100;
        var size = (int)100 / ServerNavScale;
        root.SetAttribute("ID", "Water");
        root.SetAttribute("Deep", d.ToString());
        root.SetAttribute("MapWidth", w.ToString());
        root.SetAttribute("MapHeight", h.ToString());
        root.SetAttribute("NodeSize", size.ToString());

        string values = "";
        for (int y = 0; y < _array.GetUpperBound(1); y++)
        {
            for (int x = 0; x < _array.GetUpperBound(0); x++)
            {
                if (_array[x, y] == sFlag_Water)
                {
                    if (string.IsNullOrEmpty(values))
                        values = string.Format("{0}_{1}", x, y);
                    else
                        values += string.Format(",{0}_{1}", x, y);
                }
            }
        }
        root.SetAttribute("Value", values);

        xmlDoc.Save(filePath);
        Debug.Log("保存" + filePath + "成功!");
    }


    /// <summary>
    /// 保存通用地形數據
    /// </summary>
    /// <param name="_array"></param>
    /// <param name="filePath"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    private void SaveGMTerrainFile(byte[,] _array, string filePath, int width, int height, string terrainName, byte flag)
    {
        XmlDocument xmlDoc = new XmlDocument();
        XmlNode node = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", "");
        xmlDoc.AppendChild(node);

        XmlElement root = xmlDoc.CreateElement("Item");
        xmlDoc.AppendChild(root);

        var w = width * 100;
        var h = height * 100;
        var size = (int)100 / ServerNavScale;
        root.SetAttribute("ID", terrainName);
        root.SetAttribute("MapWidth", w.ToString());
        root.SetAttribute("MapHeight", h.ToString());
        root.SetAttribute("NodeSize", size.ToString());

        string values = "";
        for (int y = 0; y < _array.GetUpperBound(1); y++)
        {
            for (int x = 0; x < _array.GetUpperBound(0); x++)
            {
                if (_array[x, y] == flag)
                {
                    if (string.IsNullOrEmpty(values))
                        values = string.Format("{0}_{1}", x, y);
                    else
                        values += string.Format(",{0}_{1}", x, y);
                }
            }
        }
        root.SetAttribute("Value", values);

        xmlDoc.Save(filePath);
        Debug.Log("保存" + filePath + "成功!");
    }


    private void ClaculateBoundInfo(byte[,] _array, Polygon poly)
    {
        var target = Vector2.zero;
        var target1 = Vector2.zero;
        var target2 = Vector2.zero;
        var target3 = Vector2.zero;
        var center = Vector2.zero;

        int width = _array.GetUpperBound(0);
        int height = _array.GetUpperBound(1);
        for (int y = 0; y < height; y++)
        {
            target.y = y;
            target1.y = y + 1;
            target2.y = y;
            target3.y = y + 1;
            center.y = y + 0.5f;

            for (int x = 0; x < width; x++)
            {

                target.x = x;
                target1.x = x;
                target2.x = x + 1;
                target3.x = x + 1;
                center.x = x + 0.5f;
                if (poly.IsOnLine(target))
                {
                    for (int scalex = 0; scalex < ServerNavScale; scalex++)
                    {
                        for (int scaley = 0; scaley < ServerNavScale; scaley++)
                        {
                            int xIndex = x * ServerNavScale + scalex;
                            int yIndex = y * ServerNavScale + scaley;
                            if (xIndex < width && yIndex < height)
                                _array[xIndex, yIndex] = sUnWalkFlag_Bound;
                        }
                    }
                }
                else if (poly.IsPointIn(target) || poly.IsPointIn(target1)
                    || poly.IsPointIn(target2) || poly.IsPointIn(target3)
                    || poly.IsPointIn(center))
                {

                }
                else
                {
                    //不在邊界內
                    for (int scalex = 0; scalex < ServerNavScale; scalex++)
                    {
                        for (int scaley = 0; scaley < ServerNavScale; scaley++)
                        {
                            int xIndex = x * ServerNavScale + scalex;
                            int yIndex = y * ServerNavScale + scaley;
                            if (xIndex < width && yIndex < height)
                                _array[xIndex, yIndex] = sUnWalkFlag_OutSide;
                        }
                    }
                }
            }
        }
    }

    private void ClaculateBoundInfo(byte[,] _array, Polygon poly, byte UnWalkFlag)
    {
        var target = Vector2.zero;
        var target1 = Vector2.zero;
        var target2 = Vector2.zero;
        var target3 = Vector2.zero;
        var center = Vector2.zero;

        int width = _array.GetUpperBound(0);
        int height = _array.GetUpperBound(1);
        for (int y = 0; y < height; y++)
        {
            target.y = y;
            target1.y = y + 1;
            target2.y = y;
            target3.y = y + 1;
            center.y = y + 0.5f;

            for (int x = 0; x < width; x++)
            {

                target.x = x;
                target1.x = x;
                target2.x = x + 1;
                target3.x = x + 1;
                center.x = x + 0.5f;
                if (poly.IsOnLine(target))
                {
                    for (int scalex = 0; scalex < ServerNavScale; scalex++)
                    {
                        for (int scaley = 0; scaley < ServerNavScale; scaley++)
                        {
                            int xIndex = x * ServerNavScale + scalex;
                            int yIndex = y * ServerNavScale + scaley;
                            if (xIndex < width && yIndex < height)
                                _array[xIndex, yIndex] = UnWalkFlag;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 計算碰撞區塊
    /// </summary>
    private void CalculateNavInfo(byte[,] _array, Polygon poly, byte UnWalkFlag, int width, bool reverse = false, bool bClient = false)
    {
        //拿取外接矩形
        Rect rect = poly.GetCoverRect();

        var target = Vector2.zero;  //矩形左上
        var target1 = Vector2.zero; //矩形右上
        var target2 = Vector2.zero; //矩形左下
        var target3 = Vector2.zero; //矩形右下
        var center = Vector2.zero;  //矩形中心

        //計算Poly裡的碰撞矩形
        for (int y = (int)rect.yMin; y <= Math.Ceiling(rect.yMax); y++)
        {
            target.y = y;
            target1.y = y + 1;
            target2.y = y;
            target3.y = y + 1;
            center.y = y + 0.5f;
            for (int x = (int)rect.xMin; x <= Math.Ceiling(rect.xMax); x++)
            {
                target.x = x;
                target1.x = x;
                target2.x = x + 1;
                target3.x = x + 1;
                center.x = x + 0.5f;

                try
                {
                    //檢查point是否在poly裡面
                    if (bClient)
                    {
                        //使用大阻擋
                        //優化算法，增加判定線段是否相交
                        if (poly.IsPointIn(target) || poly.IsPointIn(target1)
                            || poly.IsPointIn(target2) || poly.IsPointIn(target3)
                        || poly.IsPointIn(center) || poly.IsCross(target, target1) || poly.IsCross(target1, target2) || poly.IsCross(target2, target3) || poly.IsCross(target, target3))
                        {
                            //if (reverse == false)
                            {
                                for (int scalex = 0; scalex < ServerNavScale; scalex++)
                                {
                                    for (int scaley = 0; scaley < ServerNavScale; scaley++)
                                    {
                                        _array[x * ServerNavScale + scalex, y * ServerNavScale + scaley] = UnWalkFlag;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (poly.IsPointIn(target) && poly.IsPointIn(target1)
                            && poly.IsPointIn(target2) && poly.IsPointIn(target3))
                        //|| poly.isPointIn(center))
                        {
                            //if (reverse == false)
                            {
                                //放入陣列，因有放大所以要等比放大
                                for (int scalex = 0; scalex < ServerNavScale; scalex++)
                                {
                                    for (int scaley = 0; scaley < ServerNavScale; scaley++)
                                    {
                                        _array[x * ServerNavScale + scalex, y * ServerNavScale + scaley] = UnWalkFlag;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (IndexOutOfRangeException e)
                {
                    Debug.LogError("請檢查點是否有 \"超出地圖長寬\" 或是 \"點的數據為負數\" ");
                    Debug.LogError(e);
                }
                catch (Exception e)
                {
                    Debug.LogError("請通知程式人員 : " + e);
                }

            }
        }
    }

}