using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;
using System.Xml.Linq;
using UnityEngine.SceneManagement;
using Core.Framework.Res;

namespace Core.Framework.Map
{

    /// <summary>
    /// Author: Peter
    /// Date : 2021/05/07
    /// Desc : 地圖障礙物管理器，
    /// 用法是，設定MapCode後再呼叫NextPosInMap就能拿到下一個位置點
    /// 
    /// 2021/05/10 新增 MapCode屬性
    /// 2021/05/13 新增 ServerNavScale變數，修正資料中所有/2地方
    /// </summary>
    public class MapDataManager : Singleton<MapDataManager>
    {
        //代號靈中地圖資料 參數
        private const int ServerNavScale = 1;

        //地圖編號
        private int mMapCode = 0;

        //用於向外部展示，現在地圖編號，但設定地圖編號只能由SetGMapData()執行
        public int MapCode { get { return mMapCode; } }

        //代號靈地圖數字換算  
        private const float mMapConvert = 100.0F;

        //代號靈的GMapData資料
        public GMapData currentMapData = new GMapData();

        class CheckLeaderMove
        {
            //上一次移動時，角色的原點
            public Vector2 LastCheckFrom;
            //上一次移動時，角色移動的目標點
            public Vector2 LastCheckTo;
            //上一次移動，腳色最終要移動的點
            public Vector2 LastCheckResult;
            public bool LastbCheckRolePos;
        }
        //上一次移動的計算結果
        private List<CheckLeaderMove> mCheckLeaderMoveList = new List<CheckLeaderMove>();

        private List<int> mCheckLeaderMoveListx = new List<int>() { 0, 3, -3 };
        private List<int> mCheckLeaderMoveListy = new List<int>() { 0, 3, -3 };
        private List<Vector2> mCheckLeaderMoveListpt = new List<Vector2>();
        private List<int> mCheckpolysList = new List<int>();

        /// <summary>
        /// 該方向移動的下一個點位置 
        /// </summary>
        /// <param name="posX">當前 X 位置</param>
        /// <param name="posY">當前 Y 位置</param>
        /// <param name="angle">要前往的方向(以Z箭頭為0，順時針計算，要正數)</param>
        /// <param name="moveDistance">往前的距離，預設50 = 0.5 unit</param>
        /// <returns>返回Vector3 (nextX,0,nextY)</returns>
        public Vector3 NextPosInMap(float posX, float posY, float angle, float moveDistance = 50)
        {
            int nextX = 0;
            int nextY = 0;
            WalkNextPos(
                (int)(posX * mMapConvert),
                 (int)(posY * mMapConvert),
                 angle, out nextX, out nextY, moveDistance);

            return new Vector3(
                nextX / mMapConvert, 0,
                 nextY / mMapConvert);
        }

        /// <summary>
        /// 設置需要的 Map 的障礙物資料 TODO:因沒有數字Map表，暫時放著，待修正
        /// </summary>
        /// <param name="mapCode">地圖編號</param>
        public void SetGMapData(int mapCode)
        {
            currentMapData = new GMapData();//重製GMapData
            this.mMapCode = mapCode;        //設置編號
            currentMapData.mapCode = mapCode;//設置GMapData編號

            //設置Obs資料 
            ResourceManager.Instance.LoadTextAssetAsync(
               "Assets/PublicAssets/MapConfig/" + mMapCode + "/obs.xml",
               (obsXml) =>
               {
                   LoadObstruction(obsXml.ToString());
                   Debug.Log(obsXml);
               });
        }

        /// <summary>
        /// (美術測試用)設置需要的 Map 的障礙物資料
        /// </summary>
        public void TestSetGMapData()
        {
            currentMapData = new GMapData();//重製GMapData

            //設置Obs資料  (美術用，直接讀到美術文件目錄的設定資料)
            LoadObstruction(
                XDocument.Load(
                    Application.dataPath + "/Arts/Map/MapConfig/" +
                     SceneManager.GetActiveScene().name + "Obs.xml").ToString());
            Debug.Log(currentMapData);

        }


