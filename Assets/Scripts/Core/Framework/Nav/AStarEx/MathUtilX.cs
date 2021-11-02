using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Core.Framework.Nav.AStarEx
{
    public delegate float CalcLineHandler(float value);
    public static class MathUtilX
    {
        public static CalcLineHandler getLineFunc(Vector2 ponit1, Vector2 point2, int type = 0)
        {
            CalcLineHandler resultFuc = null;
            if (ponit1.x == point2.x)
            {
                if (type == 0)
                {
                    throw new Exception("两点所确定直线垂直于y轴，不能根据x值得到y值");
                }
                else if (type == 1)
                {
                    resultFuc = (float y) =>
                                {
                                    return ponit1.x;
                                };
                }
                return resultFuc;
            }
            else if (ponit1.y == point2.y)
            {
                if (type == 0)
                {
                    resultFuc = (float y) =>
                    {
                        return ponit1.y;
                    };
                }
                else if (type == 1)
                {
                    throw new Exception("两点所确定直线垂直于y轴，不能根据x值得到y值");
                }
                return resultFuc;
            }
            float a;
            a = (ponit1.y - point2.y) / (ponit1.x - point2.x);
            float b;
            b = ponit1.y - a * ponit1.x;
            if (type == 0)
            {
                resultFuc = (float x) =>
                            {
                                return a * x + b;
                            };
            }
            else if (type == 1)
            {
                resultFuc = (float y) =>
                {
                    return (y - b) / a;
                };
            }
            return resultFuc;
        }
        public static float getSlope(Vector2 ponit1, Vector2 point2)
        {
            return (point2.y - ponit1.y) / (point2.x - ponit1.x);
        }
    }
}
