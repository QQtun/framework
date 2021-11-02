using UnityEngine;
using System.Collections.Generic;
using Core.Framework.Map;

/// <summary>
/// 繪製Scene視窗裡的圖像
/// </summary>
public class UnWalkEditor : MonoBehaviour
{
    private const int ServerNavScale = 1;

    public UnWalkDataManager dataManager = new UnWalkDataManager();
    public int navMeshHeight = 10;
    public bool showNavMesh = true;
    public int selArea = 0;
    public int lastSelArea = -1;
    public int selPoint = 0;
    public int lastSelPoint = -1;

    //選取存檔
    public int selSaveData = 0;
    //上次選到的存檔列表
    public int lastSelSaveData = -1;


    //繪製服務器的導航格子
    public bool showServerNavGrid = false;
    //繪製客戶端的導航格子
    public bool showClientNavGrid = false;
    //繪製高度的導航格子
    public bool showHeightNavGrid = false;
    //繪製水域格子
    public bool showWaterGrid = false;
    /// <summary>
    /// 同時創建小地圖
    /// </summary>
    public bool createMinimap = false;
    /// <summary>
    /// 是否使用大阻擋
    /// </summary>
    public bool useClientGrid = false;

    void OnDrawGizmos()
    {
        DrawAllAreas();
        DrawSelectPoint();
        DrawServerGridInfo();
    }

    private void DrawServerGridInfo()
    {
        if (showServerNavGrid)
        {
            var dic = dataManager.dicObstruction;
            if (dic == null)
            {
                return;
            }
            // Debug.Log("Draw Server Grid.");
            for (int y = 0; y < dic.GetUpperBound(1); y++)
            {
                for (int x = 0; x < dic.GetUpperBound(0); x++)
                {
                    //設置默認值,可以通過的均在矩陣中用1表示
                    //CurrentMapData.fixedObstruction[x, y] = 1;
                    byte value = dic[x, y];
                    if (value != 0)
                    {
                        float fx = (float)x;
                        float fy = (float)y;
                        Gizmos.color = Color.red;
                        Gizmos.DrawCube(new Vector3(x / ServerNavScale /* / 2.0f */  + 0.25f, navMeshHeight, y / ServerNavScale /* / 2.0f */ ), new Vector3(0.5f, 0.1f, 0.1f));
                        Gizmos.DrawCube(new Vector3(x / ServerNavScale /* / 2.0f */ , navMeshHeight, y / ServerNavScale /* / 2.0f */ + 0.25f), new Vector3(0.1f, 0.1f, 0.5f));
                        Gizmos.DrawCube(new Vector3(x / ServerNavScale /* / 2.0f */  + 0.25f, navMeshHeight, y / ServerNavScale /* / 2.0f */  + 0.5f), new Vector3(0.5f, 0.1f, 0.1f));
                        Gizmos.DrawCube(new Vector3(x / ServerNavScale /* / 2.0f */  + 0.5f, navMeshHeight, y / ServerNavScale /* / 2.0f */  + 0.25f), new Vector3(0.1f, 0.1f, 0.5f));
                    }
                }
            }
        }
        if (showClientNavGrid)
        {

            var dic = dataManager.dicObstructionClient;
            if (dic == null)
            {
                return;
            }
            // Debug.Log("Draw Client Grid.");
            for (int y = 0; y < dic.GetUpperBound(1); y++)
            {
                for (int x = 0; x < dic.GetUpperBound(0); x++)
                {
                    //設置默認值,可以通過的均在矩陣中用1錶示
                    //CurrentMapData.fixedObstruction[x, y] = 1;
                    byte value = dic[x, y];
                    if (value != 0)
                    {
                        float fx = (float)x;
                        float fy = (float)y;
                        Gizmos.color = Color.black;
                        Gizmos.DrawCube(new Vector3(x / ServerNavScale /* / 2.0f */  + 0.25f, navMeshHeight, y / ServerNavScale /* / 2.0f */ ), new Vector3(0.5f, 0.1f, 0.1f));
                        Gizmos.DrawCube(new Vector3(x / ServerNavScale /* / 2.0f */ , navMeshHeight, y / ServerNavScale /* / 2.0f */  + 0.25f), new Vector3(0.1f, 0.1f, 0.5f));
                        Gizmos.DrawCube(new Vector3(x / ServerNavScale /* / 2.0f */  + 0.25f, navMeshHeight, y / ServerNavScale /* / 2.0f */  + 0.5f), new Vector3(0.5f, 0.1f, 0.1f));
                        Gizmos.DrawCube(new Vector3(x / ServerNavScale /* / 2.0f */  + 0.5f, navMeshHeight, y / ServerNavScale /* / 2.0f */  + 0.25f), new Vector3(0.1f, 0.1f, 0.5f));
                    }
                }
            }

        }

        if (showHeightNavGrid)
        {

            var dic = dataManager.dicObstructionHeight;
            if (dic == null)
            {
                return;
            }
            Debug.Log("Draw Height Grid.");
            for (int y = 0; y < dic.GetUpperBound(1); y++)
            {
                for (int x = 0; x < dic.GetUpperBound(0); x++)
                {
                    //設置默認值,可以通過的均在矩陣中用1錶示
                    //CurrentMapData.fixedObstruction[x, y] = 1;
                    byte value = dic[x, y];
                    if (value != 0)
                    {
                        float fx = (float)x;
                        float fy = (float)y;
                        Gizmos.color = Color.green;
                        Gizmos.DrawCube(new Vector3(x / ServerNavScale /* / 2.0f */  + 0.25f, navMeshHeight, y / ServerNavScale /* / 2.0f */ ), new Vector3(0.5f, 0.1f, 0.1f));
                        Gizmos.DrawCube(new Vector3(x / ServerNavScale /* / 2.0f */ , navMeshHeight, y / ServerNavScale /* / 2.0f */  + 0.25f), new Vector3(0.1f, 0.1f, 0.5f));
                        Gizmos.DrawCube(new Vector3(x / ServerNavScale /* / 2.0f */ + 0.25f, navMeshHeight, y / ServerNavScale /* / 2.0f */  + 0.5f), new Vector3(0.5f, 0.1f, 0.1f));
                        Gizmos.DrawCube(new Vector3(x / ServerNavScale /* / 2.0f */  + 0.5f, navMeshHeight, y / ServerNavScale /* / 2.0f */  + 0.25f), new Vector3(0.1f, 0.1f, 0.5f));
                    }
                }
            }

        }


    }