        /// <summary>
        /// 讀取 Obs.Xml 地圖障礙物區資料 (代號靈 未修正演算)
        /// </summary>
        /// <param name="obsData">obs.xml</param>
        private void LoadObstruction(string obsData)
        {
            //XML轉換
            System.Xml.Linq.XElement xml = XElement.Parse(obsData);

            //設定地圖長寬
            currentMapData.mapWidth = (int)xml.Attribute("MapWidth");
            currentMapData.mapHeight = (int)xml.Attribute("MapHeight");

            //設定地圖一塊大小
            currentMapData.gridSizeX = 100;
            currentMapData.gridSizeY = 100;

            //新增地塊內容容器
            // CurrentMapData.MapGrid = new MapGrid(mMapCode, CurrentMapData.MapWidth, CurrentMapData.MapHeight, CurrentMapData.GridSizeX, CurrentMapData.GridSizeY, CurrentMapData);

            //設定地圖資料，採2的次方為準，例:地圖10,10 => 16,16
            currentMapData.polys = new List<PolyData>();
            int widthGridsNum = (currentMapData.mapWidth - 1) / currentMapData.gridSizeX + 1;
            int heightGridsNum = (currentMapData.mapHeight - 1) / currentMapData.gridSizeY + 1;
            widthGridsNum = (int)Math.Ceiling(Math.Log(widthGridsNum, 2));
            widthGridsNum = (int)Math.Pow(2, widthGridsNum);
            heightGridsNum = (int)Math.Ceiling(Math.Log(heightGridsNum, 2));
            heightGridsNum = (int)Math.Pow(2, heightGridsNum);
            currentMapData.gridSizeXNum = widthGridsNum;
            currentMapData.gridSizeYNum = heightGridsNum;

            //初始化地圖陣列資料 
            currentMapData.fixedObstruction = new byte[widthGridsNum, heightGridsNum];
            for (int y = 0; y < currentMapData.fixedObstruction.GetUpperBound(1); y++)
            {
                for (int x = 0; x < currentMapData.fixedObstruction.GetUpperBound(0); x++)
                {
                    currentMapData.fixedObstruction[x, y] = 1;
                }
            }

            //XML "Value" 輸入
            string xmlValue = (string)xml.Attribute("Value");

            if (xmlValue != "" && xmlValue != null)
            {
                //設定 currentMapData.fixedObstruction 哪格不能走
                string[] obstruction = xmlValue.Split(',');
                for (int i = 0; i < obstruction.Count(); i++)
                {
                    if (obstruction[i].Trim() == "") continue;
                    string[] obstructionXY = obstruction[i].Split('_');
                    int xIndex = Convert.ToInt32(obstructionXY[0]) / ServerNavScale/* / 2*/;
                    int yIndex = Convert.ToInt32(obstructionXY[1]) / ServerNavScale /* / 2*/;
                    if (xIndex < 0 || xIndex >= widthGridsNum || yIndex < 0 || yIndex >= heightGridsNum)
                    {
                        continue;
                    }
                    try
                    {
                        currentMapData.fixedObstruction[xIndex, yIndex] = 0;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                    }
                }
            }

            //XML "ValueClient" 輸入
            string xmlClient = (string)xml.Attribute("ValueClient");
            if (xmlClient != "" && xmlClient != null)
            {
                //設定  currentMapData.fixedObstructionClient 哪格不能走
                int len1 = xmlValue.Length;
                int len2 = xmlClient.Length;
                currentMapData.fixedObstructionClient = currentMapData.fixedObstruction.Clone() as byte[,];
                string[] obstruction = xmlClient.Split(',');
                for (int i = 0; i < obstruction.Count(); i++)
                {
                    if (obstruction[i].Trim() == "") continue;
                    string[] obstructionXY = obstruction[i].Split('_');
                    int xIndex = Convert.ToInt32(obstructionXY[0]) / ServerNavScale /* / 2*/;
                    int yIndex = Convert.ToInt32(obstructionXY[1]) / ServerNavScale /* / 2*/;
                    if (xIndex < 0 || xIndex >= widthGridsNum || yIndex < 0 || yIndex >= heightGridsNum)
                    {
                        continue;
                    }
                    try
                    {
                        currentMapData.fixedObstructionClient[xIndex, yIndex] = 0;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                    }
                }
            }


            string xmlHeight = (string)xml.Attribute("ValueHeight");
            if (xmlHeight != "" && xmlHeight != null)
            {
                int len1 = xmlValue.Length;
                int len2 = xmlHeight.Length;
                if (currentMapData.fixedObstructionClient != null)
                    currentMapData.fixedObstructionHeight = currentMapData.fixedObstructionClient.Clone() as byte[,];
                else
                    currentMapData.fixedObstructionHeight = currentMapData.fixedObstruction.Clone() as byte[,];
                string[] obstruction = xmlHeight.Split(',');
                for (int i = 0; i < obstruction.Count(); i++)
                {
                    if (obstruction[i].Trim() == "") continue;
                    string[] obstructionXY = obstruction[i].Split('_');
                    int xIndex = Convert.ToInt32(obstructionXY[0]) / ServerNavScale /* / 2*/;
                    int yIndex = Convert.ToInt32(obstructionXY[1]) / ServerNavScale /* / 2*/;
                    if (xIndex < 0 || xIndex >= widthGridsNum || yIndex < 0 || yIndex >= heightGridsNum)
                    {
                        continue;
                    }
                    try
                    {
                        currentMapData.fixedObstructionHeight[xIndex, yIndex] = 0;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                    }
                }
            }

            //XML "Polys" 輸入
            string xmlPoly = (string)xml.Attribute("Polys");
            if (xmlPoly != "" && xmlPoly != null)
            {
                float groundy = 1.05f;
                string[] obstruction = xmlPoly.Split(',');
                for (int i = 0; i < obstruction.Count(); i++)
                {
                    if (obstruction[i].Trim() == "") continue;
                    string[] obstructionXY = obstruction[i].Split('_');
                    Vector3[] vertices = new Vector3[obstructionXY.Length / 2];
                    int index = 0;
                    PolyData polyData = new PolyData();
                    for (int j = 0; j < obstructionXY.Length; j += 2)
                    {
                        //添加Poly數據
                        polyData.ploy.allPoints.Add(
                            new Vector2((int)(Convert.ToDouble(obstructionXY[j]) * 100),
                             (int)(Convert.ToDouble(obstructionXY[j + 1]) * 100)));

                        //無用數據
                        vertices[index++] = new Vector3((float)Convert.ToDouble(obstructionXY[j]), groundy, (float)Convert.ToDouble(obstructionXY[j + 1]));
                    }
                    //設定poly形狀
                    currentMapData.polys.Add(polyData);
                }
            }
            Debug.Log("Load OBS OK");


            //TODO:測試用生成 球 帶刪除
            #region 
            /*
            GameObject newParent = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            for (int x = 0; x < currentMapData.fixedObstruction.GetLength(0); x++)
            {
                for (int y = 0; y < currentMapData.fixedObstruction.GetUpperBound(0) + 1; y++)
                {
                    if (currentMapData.fixedObstruction[x, y] == 0)
                    {
                        GameObject newG = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        newG.name = "PosX " + x + " " + "PosY " + y;
                        newG.transform.position = new Vector3(x, 0, y);
                        newG.transform.parent = newParent.transform;
                        // newG.transform.localScale /= 5;
                        newG.GetComponent<MeshRenderer>().material.color = Color.red;
                    }
                }
            }*/
            #endregion
        }


