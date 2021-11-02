using System.Collections.Generic;

namespace Core.Framework.Map
{
    /// <summary>
    /// Author: Peter
    /// Date : 2021/05/07
    /// Desc : 地圖資料檔
    /// </summary>
    public class GMapData
    {   
        //Map代號
        public int mapCode;
        
        //Map長寬
        public int mapWidth;
        public int mapHeight;
        
        //一格地圖格子大小
        public int gridSizeX = 100;
        public int gridSizeY = 100;
        
        //地圖大小(採2的次方為單位)
        public int gridSizeXNum = 0;
        public int gridSizeYNum = 0;

        //障礙物地圖 0代表不能走
        public byte[,] fixedObstruction;
        public byte[,] fixedObstructionClient;
        public byte[,] fixedObstructionHeight;
        
        //紀錄場上的障礙物
        public List<PolyData> polys = null;

        //由角色在Update設定，紀錄角色現在接近那些Poly，節省效能
        public List<PolyData> enablePolys = new List<PolyData>();

        /// <summary>
        /// 拿取障礙物地圖
        /// </summary>
        /// <param name="autoMove"></param>
        /// <returns></returns>
        public byte[,] GetFixedObstruction(bool autoMove = false)
        {
            if (autoMove)
            {
                if (fixedObstructionHeight != null)
                    return fixedObstructionHeight;
            }
            if (fixedObstructionClient != null)
                return fixedObstructionClient;
            return fixedObstruction;
        }

    }
}