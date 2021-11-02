/********************************************************************
   定義導航網格生成和尋路使用的基礎數據
*********************************************************************/
using System;
using System.Collections.Generic;
using UnityEngine;

public enum PointSide
{
    ON_LINE = 0,
    LEFT_SIDE = 1,
    RIGHT_SIDE = 2,
};

public enum LineCrossState
{
    COLINE = 0,
    PARALLEL,
    CROSS,
    NOT_CROSS
}

public enum PolyResCode
{
    Success = 0,
    ErrEmpty = -1,
    ErrNotCross = -2,
    ErrCrossNum = -3, // 多邊形交點數量錯誤
    ErrNotInside = -4
}

public enum NavResCode
{
    Success = 0,
    Failed = -1,
    NotFindDt = -2,
    FileNotExist = -3,
    VersionNotMatch = -4,
}

public enum PathResCode
{
    Success = 0,
    Failed = -1,
    NoMeshData = -2,
    NoStartTriOrEndTri = -3,
    NavIdNotMatch = -4, //導航網格的索引和id不匹配
    NotFoundPath = -5,
    CanNotGetNextWayPoint = -6,//找不到下一個拐點信息
    GroupNotMatch, //起點和中點在不同的孤島之間，無法到達
    NoCrossPoint,
}

public class WayPoint
{
    public Vector2 Position { get; set; }
    public Triangle Triangle { get; set; }

    public WayPoint() { }

    public WayPoint(Vector2 pnt, Triangle tri)
    {
        Position = pnt;
        Triangle = tri;
    }
}

public class Line2D
{
    private Vector2 mPointStart;
    public UnityEngine.Vector2 PointStart
    {
        get { return mPointStart; }
        set { mPointStart = value; }
    }
    private Vector2 mPointEnd;
    public UnityEngine.Vector2 PointEnd
    {
        get { return mPointEnd; }
        set { mPointEnd = value; }
    }
    public Line2D(Vector2 ps, Vector2 pe)
    {
        mPointStart = ps;
        mPointEnd = pe;
    }
    public Line2D()
    {
        mPointEnd = mPointStart = new Vector2();
    }

    /// <summary>
    /// 檢測線段是否在給定線段列錶裏麵
    /// </summary>
    /// <param name="allLines"></param>
    /// <param name="chkLine"></param>
    /// <param name="index">如果在，返回索引</param>
    /// <returns></returns>
    public static bool CheckLineIn(List<Line2D> allLines, Line2D chkLine, out int index)
    {
        index = -1;
        for (int i = 0; i < allLines.Count; i++)
        {
            Line2D line = allLines[i];
            if (line.Equals(chkLine))
            {
                index = i;
                return true;
            }
        }
        return false;
    }

    public override bool Equals(object lineTemp)
    {
        Line2D line = (Line2D)lineTemp;
        if (line == null)
        {
            return false;
        }

        return (SGMath.IsEqual(mPointStart, line.mPointStart) && SGMath.IsEqual(mPointEnd, line.mPointEnd));
    }

    public override int GetHashCode() { return 0; }



    /// <summary>
    /// 判斷點與直線的關係，假設你站在a點朝嚮b點， 
    /// 則輸入點與直線的關係分為：Left, Right or Centered on the line
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public PointSide classifyPoint(Vector2 point)
    {
        if (point == mPointStart || point == mPointEnd)
            return PointSide.ON_LINE;
        //嚮量a
        Vector2 vectorA = mPointEnd - mPointStart;
        //嚮量b
        Vector2 vectorB = point - mPointStart;

        float crossResult = SGMath.CrossProduct(vectorA, vectorB);
        if (SGMath.IsEqualZero(crossResult))
            return PointSide.ON_LINE;
        else if (crossResult < 0)
            return PointSide.RIGHT_SIDE;
        else
            return PointSide.LEFT_SIDE;
    }

    /// <summary>
    /// 線段是否相等，忽略方嚮
    /// </summary>
    /// <param name="line"></param>
    /// <returns></returns>
    public bool Equals(Line2D line)
    {
        //隻是一個點
        if (SGMath.IsEqualZero(line.mPointStart - line.mPointEnd) ||
            SGMath.IsEqualZero(mPointStart - mPointEnd))
            return false;

        bool bEquals = SGMath.IsEqualZero(mPointStart - line.mPointStart) ? true : SGMath.IsEqualZero(mPointStart - line.mPointEnd);
        if (bEquals)
        {
            bEquals = SGMath.IsEqualZero(mPointEnd - line.mPointStart) ? true : SGMath.IsEqualZero(mPointEnd - line.mPointEnd);
        }
        return bEquals;
    }

