using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Framework.Map
{
    public class PolyData
    {
        public Polygon ploy = new Polygon();

        public bool skip = false;

        public bool enable = true;

        private bool mInit = false;

        private static List<System.Tuple<float, int, Vector2>> sCrossList = new List<System.Tuple<float, int, Vector2>>();

        private Rect mTargetRect = new Rect();

        private Rect mRect;

        public Rect Rect
        {
            get
            {
                if (!mInit)
                {
                    mInit = true;
                    mRect = ploy.GetCoverRect();
                }
                return mRect;
            }
        }

        private int Dblcmp(double a, double b)
        {
            if (Math.Abs(a - b) <= 1E-6) return 0;
            if (a > b) return 1;
            else return -1;
        }

        private double Dot(double x1, double y1, double x2, double y2)
        {
            return x1 * x2 + y1 * y2;
        }

        private int Point_on_line(Vector2 a, Vector2 b, Vector2 c)
        {
            return Dblcmp(Dot(b.x - a.x, b.y - a.y, c.x - a.x, c.y - a.y), 0);
        }

        private double Cross(double x1, double y1, double x2, double y2)
        {
            return x1 * y2 - x2 * y1;
        }

        private double Ab_cross_ac(Vector2 a, Vector2 b, Vector2 c)
        {
            return Cross(b.x - a.x, b.y - a.y, c.x - a.x, c.y - a.y);
        }

        private int Ab_cross_cd(Vector2 a, Vector2 b, Vector2 c, Vector2 d, ref Vector2 p)
        {
            double s1, s2, s3, s4;
            int d1, d2, d3, d4;
            d1 = Dblcmp(s1 = Ab_cross_ac(a, b, c), 0);
            d2 = Dblcmp(s2 = Ab_cross_ac(a, b, d), 0);
            d3 = Dblcmp(s3 = Ab_cross_ac(c, d, a), 0);
            d4 = Dblcmp(s4 = Ab_cross_ac(c, d, b), 0);
            if ((d1 ^ d2) == -2 && (d3 ^ d4) == -2)
            {
                p.x = (float)(((double)c.x * s2 - (double)d.x * s1) / (s2 - s1));
                p.y = (float)(((double)c.y * s2 - (double)d.y * s1) / (s2 - s1));
                return 1;
            }
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
            return -1;
        }

        public bool Check(Vector2 point)
        {
            if (!enable || skip)
                return false;
            var allPoints = ploy.allPoints;
            if (ploy == null || allPoints.Count <= 2)
                return false;
            if (!CheckBox(point))
                return false;
            int polygonLength = allPoints.Count;
            int i = 0;
            bool inside = false;
            float pointX = point.x;
            float pointY = point.y;
            float startX, startY, endX, endY;
            Vector2 endPoint = allPoints[polygonLength - 1];
            endX = endPoint.x;
            endY = endPoint.y;
            while (i < polygonLength)
            {
                startX = endX;
                startY = endY;
                endPoint = allPoints[i++];
                endX = endPoint.x;
                endY = endPoint.y;
                inside ^= (endY >= pointY ^ startY >= pointY) && ((pointX - endX) <= (pointY - endY) * (startX - endX) / (startY - endY));
            }
            return inside;
        }

        private float SimpleDistance(Vector2 a, Vector2 b)
        {
            return (a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y);
        }

        public bool CheckBox(Vector2 pt)
        {
            if (Rect.Contains(pt))
                return true;
            return false;
        }

        public bool CheckEnable(Vector2 pt)
        {
            if (Rect.xMin > pt.x + 1000 || Rect.xMax < pt.x - 1000
                || Rect.yMin > pt.y + 1000 || Rect.yMax < pt.y - 1000)
                enable = false;
            else
                enable = true;
            return enable;
        }

        public bool CheckBox(Vector2 pt1, Vector2 pt2)
        {
            if (!enable || skip)
                return false;
            mTargetRect.x = Math.Min(pt1.x, pt2.x);
            mTargetRect.y = Math.Min(pt1.y, pt2.y);
            mTargetRect.width = Mathf.Max(Math.Abs(pt1.x - pt2.x), 1);
            mTargetRect.height = Mathf.Max(Math.Abs(pt1.y - pt2.y), 1);
            return Rect.Overlaps(mTargetRect);
        }

        public bool Check(Vector2 from, Vector2 to, ref Vector2 cross, ref int distance, bool bAutoFixPos = false)
        {
            //不檢查跳出
            if (!enable || skip)
                return false;

            //不存在poly 或是 poly小於2點(不成3角面) 跳出
            if (ploy == null || ploy.allPoints.Count <= 2)
                return false;

            var allPoints = ploy.allPoints;
            Vector2 lastpt = from;
            Vector2 checkpt = Vector2.zero;
            Vector2 startCheckpt = Vector2.zero;
            Vector2 endCheckpt = Vector2.zero;
            bool bfindPoint = false;
            sCrossList.Clear();

            for (int i = 0; i < allPoints.Count; i++)
            {
                startCheckpt = allPoints[i];
                endCheckpt = (i == allPoints.Count - 1) ? allPoints[0] : allPoints[i + 1];
                int ret = Ab_cross_cd(from, to, startCheckpt, endCheckpt, ref checkpt);
                if (ret >= 0)
                {
                    sCrossList.Add(new System.Tuple<float, int, Vector2>(Vector2.Distance(from, checkpt), i, checkpt));
                }
            }

            if (sCrossList.Count > 0)
            {
                sCrossList.Sort((a, b) => a.Item1.CompareTo(b.Item2));
                if (sCrossList.Count >= 2)
                {
                    if (Math.Abs(sCrossList[sCrossList.Count - 1].Item1 - sCrossList[0].Item1) < 15)
                        return false;
                }
                int index = sCrossList[0].Item2;
                startCheckpt = allPoints[index];
                endCheckpt = (index == allPoints.Count - 1) ? allPoints[0] : allPoints[index + 1];
                if (checkpt.Equals(from))
                {
                    bfindPoint = true;
                    distance = 0;
                    lastpt = from;
                }
                else
                {
                    bool bfind = false;
                    if (bAutoFixPos)
                    {
                        checkpt = GetFixPointInCrossLine(startCheckpt, endCheckpt, checkpt, from, Vector2.Distance(from, to));
                    }
                    for (int j = 9; j > 0; j--)
                    {
                        Vector2 checkTargetpt = Vector2.Lerp(from, checkpt, (float)j / 10.0f);
                        if (DistanceLine(startCheckpt, endCheckpt, checkTargetpt) > 5)
                        {
                            int newdistance = (int)SimpleDistance(from, checkTargetpt);
                            if (newdistance > 0)
                            {
                                if (newdistance < distance)
                                {
                                    distance = newdistance;
                                    lastpt = checkTargetpt;
                                }
                                bfindPoint = true;
                                bfind = true;
                                break;
                            }
                        }
                    }
                    if (!bfind && !bfindPoint)
                    {
                        bfindPoint = true;
                        distance = 0;
                        lastpt = from;
                    }
                }
            }

            if (bfindPoint)
            {
                cross = lastpt;
                return true;
            }
            return false;
        }

        public static Vector2 GetFixPointInCrossLine(Vector2 a, Vector2 b, Vector2 c, Vector2 s, float distance)
        {
            Vector2 bc = b - c;
            Vector2 sc = s - c;
            float f = Vector2.Dot(bc, sc);
            Vector2 targetPt = a;
            if (f < 0)
            {
                targetPt = b;
            }
            float lineD = Vector2.Distance(c, targetPt);
            int amount = (int)Math.Ceiling(lineD / 5);
            for (int i = 1; i <= amount; i++)
            {
                Vector2 check = Vector2.Lerp(c, targetPt, i / lineD);
                if (Vector2.Distance(check, s) >= distance)
                {
                    return check;
                }
            }
            return targetPt;
        }

        public static float DistanceLine(Vector2 p, Vector2 q, Vector2 pt, bool bCheckLine = false)
        {
            float pqx = q.x - p.x;
            float pqy = q.y - p.y;
            float dx = pt.x - p.x;
            float dy = pt.y - p.y;
            float d = pqx * pqx + pqy * pqy;
            float t = pqx * dx + pqy * dy;
            if (d > 0)
                t /= d;
            if (!bCheckLine)
            {
                if (t < 0)
                    t = 0;
                else if (t > 1)
                    t = 1;
            }
            dx = p.x + t * pqx - pt.x;
            dy = p.y + t * pqy - pt.y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

    }
}