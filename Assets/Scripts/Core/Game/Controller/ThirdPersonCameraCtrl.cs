using Core.Framework.Utility;
using Lean.Touch;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Game.Controller
{
    public class ThirdPersonCameraCtrl : MonoBehaviour
    {
        private enum State
        {
            None,
            WaitUserFingerMove,
            Vertial,
            Horizontal,
            FallowBack,
            WaitFallowBack,
            MoveToBack,
            Pinch,
        }

        public float fingerMoveThreshold = 10;
        public float horizontalMultiplier = 1;
        public float vertialMultiplier = 1;
        public float maxVerticalAngle = 90;
        public float minVerticalAngle = 0;
        public float pinchMutiplier = 1;
        public float maxDistance = 20;
        public float minDistance = 10;
        public float fallowBackRotationAngle = 60;
        public float moveBackRotationAngle = 360;

        [SerializeField]
        private bool mIgnoreInput;
        private Transform mTrans;
        private FiniteState<State> mState = new FiniteState<State>(State.FallowBack);
        [SerializeField]
        private Transform mFallowTaget;
        private LeanFinger mCurFinger;
        private Vector3 mLastTargetPos;

        public bool IgnoreInput
        {
            get => mIgnoreInput;
            set => mIgnoreInput = value;
        }

        public Transform FallowTaget
        {
            get => mFallowTaget;
            set
            {
                mFallowTaget = value;
                mLastTargetPos = mFallowTaget.position;
                mState.Transit(State.FallowBack);
            }
        }

        public void StartFallow(Transform transform)
        {
            FallowTaget = transform;

            var dir = mFallowTaget.position - mTrans.position;
            var distance = dir.magnitude;
            var dirY0 = dir;
            dirY0.y = 0;
            var angle = Vector3.Angle(dirY0, dir);
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
            mTrans.position = mFallowTaget.position + (-dir).normalized * distance;
            angle = Mathf.Clamp(angle, minVerticalAngle, maxVerticalAngle);
            var q = Quaternion.AngleAxis(-angle, Vector3.Cross(dir, dirY0));
            mTrans.rotation = q;
        }

        public void MoveToBack()
        {
            mState.Transit(State.MoveToBack);
        }

        private void Awake()
        {
            mTrans = transform;
            if(mFallowTaget != null)
                mLastTargetPos = mFallowTaget.position;
        }

        private void OnEnable()
        {
            LeanTouch.OnFingerDown += OnFingerDown;
            LeanTouch.OnFingerUpdate += OnFingerUpdate;
            LeanTouch.OnFingerUp += OnFingerUp;
            LeanTouch.OnGesture += OnGesture;
        }

        private void OnDisable()
        {
            LeanTouch.OnFingerDown -= OnFingerDown;
            LeanTouch.OnFingerUpdate -= OnFingerUpdate;
            LeanTouch.OnFingerUp -= OnFingerUp;
            LeanTouch.OnGesture -= OnGesture;
        }

        private void LateUpdate()
        {
            switch (mState.Tick())
            {
                case State.None:
                    break;
                case State.FallowBack:
                {
                    FallowTargetPos();

                    if (mFallowTaget == null)
                        return;

                    var cameraForward = mTrans.forward;
                    cameraForward.y = 0;
                    var targetForward = mFallowTaget.forward;
                    targetForward.y = 0;

                    var angleDir = Vector3.Dot(Vector3.up, Vector3.Cross(cameraForward, targetForward)) < 0 ? -1 : 1;
                    var angle = Vector3.Angle(cameraForward, targetForward);
                    if (angle > 30)
                    {
                        var cameraPos = mTrans.position;
                        var y = cameraPos.y;
                        cameraPos.y = 0;
                        var targetPos = FallowTaget.position;
                        targetPos.y = 0;
                        var dir = cameraPos - targetPos;
                        var q = Quaternion.AngleAxis(angleDir * fallowBackRotationAngle * Time.deltaTime * horizontalMultiplier, Vector3.up);
                        var newDir = q * dir;
                        newDir.y = y;
                        var newCameraPos = targetPos + newDir;
                        mTrans.position = newCameraPos;
                        mTrans.rotation = Quaternion.LookRotation(-newDir);
                    }
                    break;
                }
                case State.WaitFallowBack:
                {
                    FallowTargetPos();

                    //if (mState.Elapsed > 3)
                    //    mState.Transit(State.FallowBack);
                    break;
                }
                case State.Horizontal:
                {
                    FallowTargetPos();

                    if (mCurFinger == null)
                        return;
                    if (FallowTaget == null)
                        return;

                    var cameraPos = mTrans.position;
                    var y = cameraPos.y;
                    cameraPos.y = 0;
                    var targetPos = FallowTaget.position;
                    targetPos.y = 0;
                    var dir = cameraPos - targetPos;
                    var deltaX = mCurFinger.ScreenDelta.x / Screen.width * 100; // 螢幕百分比
                    var q = Quaternion.AngleAxis(deltaX * horizontalMultiplier, Vector3.up);
                    var newDir = q * dir;
                    newDir.y = y;
                    var newCameraPos = targetPos + newDir;
                    mTrans.position = newCameraPos;
                    mTrans.rotation = Quaternion.LookRotation(-newDir);
                    break;
                }
                case State.Vertial:
                {
                    FallowTargetPos();

                    if (mCurFinger == null)
                        return;
                    if (FallowTaget == null)
                        return;

                    var cameraPos = mTrans.position;
                    var targetPos = FallowTaget.position;
                    var dir = cameraPos - targetPos;
                    var dirY0 = dir;
                    dirY0.y = 0;
                    dirY0 = dirY0.normalized * dir.magnitude;
                    var temp = dir;
                    temp.y += 10;
                    var deltaY = mCurFinger.ScreenDelta.y / Screen.height * 100; // 螢幕百分比
                    var q = Quaternion.AngleAxis(deltaY * vertialMultiplier, Vector3.Cross(temp, dir));
                    var newDir = q * dir;

                    var angle = Vector3.Angle(newDir, dirY0);
                    if (angle > maxVerticalAngle)
                    {
                        q = Quaternion.AngleAxis(-maxVerticalAngle, Vector3.Cross(temp, dir));
                        newDir = q * dirY0;
                    }
                    else if(angle < minVerticalAngle)
                    {
                        q = Quaternion.AngleAxis(-minVerticalAngle, Vector3.Cross(temp, dir));
                        newDir = q * dirY0;
                    }

                    var newCameraPos = targetPos + newDir;
                    mTrans.position = newCameraPos;
                    mTrans.rotation = Quaternion.LookRotation(-newDir);
                    break;
                }
                case State.Pinch:
                {
                    FallowTargetPos();

                    if (FallowTaget == null)
                        return;

                    var cameraPos = mTrans.position;
                    var targetPos = FallowTaget.position;
                    var dir = cameraPos - targetPos;
                    var dirNormal = dir.normalized;
                    var ratio = LeanGesture.GetPinchRatio();
                    var distance = LeanGesture.GetScreenDistance();
                    var lastDistance = LeanGesture.GetLastScreenDistance();
                    var delta = Mathf.Abs(lastDistance - distance);
                    delta = delta / new Vector2(Screen.width, Screen.height).magnitude * 100; // 螢幕百分比
                    if (ratio > 1)
                    {
                        // 縮小
                        mTrans.position += dirNormal * delta * pinchMutiplier;
                    }
                    else
                    {
                        // 放大
                        mTrans.position += -dirNormal * delta * pinchMutiplier;
                    }

                    var newDir = mTrans.position - targetPos;
                    if(newDir.magnitude > maxDistance)
                    {
                        mTrans.position = targetPos + newDir.normalized * maxDistance;
                    }
                    else if(newDir.magnitude < minDistance)
                    {
                        mTrans.position = targetPos + newDir.normalized * minDistance;

                    }
                    break;
                }
                case State.MoveToBack:
                {
                    FallowTargetPos();

                    if (mFallowTaget == null)
                        return;

                    var cameraForward = mTrans.forward;
                    cameraForward.y = 0;
                    var targetForward = mFallowTaget.forward;
                    targetForward.y = 0;

                    var angleDir = Vector3.Dot(Vector3.up, Vector3.Cross(cameraForward, targetForward)) < 0 ? -1 : 1;
                    var angle = Vector3.Angle(cameraForward, targetForward);
                    if (Mathf.Abs(angle) > 1)
                    {
                        var cameraPos = mTrans.position;
                        var y = cameraPos.y;
                        cameraPos.y = 0;
                        var targetPos = FallowTaget.position;
                        targetPos.y = 0;
                        var dir = cameraPos - targetPos;
                        var angleNow = Mathf.Min(Mathf.Abs(moveBackRotationAngle * Time.deltaTime * horizontalMultiplier), Mathf.Abs(angle));
                        var q = Quaternion.AngleAxis(angleDir * angleNow, Vector3.up);
                        var newDir = q * dir;
                        newDir.y = y;
                        var newCameraPos = targetPos + newDir;
                        mTrans.position = newCameraPos;
                        mTrans.rotation = Quaternion.LookRotation(-newDir);
                    }
                    else
                    {
                        mState.Transit(State.WaitFallowBack);
                    }
                    break;
                }
            }
        }

        private void OnFingerUp(LeanFinger finger)
        {
            if (LeanTouch.Fingers.Count == 0 
                || (mCurFinger != null && finger.Index == mCurFinger.Index))
            {
                mState.Transit(State.WaitFallowBack);
                mCurFinger = null;
            }
        }

        private void OnFingerUpdate(LeanFinger finger)
        {
            if (mCurFinger != null && finger.Index != mCurFinger.Index)
                return;

            if(mState.Current == State.WaitUserFingerMove)
            {
                var delta = finger.SwipeScreenDelta;
                if (delta.sqrMagnitude < fingerMoveThreshold)
                    return;
                if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                    mState.Transit(State.Horizontal);
                else
                    mState.Transit(State.Vertial);
            }
        }

        private void OnFingerDown(LeanFinger finger)
        {
            if (LeanTouch.Fingers.Count > 1)
                return;
            if (finger.StartedOverGui)
                return;
            mState.Transit(State.WaitUserFingerMove);
            mCurFinger = finger;
        }

        private void OnGesture(List<LeanFinger> finger)
        {
            if (finger.Count != 2)
                return;
            if (finger[0].StartedOverGui || finger[1].StartedOverGui)
                return;

            if (mState.Current == State.Vertial
                || mState.Current == State.Horizontal)
                return;

            mState.Transit(State.Pinch);
        }

        private void FallowTargetPos()
        {
            if (FallowTaget == null)
                return;

            var targetPos = FallowTaget.position;
            if (Mathf.Approximately((targetPos - mLastTargetPos).sqrMagnitude, 0))
                return;

            var dir = targetPos - mLastTargetPos;
            mTrans.position += dir;
            mLastTargetPos = targetPos;
        }
    }
}