    /// <summary>
    /// 獲得直線方嚮
    /// </summary>
    /// <returns></returns>
    public Vector2 GetDirection()
    {
        Vector2 dir = mPointEnd - mPointStart;
        return dir;
    }

    /// <summary>
    /// 計算兩條二維線段的交點
    /// </summary>
    /// <param name="other">Other line</param>
    /// <param name="intersectPoint">線段交點</param>
    /// <returns>返回值說明了兩條線段的位置關係(COLINE,PARALLEL,CROSS,NOT_CROSS) </returns>
    public LineCrossState intersection(Line2D other, out Vector2 intersectPoint)
    {
        intersectPoint.x = intersectPoint.y = float.NaN;
        if (!SGMath.CheckCross(PointStart, mPointEnd, other.PointStart, other.PointEnd))
            return LineCrossState.NOT_CROSS;

        double A1, B1, C1, A2, B2, C2;

        A1 = this.mPointEnd.y - this.mPointStart.y;
        B1 = this.mPointStart.x - this.mPointEnd.x;
        C1 = this.mPointEnd.x * this.mPointStart.y - this.mPointStart.x * this.mPointEnd.y;

        A2 = other.mPointEnd.y - other.mPointStart.y;
        B2 = other.mPointStart.x - other.mPointEnd.x;
        C2 = other.mPointEnd.x * other.mPointStart.y - other.mPointStart.x * other.mPointEnd.y;

        if (SGMath.IsEqualZero(A1 * B2 - B1 * A2))
        {
            if (SGMath.IsEqualZero((A1 + B1) * C2 - (A2 + B2) * C1))
            {
                return LineCrossState.COLINE;
            }
            else
            {
                return LineCrossState.PARALLEL;
            }
        }
        else
        {
            intersectPoint.x = (float)((B2 * C1 - B1 * C2) / (A2 * B1 - A1 * B2));
            intersectPoint.y = (float)((A1 * C2 - A2 * C1) / (A2 * B1 - A1 * B2));
            return LineCrossState.CROSS;
        }
    }

    public float Length()
    {
        return (float)Math.Sqrt(Math.Pow(mPointStart.x - mPointEnd.x, 2.0) + Math.Pow(mPointStart.y - mPointEnd.y, 2.0));
    }

}

public class Triangle
{
    public Vector2[] Points { get; set; } //三角形頂點列錶

    public int ID { get; set; } //三角形編號

    public int Group { get; set; } //不同的孤島要分組

    public int[] Neighbors { get; set; } //三角形鄰居節點

    public Vector2 CenterPos { get; set; } //三角形中心位置

    // 尋路相關參數
    public int SessionID { get; set; }
    public int ParentId { get; set; }
    public bool IsOpen { get; set; }
    // 相鄰兩邊的中點距離 [3/10/2011 ivan edit]
    public double[] WallDistance { get; set; }

    // 路徑的綜合評分 [3/10/2011 ivan edit]
    public double HValue { get; set; }
    public double GValue { get; set; }
    public int ArrivalWallIndex { get; set; }
    // 尋路相關
    public int OutWallIndex { get; set; }

    //三角形包圍盒
    public Rect BoxCollider { get; set; }

    public Triangle()
    {
        InitData();
    }

    public Triangle(Vector2 pointA, Vector2 pointB, Vector2 pointC)
    {
        InitData();
        Points[0] = pointA;
        Points[1] = pointB;
        Points[2] = pointC;

        CalcCenter();
        CalcCollider();
    }

    /// <summary>
    /// 計算包圍盒
    /// </summary>
    public void CalcCollider()
    {
        if (Points[0] == Points[1] || Points[1] == Points[2] || Points[0] == Points[2])
            return;

        Rect collider = new Rect();
        collider.xMin = collider.xMax = Points[0].x;
        collider.yMin = collider.yMax = Points[0].y;
        for (int i = 1; i < 3; i++)
        {
            if (Points[i].x < collider.xMin)
            {
                collider.xMin = Points[i].x;
            }
            else if (Points[i].x > collider.xMax)
            {
                collider.xMax = Points[i].x;
            }

            if (Points[i].y < collider.yMin)
            {
                collider.yMin = Points[i].y;
            }
            else if (Points[i].y > collider.yMax)
            {
                collider.yMax = Points[i].y;
            }
        }

        BoxCollider = collider;
    }