        //代號靈原版演算-----------------

        /// <summary>
        /// 代號靈原版演算
        /// 取得前進方向上，下一個點的位置
        /// </summary>
        /// <param name="currX">原座標點X (乘100的)</param>
        /// <param name="currY">原座標點Y (乘100的)</param>
        /// <param name="dir">要前往的方向(以Z箭頭為0，順時針計算，要正數)</param>
        /// <param name="nextX">計算後的下一點位置X (乘100的)</param>
        /// <param name="nextY">計算後的下一點位置Y (乘100的)</param>
        /// <param name="moveDistance">往前的距離，預設50 = 0.5 unit</param>
        private void WalkNextPos(int currX, int currY, float dir, out int nextX, out int nextY, float moveDistance = 50)
        {
            nextX = currX;
            nextY = currY;

            //抓到前進方向得下一個點
            Vector2Int nextPt = GetExtensionPoint(new Vector2Int(currX, currY), dir, moveDistance);

            //計算移動方向上 X Y 誰最大=>用於跑For迴圈偵測格子數
            int nlen = (int)Math.Max(Math.Abs(nextPt.x - currX), Math.Abs(nextPt.y - currY));
            if (nlen <= 0)
                return;

            GMapData mapData = currentMapData;

            //計算角色所在的格子
            int GridX = (int)(currX / mapData.gridSizeX);
            int GridY = (int)(currY / mapData.gridSizeY);

            //for迴圈計算格子，得出目標點位置
            for (int i = 1; i <= nlen; i++)
            {
                //算出比較pos位置  =  原點 + (比例 * 下一個點)
                int posX = (int)(currX + (nextPt.x - currX) * i / nlen);
                int posY = (int)(currY + (nextPt.y - currY) * i / nlen);

                //當超出格子時，移動判斷格
                if ((int)(posX / mapData.gridSizeX) != GridX || (int)(posY / mapData.gridSizeY) != GridY)
                {
                    GridX = (int)(posX / mapData.gridSizeX);
                    GridY = (int)(posY / mapData.gridSizeY);
                    if (!CanMove(GridX, GridY, false))
                    {
                        break;
                    }
                }
                nextX = posX;
                nextY = posY;
            }
            Debug.Log(nextX + " " + nextY);

            //檢查poly碰撞
            Vector2 lastPt = CheckPoly(new Vector2(currX, currY), new Vector2(nextX, nextY), false, true);
            Debug.Log(lastPt);
            // Point checkdpt = GScene.GetMovePointByCheckRolePos(obj, obj.Coordinate, new Point((int)lastpt.x, (int)lastpt.y), CheckRolePos.BOSS_AND_OBS);
            // nX = (int)checkdpt.X;
            // nY = (int)checkdpt.Y;
            nextX = (int)lastPt.x;
            nextY = (int)lastPt.y;
            Debug.Log(nextX + " " + nextY);

            //計算移動位置
            CheckMoveDistance(/*obj,*/ currX, currY, ref nextX, ref nextY, 5, 20);
        }

