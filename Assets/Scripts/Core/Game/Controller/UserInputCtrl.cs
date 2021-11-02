using Core.Framework.Utility;
using Core.Game.Logic;
using Core.Game.Network;
using Core.Game.Scene;
using Core.Game.UI.Widget;
using Core.Game.Utility;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Core.Game.Controller
{
    public class UserInputCtrl : MonoBehaviour
    {
        public Joystick joystick;
        public Logic.Sprite leader;
        public List<float> tryFindAngle = new List<float>() { 45, -45 };

        private float mLastHandleUserInputTime;
        private Stopwatch mRushTimer = new Stopwatch();

        public void HandleUserInput()
        {
            if(joystick == null)
            {
                joystick = FindObjectOfType<Joystick>();
                if (joystick == null)
                    return;
            }

            if (Time.time < mLastHandleUserInputTime + Define.HandleUserInputInterval)
            {
                return;
            }

            var mainCamera = Camera.main;
            if (mainCamera == null)
                return;

            if (leader != null && leader.IsReady)
            {
                mLastHandleUserInputTime = Time.time;
                // TODO

                var dir = joystick.Value;
                HandleKeyboardInput(ref dir);

                var joystickDirDirV3 = dir.ToVector3();
                var joystickRotation = Quaternion.FromToRotation(Vector3.forward, joystickDirDirV3);
                var len = dir.magnitude;

                var cameraForward = mainCamera.transform.forward;
                cameraForward.y = 0;
                var cameraRotation = Quaternion.FromToRotation(Vector3.forward, cameraForward);

                if(!leader.CanBreakActionByUserInput())
                {
                    return;
                }

                var preAction = leader.Action;
                var preMoveSpeed = leader.MoveSpeed;
                var totalRotation = cameraRotation * joystickRotation;
                if (!Mathf.Approximately(len, 0))
                {
                    leader.Rotation = totalRotation;
                    leader.SetMoveSpeed(Define.RunSpeed, true);
                }
                else
                {
                    leader.SetMoveSpeed(0);
                }
                if(preMoveSpeed != leader.MoveSpeed)
                {
                    if (leader.MoveSpeed == 0)
                        SendMsgHelper.SendRunStatus(0);
                    else if (leader.MoveSpeed == Define.RunSpeed)
                        SendMsgHelper.SendRunStatus(1);
                }
                if(preAction == ActionType.Rush && leader.Action == ActionType.RushEnd)
                {
                    SendMsgHelper.SendMoveEnd(GameScene.Instance.MapId, leader);
                }

                var finalDirV3 = totalRotation * Vector3.forward;
                var finalDirV2 = finalDirV3.ToVector2IgnoreY();
                finalDirV2 = finalDirV2.normalized;

                if (len > Define.RushThreshold)
                {
                    if (!mRushTimer.IsRunning)
                        mRushTimer.Start();
                    if (mRushTimer.IsRunning && mRushTimer.Elapsed.TotalSeconds > 2)
                        leader.StartRush();
                }
                else
                {
                    mRushTimer.Stop();
                    mRushTimer.Reset();
                }

                if (len < 0 || Mathf.Approximately(len, 0))
                    return;

                var dest = leader.Coordinate + finalDirV2 * Define.PositionScale;
                if(GameUtil.CheckHeightDiff(leader.GameObject.transform.position.y, leader.Coordinate, dest, out var stopPos))
                {
                    if(!TryFindNewDir(finalDirV2, out var newDir))
                    {
                        return;
                    }
                    dest = leader.Coordinate + newDir * Define.PositionScale;
                }

                //LogUtil.Debug.LogError($"cur={leader.Coordinate} dest={dest} len={(dest-leader.Coordinate).magnitude} speed={leader.MoveSpeed}");
                SceneObjectMovingManager.Instance.StartMoving("Leader UserInput", leader, Vector2Int.FloorToInt(dest), true,
                    (obj)=>
                    {
                        leader.SetMoveSpeed(0, true);
                    });
                SendMsgHelper.SendMoveStart(GameScene.Instance.MapId, leader.Coordinate, Vector2Int.FloorToInt(dest));
            }
        }

        public bool IsUserHolding()
        {
            if (joystick == null)
            {
                joystick = FindObjectOfType<Joystick>();
                if (joystick == null)
                    return false;
            }
            var dir = joystick.Value;
            HandleKeyboardInput(ref dir);
            if (Mathf.Approximately(dir.sqrMagnitude, 0))
                return false;
            return true;
        }

        public Vector3 GetCurrentDirection()
        {
            if (joystick == null)
                return Vector3.zero;
            var mainCamera = Camera.main;
            if(mainCamera == null)
                return Vector3.zero;

            var dir = joystick.Value;

            var joystickDirDirV3 = dir.ToVector3();
            var joystickRotation = Quaternion.FromToRotation(Vector3.forward, joystickDirDirV3);

            var cameraForward = mainCamera.transform.forward;
            cameraForward.y = 0;
            var cameraRotation = Quaternion.FromToRotation(Vector3.forward, cameraForward);
            var totalRotation = cameraRotation * joystickRotation;
            var finalDirV3 = totalRotation * Vector3.forward;
            return finalDirV3;
        }

        private void HandleKeyboardInput(ref Vector2 dir)
        {
            bool w = Input.GetKey(KeyCode.W);
            bool a = Input.GetKey(KeyCode.A);
            bool s = Input.GetKey(KeyCode.S);
            bool d = Input.GetKey(KeyCode.D);
            bool w_a = w && a;
            bool a_s = a && s;
            bool s_d = s && d;
            bool d_w = d && w;
            if (w_a)
                dir = (Vector2.up + Vector2.left).normalized;
            else if(a_s)
                dir = (Vector2.down + Vector2.left).normalized;
            else if (s_d)
                dir = (Vector2.down + Vector2.right).normalized;
            else if (d_w)
                dir = (Vector2.up + Vector2.right).normalized;
            else if (w)
                dir = Vector2.up;
            else if (a)
                dir = Vector2.left;
            else if (s)
                dir = Vector2.down;
            else if (d)
                dir = Vector2.right;
        }

        public bool TryFindNewDir(Vector2 dir, out Vector2 newDir)
        {
            var dirV3 = dir.ToVector3();
            var rotate = Quaternion.AngleAxis(45, Vector3.up);
            var newDirV3 = rotate * dirV3;
            newDir = newDirV3.ToVector2IgnoreY();
            var newDest = leader.Coordinate + newDir * Define.PositionScale;
            if (!GameUtil.CheckHeightDiff(leader.GameObject.transform.position.y, leader.Coordinate, newDest, out var stopPos))
            {
                return true;
            }

            rotate = Quaternion.AngleAxis(-45, Vector3.up);
            newDirV3 = rotate * dirV3;
            newDir = newDirV3.ToVector2IgnoreY();
            newDest = leader.Coordinate + newDir * Define.PositionScale;
            if (!GameUtil.CheckHeightDiff(leader.GameObject.transform.position.y, leader.Coordinate, newDest, out stopPos))
            {
                return true;
            }

            return false;
        }
    }
}