    /// <summary>
    /// 初始化數據
    /// </summary>
    private void InitData()
    {
        Points = new Vector2[3];
        Neighbors = new int[3];
        WallDistance = new double[3];

        for (int i = 0; i < 3; i++)
        {
            Neighbors[i] = -1;
        }

        SessionID = -1;
        ParentId = -1;
        IsOpen = false;
        HValue = 0;
        GValue = 0;
        ArrivalWallIndex = -1;
    }

    /// <summary>
    /// 計算中心點位置
    /// </summary>
    private void CalcCenter()
    {
        Vector2 temp = new Vector2();
        temp.x = (Points[0].x + Points[1].x + Points[2].x) / 3;
        temp.y = (Points[0].y + Points[1].y + Points[2].y) / 3;
        CenterPos = temp;
    }

    /// <summary>
    /// 保存當前三角形的鄰居節點
    /// </summary>
    /// <param name="triId"></param>
    /// <returns></returns>
    //public void SetNeighbor(int triId)
    //{
    //    for (int i = 0; i < Neighbors.Length; i++)
    //    {
    //        if(Neighbors[i] == -1)
    //        {
    //            Neighbors[i] = triId;
    //            return;
    //        }
    //    }
    //}

    /// <summary>
    /// 根據索引獲得相應的邊
    /// </summary>
    /// <param name="sideIndex"></param>
    /// <returns></returns>
    public Line2D GetSide(int sideIndex)
    {
        Line2D newSide;

        switch (sideIndex)
        {
            case 0:
                newSide = new Line2D(Points[0], Points[1]);
                break;
            case 1:
                newSide = new Line2D(Points[1], Points[2]);
                break;
            case 2:
                newSide = new Line2D(Points[2], Points[0]);
                break;
            default:
                newSide = new Line2D(Points[0], Points[1]);
                //Debug.LogError("Triangle:GetSide 獲取索引[" + sideIndex + "]錯誤");
                break;
        }

        return newSide;
    }

