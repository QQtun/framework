using Core.Framework.Utility;
using Core.Game.Logic;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Game.Utility
{
    public class SceneObjectMovingManager : Singleton<SceneObjectMovingManager>
    {

        private List<MovingRequest> mAllReqs = new List<MovingRequest>();
        private Dictionary<ISceneObject, MovingRequest> mObjToMovingReqDic = new Dictionary<ISceneObject, MovingRequest>();
        private Queue<MovingRequest> mPool = new Queue<MovingRequest>();

        public void Cancel(ISceneObject obj)
        {
            if (mObjToMovingReqDic.TryGetValue(obj, out var req))
            {
                mPool.Enqueue(req);
                mObjToMovingReqDic.Remove(obj);
                mAllReqs.Remove(req);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="dest"></param>
        /// <param name="speed"></param>
        /// <param name="delay">單位s</param>
        /// <param name="onCompleted"></param>
        public void StartMoving(string name, ISceneObject obj, Vector2Int dest, 
            float speed, float delay = 0f, bool faceMovementDir = true, Action<ISceneObject> onCompleted = null)
        {
            if (obj.Coordinate == dest)
                return;

            StartMoving(name, obj, new List<Vector2Int>() { dest }, dest, speed, delay, faceMovementDir, onCompleted);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="path"></param>
        /// <param name="dest"></param>
        /// <param name="speed"></param>
        /// <param name="delay">單位s</param>
        /// <param name="onCompleted"></param>
        public void StartMoving(string name, ISceneObject obj, List<Vector2Int> path, Vector2Int dest, 
            float speed, float delay = 0f, bool faceMovementDir = true, Action<ISceneObject> onCompleted = null)
        {
            Cancel(obj);

            if (path.Count == 0)
                return;

            var req = AllocReq();
            req.name = name;
            req.obj = obj;
            req.path = path;
            req.dest = dest;
            req.tagetPathIndex = 0;
            req.curPos = obj.Coordinate;
            req.onCompleted = onCompleted;
            req.useObjSpeed = false;
            req.speed = speed;
            req.startTime = Time.time + delay;
            req.faceMovementDir = faceMovementDir;

            mObjToMovingReqDic.Add(obj, req);
            mAllReqs.Add(req);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="dest">伺服器單位</param>
        /// <param name="onCompleted"></param>
        public void StartMoving(string name, ISceneObject obj, Vector2Int dest, 
            bool faceMovementDir = true, Action<ISceneObject> onCompleted = null)
        {
            if (obj.Coordinate == dest)
                return;

            StartMoving(name, obj, new List<Vector2Int>() { dest }, dest, faceMovementDir, onCompleted);
        }

        public void StartMoving(string name, ISceneObject obj, List<Vector2Int> path, Vector2Int dest, 
            bool faceMovementDir = true, Action<ISceneObject> onCompleted = null)
        {
            Cancel(obj);

            //if (path.Count > 0)
            //{
            //    var curGrid = new Vector2Int(obj.Coordinate.x / 100, obj.Coordinate.y / 100);
            //    var firstPoint = path[0];
            //    var firstNodeGrid = new Vector2Int(firstPoint.x / 100, firstPoint.y / 100);
            //    if (curGrid == firstNodeGrid)
            //    {
            //        var lastPoint = path[path.Count - 1];
            //        if (Vector2IntEx.SqrDistance(obj.Coordinate, lastPoint)
            //            <= Vector2IntEx.SqrDistance(firstPoint, lastPoint))
            //        {
            //            path.RemoveAt(0);
            //        }
            //    }
            //}

            if (path.Count == 0)
                return;

            var req = AllocReq();
            req.name = name;
            req.obj = obj;
            req.path = path;
            req.dest = dest;
            req.tagetPathIndex = 0;
            req.curPos = obj.Coordinate;
            req.onCompleted = onCompleted;
            req.useObjSpeed = true;
            req.speed = 0;
            req.startTime = Time.time;
            req.faceMovementDir = faceMovementDir;

            mObjToMovingReqDic.Add(obj, req);
            mAllReqs.Add(req);
        }

        private MovingRequest AllocReq()
        {
            if (mPool.Count > 0)
                return mPool.Dequeue();
            return new MovingRequest();
        }

        private void Update()
        {
            for (int i = mAllReqs.Count - 1; i >= 0; i--)
            {
                mAllReqs[i].Update();
            }
        }
        private class MovingRequest
        {
            public string name;
            public ISceneObject obj;
            public List<Vector2Int> path;
            public Vector2Int dest;
            public bool useObjSpeed;
            public float speed;
            public float startTime;
            public Action<ISceneObject> onCompleted;
            public bool faceMovementDir;

            public int tagetPathIndex;
            public Vector2 curPos;

            public void Update()
            {
                if (Time.time < startTime)
                    return;

                var dt = Time.deltaTime;
                var sp = speed;
                if (useObjSpeed)
                    sp = obj.MoveSpeed;
                var moveDis = dt * (sp / 100f) * Define.PositionScale * Define.BasicModeSpeed;
                if (StepMove(moveDis))
                {
                    onCompleted?.Invoke(obj);
                    Instance.Cancel(obj);
                }
            }

            private bool StepMove(double moveDis)
            {
                var targetWayPoint = path[tagetPathIndex];
                var dir = targetWayPoint - curPos;
                float distanceToWayPoint = dir.magnitude;
                var dirV3 = dir.ToVector3();
                if (Mathf.Approximately(distanceToWayPoint, 0))
                    return true;

                bool toNextWayPoint = distanceToWayPoint < moveDis;
                Vector2 targetPos;
                if (!toNextWayPoint)
                {
                    targetPos = curPos + (dir.normalized * (float)moveDis);
                }
                else
                {
                    targetPos = targetWayPoint;
                }

                //if (obj.SpriteType == SpriteType.Leader)
                //    Debug.LogError($"curPos={curPos} targetWayPoint={targetWayPoint} dir={dir} distanceToWayPoint={distanceToWayPoint} moveDis={moveDis} toNextWayPoint={toNextWayPoint}");
                if (GameUtil.CheckHeightDiff(obj.GameObject.transform.position.y, curPos, targetPos, out var stopPos))
                {
                    curPos = stopPos;
                    if (faceMovementDir)
                        obj.Rotation = Quaternion.LookRotation(dirV3);
                    obj.Coordinate = Vector2Int.FloorToInt(curPos);
                    return true;
                }

                if (!toNextWayPoint)
                {
                    if (tagetPathIndex >= path.Count - 1 && Mathf.Approximately((targetPos - dest).magnitude, 0))
                    {
                        // 路徑最後一個點 && 已接近終點
                        curPos = dest;
                        if(faceMovementDir)
                            obj.Rotation = Quaternion.LookRotation(dirV3);
                        obj.Coordinate = dest;
                        //obj.SetMoveSpeed(0);
                        return true;
                    }
                    else
                    {
                        curPos = targetPos;
                        if (faceMovementDir)
                            obj.Rotation = Quaternion.LookRotation(dirV3);
                        obj.Coordinate = Vector2Int.FloorToInt(curPos);
                        return false;
                    }
                }
                else
                {
                    tagetPathIndex++;
                    if (tagetPathIndex >= path.Count)
                    {
                        // 路徑最後一個點
                        curPos = dest;
                        if (faceMovementDir)
                            obj.Rotation = Quaternion.LookRotation(dirV3);
                        obj.Coordinate = dest;
                        //obj.SetMoveSpeed(0);
                        //if(obj.SpriteType == SpriteType.Leader)
                        //{
                        //    Debug.LogError($"name={name} ");
                        //}
                        return true;
                    }
                    moveDis -= distanceToWayPoint;
                    curPos = targetWayPoint;
                    return StepMove(moveDis);
                }
            }
        }
    }
}