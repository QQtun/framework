using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Framework.Map
{
    public class Polygon
    {
        public int tag;

        public List<Vector2> allPoints;

        /// <summary>
        /// 邊界格子
        /// </summary>
        private List<Vector2> mBoundPoints = new List<Vector2>();
        public Polygon()
        {
            allPoints = new List<Vector2>();
            tag = 0;
        }

        public Polygon(List<Vector2> points)
        {
            allPoints = points;
            tag = 0;
        }

        public void CalcBoundaryGrid()
        {
            if (allPoints.Count == 0 || mBoundPoints.Count > 0)
                return;

            for (int i = 0; i < allPoints.Count; i++)
            {
                var p1 = allPoints[i];

                int index = i + 1;
                if (index >= allPoints.Count)
                {
                    index = 0;
                }
                var p2 = allPoints[index];
                var xlength = Math.Abs(p1.x - p2.x);
                var ylength = Math.Abs(p1.y - p2.y);
                if (xlength > ylength)
                {
                    for (int step = 0; step < xlength; step++)
                    {
                        Vector2 p = Vector2.Lerp(p1, p2, step / xlength);
                        p.x = (int)p.x;
                        p.y = (int)p.y;

                        mBoundPoints.Add(p);
                    }
                }
                else
                {
                    for (int step = 0; step < ylength; step++)
                    {
                        Vector2 p = Vector2.Lerp(p1, p2, step / ylength);
                        p.x = (int)p.x;
                        p.y = (int)p.y;

                        mBoundPoints.Add(p);
                    }
                }
            }
        }

        /// <summary>
        /// 刪除重複頂點
        /// </summary>
        /// <returns></returns>
        public void DelRepeatPoint()
        {
            for (int i = 0; i < allPoints.Count; i++)
            {
                for (int j = i + 1; j < allPoints.Count; j++)
                {
                    if (SGMath.IsEqualZero(allPoints[i] - allPoints[j]))
                    {
                        allPoints.Remove(allPoints[j]);
                        j = i;
                    }
                }
            }
        }

        /// <summary>
        /// 順時針排序
        /// </summary>
        /// <returns></returns>
        public void CW()
        {
            if (!IsCW())
            {
                allPoints.Reverse();
            }
        }

        /// <summary>
        /// 判斷是否是順時針
        /// </summary>
        /// <returns></returns>
        public bool IsCW()
        {
            if (allPoints.Count <= 2)
                return false;

            //最上(y最小)最左(x最小)點，肯定是一個凸點
            //尋找最上點
            Vector2 topPoint = allPoints[0];
            int topIndex = 0;
            for (int i = 1; i < allPoints.Count; i++)
            {
                Vector2 currPoint = allPoints[i];
                if ((topPoint.y > currPoint.y)
                    || ((topPoint.y == currPoint.y) && (topPoint.x > currPoint.x)))
                {
                    topPoint = currPoint;
                    topIndex = i;
                }
            }

            //尋找左右鄰居
            int preIndex = (topIndex - 1) >= 0 ? (topIndex - 1) : (allPoints.Count - 1);
            int nextIndex = (topIndex + 1) < allPoints.Count ? (topIndex + 1) : 0;

            Vector2 prePoint = allPoints[preIndex];
            Vector2 nextPoint = allPoints[nextIndex];

            //三點共線情況不存在，若三點共線則說明必有一點的y（斜線）或x（水準線）小於topPt
            float r = SGMath.CrossProduct((prePoint - topPoint), (nextPoint - topPoint));
            if (r > 0)
                return true;

            return false;
        }

        /// <summary>
        /// 返回多邊形包圍盒 => 也就是外接矩形概念(包圍矩形)
        /// </summary>
        /// <returns></returns>
        public Rect GetCoverRect()
        {
            Rect rect = new Rect(0, 0, 0, 0);

            for (int i = 0; i < allPoints.Count; i++)
            {
                Vector2 pt = allPoints[i];
                if (i == 0)
                {
                    rect.xMin = rect.xMax = pt.x;
                    rect.yMin = rect.yMax = pt.y;
                }
                if (rect.xMin > pt.x)
                    rect.xMin = pt.x;
                if (rect.xMax < pt.x)
                    rect.xMax = pt.x;
                if (rect.yMin > pt.y)
                    rect.yMin = pt.y;
                if (rect.yMax < pt.y)
                    rect.yMax = pt.y;
            }
            return rect;
        }

        /// <summary>
        /// 查找point是否在節點列錶裏麵
        /// </summary>
        /// <param name="nodeList">節點列錶</param>
        /// <param name="point">用於查找的節點</param>
        /// <param name="pIndex">返回節點索引</param>
        /// <returns>if inside,return sucess,else return not inside</returns>
        public static PolyResCode GetNodeIndex(List<NavNode> nodeList, Vector2 point, out int pIndex)
        {
            pIndex = -1;
            for (int i = 0; i < nodeList.Count; i++)
            {
                NavNode node = nodeList[i];
                if (SGMath.Equals(node.vertex, point))
                {
                    pIndex = i;
                    return PolyResCode.Success;
                }
            }
            return PolyResCode.ErrNotInside;
        }

        /// <summary>
        /// 合並兩個節點列錶，生成交點，並按順時針序插入到頂點錶中
        /// </summary>
        /// <param name="c0">主多邊形頂點錶，並返回插入交點後的頂點錶</param>
        /// <param name="c1">合並多邊形頂點錶，並返回插入交點後的頂點錶</param>
        /// <returns></returns>
        public static PolyResCode IntersectPoint(List<NavNode> c0, List<NavNode> c1, out int nInsCnt)
        {
            nInsCnt = 0;

            NavNode startNode0 = c0[0];
            NavNode startNode1 = null;
            Line2D line0, line1;
            Vector2 insPoint;
            bool hasIns = false;

            while (startNode0 != null)
            {
                // 判斷是否到末點了
                if (startNode0.next == null)
                    line0 = new Line2D(startNode0.vertex, c0[0].vertex);
                else
                    line0 = new Line2D(startNode0.vertex, startNode0.next.vertex);

                startNode1 = c1[0];
                hasIns = false;

                while (startNode1 != null)
                {
                    if (startNode1.next == null)
                        line1 = new Line2D(startNode1.vertex, c1[0].vertex);
                    else
                        line1 = new Line2D(startNode1.vertex, startNode1.next.vertex);

                    if (line0.intersection(line1, out insPoint) == LineCrossState.CROSS)
                    {
                        int insPotIndex = -1;
                        if (Polygon.GetNodeIndex(c0, insPoint, out insPotIndex) == PolyResCode.ErrNotInside)
                        {
                            nInsCnt++;
                            NavNode node0 = new NavNode(insPoint, true, true);
                            NavNode node1 = new NavNode(insPoint, true, false);

                            c0.Add(node0);
                            c1.Add(node1);

                            node0.other = node1;
                            node1.other = node0;

                            //插入頂點列錶
                            node0.next = startNode0.next;
                            startNode0.next = node0;
                            node1.next = startNode1.next;
                            startNode1.next = node1;

                            if (line0.classifyPoint(line1.PointEnd) == PointSide.RIGHT_SIDE)
                            {
                                node0.o = true;
                                node1.o = true;
                            }

                            hasIns = true;
                            break;
                        }
                    }
                    startNode1 = startNode1.next;

                }
                if (!hasIns)
                    startNode0 = startNode0.next;

            }

            return PolyResCode.Success;
        }

        /// <summary>
        /// 合並兩個多邊形(多邊形必須先轉換為順時針方嚮，調用CW()函數!)
        /// </summary>
        /// <param name="other"></param>
        /// <param name="polyRes"></param>
        /// <returns></returns>
        public PolyResCode Union(Polygon other, ref List<Polygon> polyRes)
        {
            if (allPoints.Count == 0 || other.allPoints.Count == 0)
                return PolyResCode.ErrEmpty;
            else if (!SGMath.CheckCross(GetCoverRect(), other.GetCoverRect()))
                return PolyResCode.ErrNotCross;

            // 轉換為順時針方嚮
            //this.CW();
            //other.CW();

            List<NavNode> mainNode = new List<NavNode>();     //主多邊形頂點
            List<NavNode> subNode = new List<NavNode>();      //需要合並的多邊形

            // init main nodes
            for (int i = 0; i < allPoints.Count; i++)
            {
                NavNode currNode = new NavNode(allPoints[i], false, true);
                if (i > 0)
                {
                    NavNode preNode = mainNode[i - 1];
                    preNode.next = currNode;
                }
                mainNode.Add(currNode);
            }

            // init sub nodes
            for (int j = 0; j < other.allPoints.Count; j++)
            {
                NavNode currNode = new NavNode(other.allPoints[j], false, false);
                if (j > 0)
                {
                    NavNode preNode = subNode[j - 1];
                    preNode.next = currNode;
                }
                subNode.Add(currNode);
            }

            int insCnt = 0;
            PolyResCode result = Polygon.IntersectPoint(mainNode, subNode, out insCnt);
            if (result == PolyResCode.Success && insCnt > 0)
            {
                if (insCnt % 2 != 0)
                {
                    return PolyResCode.ErrCrossNum;
                }
                else
                {
                    PolyResCode linkRes = Polygon.LinkToPolygon(mainNode, subNode, ref polyRes);
                    return linkRes;
                }
            }

            return PolyResCode.ErrCrossNum;
        }

        /// <summary>
        /// 用於合並傳進來的多邊形數組，返回合並後的多邊形數組，
        /// 如果生成了孤島，則孤島的tag標誌遞增
        /// </summary>
        /// <param name="polys"></param>
        /// <returns></returns>
        public static PolyResCode UnionAll(ref List<Polygon> polys)
        {
            int tag = 1;

            for (int i = 0; i < polys.Count; i++)
                polys[i].CW();

            for (int i = 0; i < polys.Count; i++)
            {
                Polygon p1 = polys[i];
                for (int j = 0; j < polys.Count; j++)
                {
                    Polygon p2 = polys[j];
                    if (!p1.Equals(p2))
                    {
                        List<Polygon> polyResult = new List<Polygon>();
                        PolyResCode result = p1.Union(p2, ref polyResult);

                        if (result == PolyResCode.Success && polyResult.Count > 0)
                        {
                            polys.Remove(p1);
                            polys.Remove(p2);

                            for (int k = 0; k < polyResult.Count; k++)
                            {
                                Polygon poly = polyResult[k];
                                if (/*polyResult.Count > 1 &&*/ !poly.IsCW())
                                    poly.tag = tag++;//如果逆時針說明這個多邊形是孤島

                                polys.Add(poly);
                            }
                            i = -1;
                            break;
                        }
                    }
                }
            }
            return PolyResCode.Success;
        }

        /// <summary>
        /// 合並兩個節點列錶為一個多邊形，結果為順時針序( 生成的內部孔洞多邊形為逆時針序)
        /// </summary>
        /// <param name="mainNode"></param>
        /// <param name="subNode"></param>
        /// <param name="polyRes"></param>
        /// <returns></returns>
        private static PolyResCode LinkToPolygon(List<NavNode> mainNode, List<NavNode> subNode, ref List<Polygon> polyRes)
        {
            polyRes.Clear();
            for (int i = 0; i < mainNode.Count; i++)
            {
                NavNode currNode = mainNode[i];

                // 選擇一個冇有訪問過的交點做起始點
                if (currNode.isIns && !currNode.passed)
                {
                    List<Vector2> points = new List<Vector2>();
                    while (currNode != null)
                    {
                        currNode.passed = true;

                        //交點轉換
                        if (currNode.isIns)
                        {
                            currNode.other.passed = true;

                            if (!currNode.o)//該交點為進點（跟蹤裁剪多邊形邊界）
                            {
                                if (currNode.isMain)//當前點在主多邊形中
                                    currNode = currNode.other;//切換到裁剪多邊形中
                            }
                            else
                            {
                                //該交點為出點（跟蹤主多邊形邊界）
                                if (!currNode.isMain)//當前點在裁剪多邊形中
                                    currNode = currNode.other;//切換到主多邊形中
                            }
                        }

                        points.Add(currNode.vertex);

                        if (currNode.next == null)
                        {
                            if (currNode.isMain)
                                currNode = mainNode[0];
                            else
                                currNode = subNode[0];
                        }
                        else
                            currNode = currNode.next;

                        if (currNode.vertex == points[0])
                            break;
                    }

                    // 刪除重複頂點
                    Polygon poly = new Polygon(points);
                    poly.DelRepeatPoint();
                    polyRes.Add(poly);
                }
            }
            return PolyResCode.Success;
        }
        /// <summary>
        /// 檢查輸入的點，是否在Poly裡麵
        /// </summary>
        /// <param name="target">輸入點</param>
        /// <returns></returns>
        public bool IsPointIn(Vector2 target)
        {
            int i, j = allPoints.Count - 1;
            bool oddNodes = false;

            for (i = 0; i < allPoints.Count; i++)
            {
                Vector2 point1 = allPoints[i];
                Vector2 point2 = allPoints[j];
                if (point1.y < target.y && point2.y >= target.y
                || point2.y < target.y && point1.y >= target.y)
                {
                    if (point1.x + (target.y - point1.y) / (point2.y - point1.y) * (point2.x - point1.x) < target.x)
                    {
                        oddNodes = !oddNodes;
                    }
                }
                j = i;
            }

            return oddNodes;
        }

        private int Dblcmp(double a, double b)
        {
            if (Math.Abs(a - b) <= 1E-6) return 0;
            if (a > b) return 1;
            else return -1;
        }

        //***************點積判點是否在線段上***************
        private double Dot(double x1, double y1, double x2, double y2) //點積
        {
            return x1 * x2 + y1 * y2;
        }

        private int Point_on_line(Vector2 a, Vector2 b, Vector2 c) //求a點是不是在線段bc上，>0不在，=0與端點重合，<0在。
        {
            return Dblcmp(Dot(b.x - a.x, b.y - a.y, c.x - a.x, c.y - a.y), 0);
        }
        //**************************************************
        private double Cross(double x1, double y1, double x2, double y2)
        {
            return x1 * y2 - x2 * y1;
        }
        private double Ab_cross_ac(Vector2 a, Vector2 b, Vector2 c) //ab與ac的叉積
        {
            return Cross(b.x - a.x, b.y - a.y, c.x - a.x, c.y - a.y);
        }
        private int Ab_cross_cd(Vector2 a, Vector2 b, Vector2 c, Vector2 d, ref Vector2 p) //求ab是否與cd相交，交點為p。1規範相交，0交點是一線段的端點，-1不相交。
        {
            double s1, s2, s3, s4;
            int d1, d2, d3, d4;
            d1 = Dblcmp(s1 = Ab_cross_ac(a, b, c), 0);
            d2 = Dblcmp(s2 = Ab_cross_ac(a, b, d), 0);
            d3 = Dblcmp(s3 = Ab_cross_ac(c, d, a), 0);
            d4 = Dblcmp(s4 = Ab_cross_ac(c, d, b), 0);

            //如果規範相交則求交點
            if ((d1 ^ d2) == -2 && (d3 ^ d4) == -2)
            {
                p.x = (float)(((double)c.x * s2 - (double)d.x * s1) / (s2 - s1));
                p.y = (float)(((double)c.y * s2 - (double)d.y * s1) / (s2 - s1));
                return 1;
            }

            //如果不規範相交
            if (d1 == 0 && Point_on_line(c, a, b) <= 0)
            {
                p = c;
                return 0;
            }
            if (d2 == 0 && Point_on_line(d, a, b) <= 0)
            {
                p = d;
                return 0;
            }
            if (d3 == 0 && Point_on_line(a, c, d) <= 0)
            {
                p = a;
                return 0;
            }
            if (d4 == 0 && Point_on_line(b, c, d) <= 0)
            {
                p = b;
                return 0;
            }
            //如果不相交
            return -1;
        }

        public bool IsCross(Vector2 from, Vector2 to)
        {
            Vector2 checkpt = Vector2.zero;
            Vector2 startCheckpt = Vector2.zero;
            Vector2 endCheckpt = Vector2.zero;
            //兩兩判定碰撞，選一個裏from的最近點
            for (int i = 0; i < allPoints.Count; i++)
            {
                startCheckpt = allPoints[i];
                endCheckpt = (i == allPoints.Count - 1) ? allPoints[0] : allPoints[i + 1];
                if (Ab_cross_cd(from, to, startCheckpt, endCheckpt, ref checkpt) >= 0)
                {
                    return true;
                }
            }
            return false;
        }
       
        /// <summary>
        /// 是否在邊界上
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool IsOnLine(Vector2 target)
        {
            if (mBoundPoints.Count == 0)
                return false;
            for (int i = 0; i < mBoundPoints.Count; i++)
            {
                if (target.Equals(mBoundPoints[i]))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