    /// <summary>
    /// 測試給定點是否在三角形中
    /// 點在三角形邊上也算
    /// </summary>
    /// <param name="pt"></param>
    /// <returns></returns>
    public bool isPointIn(int x, int y)
    {
        var pt = new Vector2((float)x, (float)y);
        if (BoxCollider.xMin != BoxCollider.xMax && !BoxCollider.Contains(pt))
            return false;

        PointSide resultA = GetSide(0).classifyPoint(pt);
        PointSide resultB = GetSide(1).classifyPoint(pt);
        PointSide resultC = GetSide(2).classifyPoint(pt);

        if (resultA == PointSide.ON_LINE || resultB == PointSide.ON_LINE || resultC == PointSide.ON_LINE)
        {
            return true;
        }
        else if (resultA == PointSide.RIGHT_SIDE && resultB == PointSide.RIGHT_SIDE && resultC == PointSide.RIGHT_SIDE)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// 計算三角形估價函數（h值）
    /// 使用該三角形的中心點（3個頂點的平均值）到路徑終點的x和y方嚮的距離。
    /// </summary>
    /// <param name="endPos"></param>
    public void CalcHeuristic(Vector2 endPos)
    {
        double xDelta = Math.Abs(CenterPos.x - endPos.x);
        double yDelta = Math.Abs(CenterPos.y - endPos.y);
        HValue = Math.Sqrt(xDelta * xDelta + yDelta * yDelta);
    }

    public void CalcWallDistance()
    {
        Vector2[] wallMidPoint = new Vector2[3];
        wallMidPoint[0] = new Vector2((Points[0].x + Points[1].x) / 2, (Points[0].y + Points[1].y) / 2);
        wallMidPoint[1] = new Vector2((Points[1].x + Points[2].x) / 2, (Points[1].y + Points[2].y) / 2);
        wallMidPoint[2] = new Vector2((Points[2].x + Points[0].x) / 2, (Points[2].y + Points[0].y) / 2);

        WallDistance[0] = Math.Sqrt((wallMidPoint[0].x - wallMidPoint[1].x) * (wallMidPoint[0].x - wallMidPoint[1].x)
            + (wallMidPoint[0].y - wallMidPoint[1].y) * (wallMidPoint[0].y - wallMidPoint[1].y));
        WallDistance[1] = Math.Sqrt((wallMidPoint[1].x - wallMidPoint[2].x) * (wallMidPoint[1].x - wallMidPoint[2].x)
            + (wallMidPoint[1].y - wallMidPoint[2].y) * (wallMidPoint[1].y - wallMidPoint[2].y));
        WallDistance[2] = Math.Sqrt((wallMidPoint[2].x - wallMidPoint[0].x) * (wallMidPoint[2].x - wallMidPoint[0].x)
            + (wallMidPoint[2].y - wallMidPoint[0].y) * (wallMidPoint[2].y - wallMidPoint[0].y));
    }


    /// <summary>
    /// 設置當前三角形的穿入邊
    /// </summary>
    /// <param name="arriId"></param>
    public void SetArrivalWall(int arriId)
    {
        if (arriId == -1)
            return;

        ArrivalWallIndex = GetWallIndex(arriId);
    }

    /// <summary>
    /// 獲得鄰居邊的索引
    /// </summary>
    /// <param name="wallId"></param>
    /// <returns></returns>
    public int GetWallIndex(int wallId)
    {
        for (int i = 0; i < 3; i++)
        {
            if (Neighbors[i] != -1 && Neighbors[i] == wallId)
                return i;
        }
        return -1;
    }

    /// <summary>
    /// 獲得通過當前三角形的花費
    /// </summary>
    /// <param name="outWallId"></param>
    /// <returns></returns>
    public double GetCost(int outWallId)
    {
        int outWallIndex = GetWallIndex(outWallId);
        if (ArrivalWallIndex == -1)
            return 0;
        else if (ArrivalWallIndex != 0)
            return WallDistance[1];
        else if (outWallIndex == 1)
            return WallDistance[0];
        else
            return WallDistance[2];
    }

    /// <summary>
    /// 計算鄰居節點
    /// </summary>
    /// <param name="triNext"></param>
    /// <returns></returns>
    public int IsNeighbor(Triangle triNext)
    {
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (GetSide(i).Equals(triNext.GetSide(j)))
                    return i;
            }
        }
        return -1;
    }


    /// <summary>
    /// 由於重複使用導航網格，需要重置數據
    /// </summary>
    public void ResetData()
    {
        SessionID = -1;
        ParentId = -1;
        IsOpen = false;
        HValue = 0;
        GValue = 0;
        ArrivalWallIndex = -1;
    }

    /// <summary>
    /// 計算三角形麵積
    /// p = (a+b+c)/2
    /// s = 根號(p*(p-a)*(p-b)*(p-c))
    /// </summary>
    /// <returns></returns>
    float area = 0;
    public float Area()
    {
        if (area == 0)
        {
            float a = (float)Math.Sqrt((Points[0].x - Points[1].x) * (Points[0].x - Points[1].x)
                                      + (Points[0].y - Points[1].y) * (Points[0].y - Points[1].y));
            float b = (float)Math.Sqrt((Points[1].x - Points[2].x) * (Points[1].x - Points[2].x)
                                      + (Points[1].y - Points[2].y) * (Points[1].y - Points[2].y));
            float c = (float)Math.Sqrt((Points[2].x - Points[0].x) * (Points[2].x - Points[0].x)
                                      + (Points[2].y - Points[0].y) * (Points[2].y - Points[0].y));
            float p = (a + b + c) / 2;
            area = (float)Math.Sqrt(p * (p - a) * (p - b) * (p - c));
        }

        return area;
    }
}

public class NavNode
{
    public Vector2 vertex;  //頂點
    public bool passed;     //是否被訪問過
    public bool isMain;     //是否主多邊形頂點
    public bool o;          //是否輸出點
    public bool isIns;      //是否交點
    public NavNode other;   //交點用，另個多邊形上的節點
    public NavNode next;    //後麵一個點

    public NavNode(Vector2 point, bool isin, bool bMain)
    {
        vertex = point;
        isIns = isin;
        isMain = bMain;
        passed = false;
        o = false;
    }
}
