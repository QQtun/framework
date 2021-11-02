using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Framework.Utility
{
    [Serializable]
    public class MultilayerBounds
    {
        [Serializable]
        public struct Layer
        {
            public float y;
            public Vector2 center;
            public Vector2 extend;
        }

        public bool enableDebug;

        [SerializeField]
        private List<Layer> _layerBounds = new List<Layer>();

        public IReadOnlyList<Layer> LayerBounds { get { return _layerBounds; } }

        /// <summary>
        ///     取最接近範圍的點，若在範圍內則不變
        /// </summary>
        /// <param name="point">目標點</param>
        /// <param name="closestPoint">最近的點</param>
        /// <returns></returns>
        public bool ClosestPoint(Vector3 point, out Vector3 closestPoint)
        {
            if (_layerBounds == null || _layerBounds.Count == 0)
            {
                closestPoint = point;
                return false;
            }

            if (_layerBounds.Count == 1)
            {
                // 只有一層
                var layer = _layerBounds[0];
                var b = new Bounds(
                    new Vector3(layer.center.x, layer.y, layer.center.y),
                    new Vector3(layer.extend.x * 2, 0, layer.extend.y * 2));
                closestPoint = b.ClosestPoint(point);
                return true;
            }

            Layer bLayer = _layerBounds[0];
            Layer tLayer = bLayer;
            if (point.y > bLayer.y)
            {
                for (var i = 1; i < _layerBounds.Count; i++)
                {
                    tLayer = _layerBounds[i];
                    if (tLayer.y >= point.y && point.y >= bLayer.y)
                    {
                        break;
                    }
                    bLayer = tLayer;
                }
            }

            if (Math.Abs(bLayer.y - tLayer.y) < 0.0001f)
            {
                // 低於最底層 或 高於最上層
                var b = new Bounds(
                    new Vector3(bLayer.center.x, bLayer.y, bLayer.center.y),
                    new Vector3(bLayer.extend.x * 2, 0, bLayer.extend.y * 2));
                closestPoint = b.ClosestPoint(point);
            }
            else
            {
                var bBounds = new Bounds(
                    new Vector3(bLayer.center.x, bLayer.y, bLayer.center.y),
                    new Vector3(bLayer.extend.x * 2, 0, bLayer.extend.y * 2));

                var tBounds = new Bounds(
                    new Vector3(tLayer.center.x, tLayer.y, tLayer.center.y),
                    new Vector3(tLayer.extend.x * 2, 0, tLayer.extend.y * 2));

                Vector3 bMax = bBounds.max;
                Vector3 bMin = bBounds.min;
                Vector3 tMax = tBounds.max;
                Vector3 tMin = tBounds.min;

                var lineMax = new KeyValuePair<Vector3, Vector3>(bMax, tMax);
                var lineMin = new KeyValuePair<Vector3, Vector3>(bMin, tMin);

                // point所在平面在錐形內的中心點
                var center = (point.y - bLayer.y) / (tLayer.y - bLayer.y) * (tBounds.center - bBounds.center) + bBounds.center;

                GetIntersectWithLineAndPlane(lineMax.Key, lineMax.Key - lineMax.Value, Vector3.up, center, out var p1);
                GetIntersectWithLineAndPlane(lineMin.Key, lineMin.Key - lineMin.Value, Vector3.up, center, out var p2);

                var bp = new Bounds((p1 + p2) / 2, (p1 - p2)); // point所在平面的bounds
                closestPoint = bp.ClosestPoint(point);
            }
            return true;
        }

        /// <summary>
        ///     求直線與平面的交點
        /// </summary>
        /// <param name="point">直線上某一點</param>
        /// <param name="direct">直線的方向</param>
        /// <param name="planeNormal">平面的法向量</param>
        /// <param name="planePoint">平面上的任意一點</param>
        /// <param name="crossPoint">交點</param>
        /// <returns>是否有交點</returns>
        public bool GetIntersectWithLineAndPlane(Vector3 point, Vector3 direct, Vector3 planeNormal, Vector3 planePoint, out Vector3 crossPoint)
        {
            float d2 = Vector3.Dot(direct.normalized, planeNormal);
            if (Math.Abs(d2) < 0.00001f)
            {
                crossPoint = Vector3.zero;
                return false;
            }
            float d = Vector3.Dot(planePoint - point, planeNormal) / d2;
            crossPoint = d * direct.normalized + point;
            return true;
        }

        /// <summary>
        ///     point是否在四邊形內
        /// </summary>
        /// <param name="point4">四邊形的四個頂點，必須順時或逆時</param>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool IsInside(List<Vector3> point4, Vector3 point)
        {
            float degree = 0;
            for (var i = 0; i < point4.Count; i++)
            {
                Vector3 p1, p2;
                if (i == point4.Count - 1)
                {
                    p1 = point4[i];
                    p2 = point4[0];
                }
                else
                {
                    p1 = point4[i];
                    p2 = point4[i + 1];
                }

                float d = Vector3.Angle(p1 - point, p2 - point);
                if (Math.Abs(d) < 0.00001f)
                {
                    degree += 180;
                }
                else
                {
                    degree += Vector3.Angle(p1 - point, p2 - point);
                }
            }

            return Math.Abs(degree - 360) < 0.00001f;
        }

        public void DebugDraw(Vector3? point = null)
        {
#if !UNITY_EDITOR
            return;
#endif
            if (!enableDebug)
                return;

            var bY = LayerBounds[0].y;
            var b1 = LayerBounds[0].center + LayerBounds[0].extend;
            var b3 = LayerBounds[0].center - LayerBounds[0].extend;
            var b2 = new Vector2(b1.x, b3.y);
            var b4 = new Vector2(b3.x, b1.y);

            var tY = LayerBounds[1].y;
            var t1 = LayerBounds[1].center + LayerBounds[1].extend;
            var t3 = LayerBounds[1].center - LayerBounds[1].extend;
            var t2 = new Vector2(t1.x, t3.y);
            var t4 = new Vector2(t3.x, t1.y);

            UnityEngine.Debug.DrawLine(new Vector3(b1.x, bY, b1.y), new Vector3(b2.x, bY, b2.y));
            UnityEngine.Debug.DrawLine(new Vector3(b2.x, bY, b2.y), new Vector3(b3.x, bY, b3.y));
            UnityEngine.Debug.DrawLine(new Vector3(b3.x, bY, b3.y), new Vector3(b4.x, bY, b4.y));
            UnityEngine.Debug.DrawLine(new Vector3(b4.x, bY, b4.y), new Vector3(b1.x, bY, b1.y));
            UnityEngine.Debug.DrawLine(new Vector3(t1.x, tY, t1.y), new Vector3(t2.x, tY, t2.y));
            UnityEngine.Debug.DrawLine(new Vector3(t2.x, tY, t2.y), new Vector3(t3.x, tY, t3.y));
            UnityEngine.Debug.DrawLine(new Vector3(t3.x, tY, t3.y), new Vector3(t4.x, tY, t4.y));
            UnityEngine.Debug.DrawLine(new Vector3(t4.x, tY, t4.y), new Vector3(t1.x, tY, t1.y));
            UnityEngine.Debug.DrawLine(new Vector3(b1.x, bY, b1.y), new Vector3(t1.x, tY, t1.y));
            UnityEngine.Debug.DrawLine(new Vector3(b2.x, bY, b2.y), new Vector3(t2.x, tY, t2.y));
            UnityEngine.Debug.DrawLine(new Vector3(b3.x, bY, b3.y), new Vector3(t3.x, tY, t3.y));
            UnityEngine.Debug.DrawLine(new Vector3(b4.x, bY, b4.y), new Vector3(t4.x, tY, t4.y));

            if (point.HasValue)
            {
                Vector3 closestPoint;
                ClosestPoint(point.Value, out closestPoint);
                DebugExtension.DebugPoint(closestPoint, Color.red);
                if (closestPoint != point)
                    DebugExtension.DebugPoint(point.Value, Color.blue);
            }
        }
    }
}