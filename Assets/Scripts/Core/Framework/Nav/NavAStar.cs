using Core.Framework.Map;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Framework.Nav.AStarEx;
using Core.Framework.Nav.AStar;

namespace Core.Framework.Nav
{
    public class NavAStar
    {
        //紀錄現在尋路所使用的地圖
        private static int sMapCode = -1;

        //尋路工具
        private static PathFinderFast sPathFinderFast = null;


        /// <summary>
        /// 查詢路徑，
        /// 路徑寬度必須在 1unit以上，否則不計入算法
        /// </summary>
        /// <param name="start">起始點</param>
        /// <param name="end">終點</param>
        /// <param name="searchLimit">AStar演算次數，用於節省效能</param>
        /// <returns>路徑List</returns>
        public static List<Vector2Int> FindPath(Vector2 start, Vector2 end, int searchLimit = 1000)
        {

            //當地圖MapCode跟尋路不符時，重新初始化
            if (sMapCode != MapDataManager.Instance.MapCode)
            {
                Debug.LogWarning("尋路MapCode與地圖不符，重新初始化");
                sMapCode = -1;
                Init();
            }

            //尋路工具為null，初始化
            if (sPathFinderFast == null)
            {
                Debug.LogWarning("尋路工具 null，初始化");
                Init();
            }


            //開始尋路
            List<PathFinderNode> mNodeList = sPathFinderFast.FindPath(
               new Vector2Int((int)start.x, (int)start.y),
               new Vector2Int((int)end.x, (int)end.y), searchLimit);

            //初始化座標
            List<Vector2Int> mPath = new List<Vector2Int>();

            //未找到路徑回傳null
            if (mNodeList == null)
            {
                Debug.LogWarning("未找到路徑");
                return null;
            }

            //將尋路後的座標轉換成Unity座標
            for (int i = 0; i < mNodeList.Count; i++)
            {
                mPath.Add(new Vector2Int(mNodeList[i].X, mNodeList[i].Y));
            }

            return mPath;
        }


        /// <summary>
        /// 查詢路徑，
        /// 路徑寬度必須在 1unit以上，否則不計入算法
        /// </summary>
        /// <param name="start">起始點(Y值自動忽略)</param>
        /// <param name="end">終點(Y值自動忽略)</param>
        /// <param name="searchLimit">AStar演算次數，用於節省效能</param>
        /// <returns>路徑List Y軸固定為0</returns>
        public static List<Vector3Int> FindPath(Vector3 start, Vector3 end, int searchLimit = 1000)
        {
            List<Vector2Int> path2D = FindPath(
                new Vector2(start.x, start.z), new Vector2(end.x, end.z), searchLimit);

            //未找到路徑回傳null
            if (path2D == null)
            {
                Debug.LogWarning("未找到路徑");
                return null;
            }

            List<Vector3Int> path3D = new List<Vector3Int>();
            for (int i = 0; i < path2D.Count; i++)
            {
                path3D.Add(new Vector3Int(path2D[i].x, 0, path2D[i].y));
            }

            return path3D;
        }

        /// <summary>
        /// 初始化尋路系統
        /// </summary>
        private static void Init()
        {
            //紀錄地圖編號
            sMapCode = MapDataManager.Instance.MapCode;

            //初始化尋路工具
            var gridX = MapDataManager.Instance.currentMapData.gridSizeXNum;
            var gridY = MapDataManager.Instance.currentMapData.gridSizeYNum;
            sPathFinderFast = new PathFinderFast(MapDataManager.Instance.currentMapData.GetFixedObstruction(true))
            {
                Formula = HeuristicFormula.Custom2,
                Diagonals = true,
                HeuristicEstimate = 10,
                ReopenCloseNodes = true,
                SearchLimit = 2147483647,
                Punish = null,
                MaxNum = gridX >= gridY ? gridX : gridY
            };
            sPathFinderFast.EnablePunish = false;
        }


    }
}