    private void DrawSelectPoint()
    {
        if (dataManager.allAreas.Count <= 0 || selArea >= dataManager.allAreas.Count ||
            dataManager.allAreas[selArea].points.Count <= 0 || selPoint >= dataManager.allAreas[selArea].points.Count)
            return;

        Gizmos.DrawIcon(dataManager.allAreas[selArea].points[selPoint].transform.position, "point.tif");
    }

    /// <summary>
    /// 繪製所有區域
    /// </summary>
    /// <returns></returns>
    private void DrawAllAreas()
    {
        // always draw unwalk area
        for (int i = 0; i < dataManager.allAreas.Count; i++)
        {
            DrawUnWalkArea(i);
        }
    }

    /// <summary>
    /// 繪製不可行走區域
    /// </summary>
    /// <returns></returns>
    private void DrawUnWalkArea(int areaNum)
    {
        if (dataManager == null)
            return;

        if (areaNum < dataManager.allAreas.Count)
        {
            List<GameObject> allPoints = dataManager.allAreas[areaNum].points;
            if (allPoints.Count <= 0)
                return;

            if (areaNum == selArea)
                Gizmos.color = Color.red;
            else
                Gizmos.color = Color.green;

            for (int i = 0; i < allPoints.Count; i++)
            {
                if (allPoints[i] == null)
                {
                    dataManager.CheckAllPoints();
                    return;
                }
                else
                {
                    if (i != allPoints.Count - 1)
                    {
                        if (allPoints[i + 1] == null)
                        {
                            dataManager.CheckAllPoints();
                            return;
                        }
                        Gizmos.DrawLine(allPoints[i].transform.position, allPoints[i + 1].transform.position);
                    }
                    else
                    {
                        Gizmos.DrawLine(allPoints[i].transform.position, allPoints[0].transform.position);
                    }
                    //var pos = allPoints[i].transform.position;
                    //int x = (int)pos.x;
                    //int z = (int)pos.z;
                    //var center = new Vector3((float)x+0.5f, pos.y, (float)z+0.5f);
                    //Gizmos.DrawCube(center, new Vector3(1.0f, 1.0f, 1.0f));
                }
            }
        }
    }

}