        /// <summary>
        /// 代號靈原版演算
        /// 檢查障礙物地圖上，該格子是否能移動
        /// </summary>
        /// <param name="nX">格子位置X</param>
        /// <param name="nY">格子位置Y</param>
        /// <param name="bCheckHeight"></param>
        /// <returns></returns>
        private bool CanMove(int nX, int nY, bool bCheckHeight = false)
        {
            GMapData mapData = currentMapData;
            if (OnObstructionByGrid(nX, nY, mapData, bCheckHeight))
            {
                return false;
            }
            return true;
        }

        public static bool OnObstructionByGrid(int gridXNum, int gridYNum, GMapData currentMapData, bool checkHeight = true)
        {
            if (null == currentMapData)
            {
                return false;
            }
            if (checkHeight)
            {
                if (currentMapData.fixedObstructionHeight != null)
                {
                    if (0 == currentMapData.GetFixedObstruction(checkHeight)[gridXNum, gridYNum])
                    {
                        return true;
                    }
                }
                if (currentMapData.fixedObstructionClient != null)
                {
                    if (0 == currentMapData.fixedObstructionClient[gridXNum, gridYNum])
                    {
                        return true;
                    }
                }
            }
            if (0 == currentMapData.fixedObstruction[gridXNum, gridYNum])
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 計算poly碰撞
        /// </summary>
        /// <param name="from">原點</param>
        /// <param name="to">目標點</param>
        /// <param name="checkRolePos"></param>
        /// <param name="autoFixPos"></param>
        /// <returns></returns>
        private Vector2 CheckPoly(Vector2 from, Vector2 to, bool checkRolePos = false, bool autoFixPos = false)
        {
            //查歷史紀錄，如果有一致就直接返回
            foreach (var site in mCheckLeaderMoveList)
            {
                if (site.LastCheckFrom == from && site.LastCheckTo == to && site.LastbCheckRolePos == checkRolePos)
                {
                    return site.LastCheckResult;
                }
            }

            //確認poly
            Vector2 ret = ComputeCheckPoly(from, to, checkRolePos, autoFixPos);

            //將確認後資料加入到歷史資料裡
            mCheckLeaderMoveList.Add(new CheckLeaderMove()
            {
                LastCheckFrom = from,
                LastCheckTo = to,
                LastCheckResult = ret,
                LastbCheckRolePos = checkRolePos,
            });

            //紀錄保持10筆
            if (mCheckLeaderMoveList.Count >= 10)
            {
                mCheckLeaderMoveList.RemoveAt(0);
            }
            return ret;
        }

        /// <summary>
        /// Poly碰撞具體計算
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="checkRolePos"></param>
        /// <param name="autoFixPos"></param>
        /// <returns></returns>
        private Vector2 ComputeCheckPoly(Vector2 from, Vector2 to, bool checkRolePos = false, bool autoFixPos = false)
        {

            //確認移動距離
            int ndistance = (int)Math.Ceiling(Vector2.Distance(from, to));

            //移動=0時返回目標點
            if (ndistance == 0)
            {
                return to;
            }

            int distance = 0x8fffff;
            bool find = false;
            Vector2 cross = Vector2.zero;
            var enablePolys = currentMapData.enablePolys;
            mCheckpolysList.Clear();

            //檢查poly的碰撞，篩選需要檢查的poly
            //(enablePolys是角色每幀去自行添加)
            for (int i = 0; i < enablePolys.Count; i++)
            {
                var poly = enablePolys[i];
                poly.skip = false;
                if (poly.Check(from))
                {
                    poly.skip = true;
                }
                else if (!poly.CheckBox(from, to))
                {
                    poly.skip = true;
                }
                mCheckpolysList.Add(i);
            }

            //檢查poly
            //如果有撞到 則 enablePolys.Check = true 並且 cross 會返回撞到的那個點
            for (int i = 0; i < mCheckpolysList.Count; i++)
            {
                if (enablePolys[mCheckpolysList[i]].Check(from, to, ref cross, ref distance, autoFixPos))
                {
                    find = true;
                    if (distance == 0)
                        break;
                }
            }

            //有碰到poly所以要要檢查新位置
            if (find)
            {
                //添加需要檢查的點
                mCheckLeaderMoveListpt.Clear();
                foreach (var x in mCheckLeaderMoveListx)
                {
                    foreach (var y in mCheckLeaderMoveListy)
                    {
                        mCheckLeaderMoveListpt.Add(new Vector2((int)cross.x + x, (int)cross.y + y));
                    }
                }

                //返回檢查OK的點
                foreach (var checkpt in mCheckLeaderMoveListpt)
                {
                    bool bPass = true;
                    for (int i = 0; i < mCheckpolysList.Count; i++)
                    {
                        if (enablePolys[mCheckpolysList[i]].Check(checkpt))
                        {
                            bPass = false;
                            break;
                        }
                    }
                    if (bPass)
                    {
                        return checkpt;
                    }
                }

                //若無返回原點
                return from;
            }
            return to;
        }

        private void CheckMoveDistance(/*GSprite obj,*/ int currX, int currY, ref int nextX, ref int nextY, float checkDistance = 5.0f, float checkHightDistance = 20.0f)
        {
            //長度小於一數值，返回原點
            if ((nextX - currX) * (nextX - currX) + (nextY - currY) * (nextY - currY) < checkDistance * checkDistance)
            {
                nextX = currX;
                nextY = currY;
                return;
            }


            if (nextX != currX || nextY != currY)
            {
                // float spriteHeight = obj.The3DGameObject.transform.position.y;
                // float curHeight = spriteHeight;
                int nLen = (int)Vector2Int.Distance(new Vector2Int(nextX, nextY), new Vector2Int(currX, currY));
                for (int i = 5; i <= checkHightDistance; i += 5)
                {
                    int checkX = (int)(currX + (nextX - currX) * i / nLen);
                    int checkY = (int)(currY + (nextY - currY) * i / nLen);
                    // Vector3 pos = Global.GetGroundPos(obj._LeaderInfo, obj.SpriteType, checkX / 100f, checkY / 100f, Global.ConstGroundMaxHeight);
                    // if (pos.y > curHeight + 0.3f)
                    // {
                    //     nX = (int)(nCurrX + (nX - nCurrX) * (i - 5) / nLen);
                    //     nY = (int)(nCurrY + (nY - nCurrY) * (i - 5) / nLen);
                    //     break;
                    // }
                    // curHeight = pos.y;
                }
            }
            Debug.Log(nextX + " " + nextY);
        }

        /// <summary>
        /// 計算點的位置
        /// </summary>
        /// <param name="center">圓心位置</param>
        /// <param name="angle">角度(正數且小於360)(以Z箭頭為0，順時針方向)</param>
        /// <param name="length">長度</param>
        /// <returns></returns>
        public Vector2Int GetExtensionPoint(Vector2Int center, double angle, double length)
        {
            if (0.0 == angle)
            {
                return new Vector2Int(center.x, center.y + (int)length);
            }
            else if (angle > 0.0 && angle < 90.0)
            {
                double radian = angle * Math.PI / 180;
                double xMargin = Math.Sin(radian) * length;
                double yMargin = Math.Cos(radian) * length;
                return new Vector2Int(center.x + (int)xMargin, center.y + (int)yMargin);
            }
            else if (90.0 == angle)
            {
                return new Vector2Int(center.x + (int)length, center.y);
            }
            else if (angle > 90.0 && angle < 180.0)
            {
                double radian = (180.0 - angle) * Math.PI / 180;
                double xMargin = Math.Sin(radian) * length;
                double yMargin = Math.Cos(radian) * length;
                return new Vector2Int(center.x + (int)xMargin, center.y - (int)yMargin);
            }
            else if (180.0 == angle)
            {
                return new Vector2Int(center.x, center.y - (int)length);
            }
            else if (angle > 180.0 && angle < 270.0)
            {
                double radian = (angle - 180.0) * Math.PI / 180;
                double xMargin = Math.Sin(radian) * length;
                double yMargin = Math.Cos(radian) * length;
                return new Vector2Int(center.x - (int)xMargin, center.y - (int)yMargin);
            }
            else if (270.0 == angle)
            {
                return new Vector2Int(center.x - (int)length, center.y);
            }
            else if (angle > 270.0 && angle < 360.0)
            {
                double radian = (360.0 - angle) * Math.PI / 180;
                double xMargin = Math.Sin(radian) * length;
                double yMargin = Math.Cos(radian) * length;
                return new Vector2Int(center.x - (int)xMargin, center.y + (int)yMargin);
            }
            return center;
        }

    }


    //! 重要:這是將角色附近Poly放進去的函式，如果沒放在角色Update裡角色會穿牆
    // {   
    //  var polys = MapData.CurrentMapData.polys;
    //  var enablePolys = MapData.CurrentMapData.enablePolys;
    //  enablePolys.Clear();
    //  var pt = new Vector2(角色X軸座標 * 100, (角色Z軸座標 * 100);
    //  foreach (var poly in polys)
    //  {
    //   if (poly.CheckEnable(pt))
    //   {
    //    enablePolys.Add(poly);
    //   }
    //  }
    // }

}