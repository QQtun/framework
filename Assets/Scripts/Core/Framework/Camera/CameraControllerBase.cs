using Core.Framework.Utility;
using Lean.Touch;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Core.Framework.Camera
{

    /// <summary>
    /// Author: chengdundeng
    /// Date: 2019/11/27
    /// Desc: 提供基礎的相機移動功能, 手指拖動, 雙指縮放, 指令移動
    /// </summary>
    public class CameraControllerBase : MonoBehaviour
    {
        public enum CameraState
        {
            Idle,
            SpeedDownStart,
            SpeedDown,
            SpeedDownEnd,
            MoveToSmoothlyStart,
            MoveToSmoothly,
            MoveToSmoothlyEnd,
            TurningBackStart,
            TurningBack,
            TurningBackEnd,
            FollowTargetStart,
            FollowTarget,
        }

        public delegate void CameraStateChanged(CameraState state);

        protected enum MovingState
        {
            Idle,
            SpeedDown,
            MoveToSmoothly,
            TurningBack,
            FollowTarget,
        }

        [Serializable]
        public struct RotationConfig
        {
            public float y;
            public Vector3 rotation;
        }

        [Serializable]
        public struct FOVConfig
        {
            public float y;
            public float fov;
        }

        // 主相機
        public UnityEngine.Camera mainCamera;

        // ui相機
        public UnityEngine.Camera uiCamera;

        // 主要控制相機移動的transform
        public Transform cameraTranslateTransform;

        // 主要控制相機轉動的transform
        public Transform cameraRotationTransform;

        // 正常 手指操作 可移動範圍
        public MultilayerBounds translateBound;

        // 雙手縮放, 必須超過這個比率 才有動作
        public float scaleThreshold;

        // 雙手縮放, 手放開時, 返回的最低Y直, 必須是在translateBound的Y之間
        [FormerlySerializedAs("turningBackY")]
        public float turningBackYMin;

        public float turningBackYMax;

        // 雙手縮放, 手放開時, 返回minCameraY的速度, 秒
        public float turningBackSpeed;

        // 單手滑動時的乘數, 1代表與手指同速度
        public Vector3 slideMultiplier = Vector3.one;

        // 指定移動到特定位置的移動速度
        public float smoothMoveSpeed = 15f;

        // 指定移動到特定位置的移動速度曲線
        public AnimationCurve smoothMoveCurve;

        // 單手滑動, 手放開後的最大初速
        public float speedDownMaxStartSpeed;

        // 單手滑動, 手放開後的阻力
        public float speedDownMultiplier;

        // 單手滑動, 手放開後的阻力, 經過多少時間達到 speedDownMultiplier 數值
        public float speedDownDuration;

        // 單手滑動, 手放開後的阻力, 在 speedDownDuration 時機內, 如何增加至 speedDownMultiplier 的曲線
        public AnimationCurve speedDownCurve;

        // 拖移邊緣移動速度
        public float dragEdgeMoveSpeed = 20f;

        // 拖移邊緣判定有效螢幕範圍(百分比)
        [Range(0, 1)]
        public float dragEdgeValidScreenPercent = 0.1f;

        // 相機移動超過指定距離時 發出會通知
        public float notifyOverDistanceThreshold;

        // 忽略使用者input (任何手勢操作都無效)
        public bool ignoreUseInput;

        // 滑動判定, 最後兩幀須要
        public float swipeThreshold;

        // 是否要啟用rotationConfigs的設定
        public bool enableRotation;

        // 根據y值 轉動Camera, y值必須是遞增
        public List<RotationConfig> rotationConfigs = new List<RotationConfig>();

        // 是否要啟用fovConfigs的設定
        public bool enableChangeFOV;

        // 根據y值 調整fov, y值必須是遞增. 主要來自拍片的需求
        public List<FOVConfig> fovConfigs = new List<FOVConfig>();

        public float simulateInterval = 0.016f;

        // 指定相機跟隨的目標 transform cached
        private Transform _followTargetCached;

        private Vector3 _lastNotifyPosition;
        public float MaxCameraBoundY { get; protected set; }  // 正常手指操作, 可移動範圍的最大Y值
        public float MinCameraBoundY { get; protected set; }   // 正常手指操作, 可移動範圍的最低Y值

        // 主相機transform cached
        private Transform _mainCameraTransformCached;

        private FiniteState<MovingState> _movingState = new FiniteState<MovingState>(MovingState.Idle);

        private int? _activeFrame;
        private bool _ignoreUseInputLast;

        private float _deltaTime;
        private float _simulateDeltaTime;
        private Vector3 _startSpeedWhenSpeedDown;
        private Vector3 _curSpeedWhenSpeedDown;
        private Vector3 _moveToTargetPos;
        private Vector3 _moveToStartPos;

        private float _curturningBackY;

        public event CameraStateChanged CameraStateChange;

        protected bool IgnoreUseInputInternal
        {
            get
            {
                if(_activeFrame.HasValue)
                {
                    if (Time.frameCount >= _activeFrame.Value)
                    {
                        _activeFrame = null;
                        return ignoreUseInput;
                    }
                    return _ignoreUseInputLast;
                }
                return ignoreUseInput;
            }
        }

        public Transform MainCameraTransform
        {
            get
            {
                if (_mainCameraTransformCached == null)
                    _mainCameraTransformCached = mainCamera.transform;
                return _mainCameraTransformCached;
            }
        }

        public Transform FollowingTarget
        {
            get { return _followTargetCached; }
        }

        private Vector2 DragEdgeScreenPixel
        {
            get { return new Vector2(Screen.width / 2, Screen.height / 2) * dragEdgeValidScreenPercent; }
        }

        /// <summary>
        ///     使用相機forward, 經過相機位置, 在指定Y時的位置
        /// </summary>
        /// <param name="y"></param>
        /// <returns></returns>
        public Vector3 GetCameraForwardAtY(float y)
        {
            return GetCameraForwardAtY(cameraTranslateTransform.position, y);
        }

        /// <summary>
        ///     針對特定高度，取出內差的CameraTransData
        /// </summary>
        /// <param name="y"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public bool GetRotationAtY(float y, out Vector3 r)
        {
            if (rotationConfigs == null || rotationConfigs.Count <= 1)
            {
                r = Vector3.zero;
                return false;
            }

            bool inRange = false;
            RotationConfig d0 = rotationConfigs[0];
            RotationConfig d1 = rotationConfigs[1];
            for (int i = 1; i < rotationConfigs.Count; ++i)
            {
                d1 = rotationConfigs[i];

                if (d1.y >= y && d0.y <= y)
                {
                    inRange = true;
                    break;
                }

                d0 = d1;
            }

            if (!inRange)
            {
                r = Vector3.zero;
                return false;
            }

            float dist = d1.y - d0.y;
            float t = (y - d0.y) / dist;
            r = Vector3.Slerp(d0.rotation, d1.rotation, t);
            return true;
        }

        /// <summary>
        ///     針對特定高度，取出內差的CameraTransData
        /// </summary>
        /// <param name="y"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public bool GetFOVAtY(float y, out float fov)
        {
            if (fovConfigs == null || fovConfigs.Count <= 1)
            {
                fov = mainCamera.fieldOfView;// 不改動
                return false;
            }

            bool inRange = false;
            FOVConfig d0 = fovConfigs[0];
            FOVConfig d1 = fovConfigs[1];
            for (int i = 1; i < fovConfigs.Count; ++i)
            {
                d1 = fovConfigs[i];

                if (d1.y >= y && d0.y <= y)
                {
                    inRange = true;
                    break;
                }

                d0 = d1;
            }

            if (!inRange)
            {
                fov = mainCamera.fieldOfView;// 不改動
                return false;
            }

            float dist = d1.y - d0.y;
            float t = (y - d0.y) / dist;

            fov = Mathf.Lerp(d0.fov, d1.fov, t);
            return true;
        }

        /// <summary>
        ///     使用相機forward, 經過指定targetPosition, 在高度y時的位置
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public Vector3 GetCameraForwardAtY(Vector3 targetPosition, float y)
        {
            Vector3 cameraDir = MainCameraTransform.forward;
            Vector3 outPos = new Vector3(
                (y - targetPosition.y) / cameraDir.y * cameraDir.x + targetPosition.x,
                y,
                (y - targetPosition.y) / cameraDir.y * cameraDir.z + targetPosition.z);

            return outPos;
        }

        /// <summary>
        ///     將指定座標位置移動到是視野正中央 (Camera Y值不變)
        /// </summary>
        /// <param name="targetPos"></param>
        public void MoveTargetPositionToScreenCenter(Vector3 targetPos)
        {
            StopMoving();

            float cameraY = cameraTranslateTransform.position.y;
            Vector3 cameraPos = GetCameraForwardAtY(targetPos, cameraY);
            translateBound.ClosestPoint(cameraPos, out Vector3 modifyPos);

            SetCameraPositionAndNotify(modifyPos, true);
        }

        /// <summary>
        ///     將指定座標位置移動到是視野正中央 (指定Camera Y值)
        /// </summary>
        /// <param name="targetPos"></param>
        public void MoveTargetPositionToScreenCenterAtY(Vector3 targetPos, float y)
        {
            StopMoving();

            float cameraY = y;
            Vector3 cameraPos = GetCameraForwardAtY(targetPos, cameraY);
            translateBound.ClosestPoint(cameraPos, out Vector3 modifyPos);

            SetCameraPositionAndNotify(modifyPos, true);
            OnCameraHeightChange(Mathf.FloorToInt(cameraY));
        }

        /// <summary>
        ///     漸漸的, 將指定座標位置移動到是視野正中央 (Camera Y值不變)
        /// </summary>
        /// <param name="targetPos"></param>
        public void MoveTargetPositionToScreenCenterSmoothly(Vector3 targetPos)
        {
            StopMoving();

            float cameraY = cameraTranslateTransform.position.y;
            Vector3 cameraPos = GetCameraForwardAtY(targetPos, cameraY);
            translateBound.ClosestPoint(cameraPos, out Vector3 modifyPos);

            DoMoveToSmoothly(modifyPos);
        }

        /// <summary>
        ///     將Camera移至指定位置
        /// </summary>
        /// <param name="targetPos"></param>
        public void MoveToTargetPosition(Vector3 targetPos)
        {
            SetCameraPositionAndNotify(targetPos, true);
        }

        /// <summary>
        ///     漸漸的, 將Camera移至指定位置
        /// </summary>
        /// <param name="targetPos"></param>
        public void MoveToTargetPositionSmoothy(Vector3 targetPos)
        {
            StopMoving();
            DoMoveToSmoothly(targetPos);
        }

        /// <summary>
        ///     指定相機跟隨特定目標 (Camera Y值不變)
        /// </summary>
        /// <param name="target"></param>
        public void SetFollowTarget(Transform target)
        {
            StopMoving();

            _followTargetCached = target;
            _movingState.Transit(MovingState.FollowTarget);
        }

        /// <summary>
        ///     取消跟隨特定目標
        /// </summary>
        public virtual void CancelFollowTarget()
        {
            _followTargetCached = null;
            _movingState.Transit(MovingState.Idle);
        }

        /// <summary>
        ///     移動相機指定delta值
        /// </summary>
        /// <param name="delta"></param>
        public void MoveDelta(Vector3 delta)
        {
            StopMoving();

            Vector3 curPos = cameraTranslateTransform.position;
            curPos += delta;
            translateBound.ClosestPoint(curPos, out curPos);

            SetCameraPositionAndNotify(curPos);
        }

        /// <summary>
        /// 設定是否接受使用者輸入, 並指定在哪個frame生效
        /// </summary>
        /// <param name="ignore"></param>
        /// <param name="frameCount"></param>
        public void SetIgnoreUserInputDelay(bool ignore, int frameCount)
        {
            _ignoreUseInputLast = ignoreUseInput;
            ignoreUseInput = ignore;
            _activeFrame = frameCount;
        }

        protected virtual void Start()
        {
            MaxCameraBoundY = float.MinValue;
            MinCameraBoundY = float.MaxValue;

            for (var i = 0; i < translateBound.LayerBounds.Count; i++)
            {
                if (MaxCameraBoundY < translateBound.LayerBounds[i].y)
                    MaxCameraBoundY = translateBound.LayerBounds[i].y;
                if (MinCameraBoundY > translateBound.LayerBounds[i].y)
                    MinCameraBoundY = translateBound.LayerBounds[i].y;
            }
        }

        protected virtual void OnEnable()
        {
            LeanTouch.OnFingerUp += OnFingerUp;
            LeanTouch.OnFingerUpdate += OnFingerSet;
            LeanTouch.OnGesture += OnGesture;

            mainCamera.tag = "MainCamera";
        }

        protected virtual void OnDisable()
        {
            LeanTouch.OnFingerUp -= OnFingerUp;
            LeanTouch.OnFingerUpdate -= OnFingerSet;
            LeanTouch.OnGesture -= OnGesture;

            mainCamera.tag = "Untagged";
        }

        protected virtual void OnFingerUp(LeanFinger finger)
        {
            if (finger.IsOverGui)
                return;
            if (finger.StartedOverGui)
                return;
            if (IgnoreUseInputInternal)
                return;

            if (LeanTouch.Fingers.Count == 1)
            {
                StopMoving();

                Vector3 pos = cameraTranslateTransform.position;
                float distance = pos.y;

                Vector3 worldDelta = Vector3.zero;
                float ageDelta = 0f;
                if (finger.Snapshots != null
                    && finger.Snapshots.Count >= 2)
                {
                    if (Math.Abs(swipeThreshold) < 0.00001
                        || finger.Age - finger.Snapshots[finger.Snapshots.Count - 1].Age < swipeThreshold)
                    {
                        worldDelta =
                            finger.Snapshots[finger.Snapshots.Count - 1].GetWorldPosition(distance)
                            - finger.Snapshots[finger.Snapshots.Count - 2].GetWorldPosition(distance);
                        worldDelta.y = 0;

                        ageDelta = finger.Snapshots[finger.Snapshots.Count - 1].Age -
                                   finger.Snapshots[finger.Snapshots.Count - 2].Age;
                    }
                }

                if (worldDelta.magnitude > 0 && Math.Abs(ageDelta) > 0)
                {
                    DoSpeedDown(worldDelta / ageDelta);
                }
            }

            // 這邊不檢查finger數量 , 如果手指兩邊放開不是同時, 會有兩個fingerup的狀況, 導致無法回彈
            if (cameraTranslateTransform.position.y < turningBackYMin)
            {
                StopMoving();
                _curturningBackY = turningBackYMin;
                _movingState.Transit(MovingState.TurningBack);
            }
            if (cameraTranslateTransform.position.y > turningBackYMax)
            {
                StopMoving();
                _curturningBackY = turningBackYMax;
                _movingState.Transit(MovingState.TurningBack);
            }
        }

        protected virtual void OnFingerSet(LeanFinger finger)
        {
            if (IgnoreUseInputInternal)
            {
                OnFingerSetIgnoreUseInput(finger);
                return;
            }
            if (finger.StartedOverGui)
                return;
            if (finger.IsOverGui)
                return;
            if (LeanTouch.Fingers.Count != 1)
                return;

            HandleFingerSet(finger);
        }

        protected void DragEdgeMove(LeanFinger finger)
        {
            Vector2 edgePixel = DragEdgeScreenPixel;
            Vector2 screenPos = finger.ScreenPosition;
            if (screenPos.x < edgePixel.x || screenPos.y < edgePixel.y
                || screenPos.x > (Screen.width - edgePixel.x) || screenPos.y > (Screen.height - edgePixel.y))
            {
                Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
                Vector2 normal = (screenPos - screenCenter).normalized;
                Vector3 delta = new Vector3(normal.x, 0, normal.y) * Time.deltaTime * dragEdgeMoveSpeed;
                MoveDelta(delta);
            }
        }

        public void HandleFingerSet(LeanFinger finger)
        {
            StopMoving();

            Vector3 cameraPos = cameraTranslateTransform.position;

            // 計算 相機到點擊到的海面 距離
            Ray fingerRay = finger.GetRay(mainCamera);
            Vector3 fingerDir = fingerRay.direction;
            Vector3 fingerPosOnOcean = new Vector3(
                (0 - cameraPos.y) / fingerDir.y * fingerDir.x + cameraPos.x,
                0,
                (0 - cameraPos.y) / fingerDir.y * fingerDir.z + cameraPos.z);

            float distance = (fingerPosOnOcean - cameraPos).magnitude;
            //Debug.Log("distance: " + distance);

            Vector3 worldDelta = finger.GetWorldDelta(distance, mainCamera);
            worldDelta.y = 0f;

            Vector3 newPosition = cameraTranslateTransform.position;
            newPosition -= new Vector3(
                worldDelta.x * slideMultiplier.x,
                worldDelta.y * slideMultiplier.y,
                worldDelta.z * slideMultiplier.z);

            // 確認最終位置是在合法範圍內
            translateBound.ClosestPoint(newPosition, out newPosition);

            SetCameraPositionAndNotify(newPosition);
        }

        protected virtual void OnGesture(List<LeanFinger> fingers)
        {
            if (IgnoreUseInputInternal)
                return;

            // tracking zoom
            if (fingers.Count == 2)
            {
                // check age of fingers if older than 0.1 second
                LeanFinger finger1 = fingers[0];
                LeanFinger finger2 = fingers[1];

                if (finger1.Up || finger2.Up)
                    return;

                StopMoving();
                bool onGui = finger1.StartedOverGui || finger2.StartedOverGui;

                if (!onGui && finger1.Age > 0.1f && finger2.Age > 0.1f)
                {
                    float ratio = LeanGesture.GetPinchRatio();
                    if (Mathf.Abs(ratio - 1) < scaleThreshold)
                        return;

                    Vector3 pos = cameraTranslateTransform.position;
                    float newY = pos.y * ratio;
                    newY = Mathf.Clamp(newY, MinCameraBoundY, MaxCameraBoundY);
                    if (Math.Abs(pos.y - newY) > 0.000001f)
                    {
                        Vector3 newPos = GetCameraForwardAtY(pos, newY);
                        translateBound.ClosestPoint(newPos, out var modifiedPos);
                        SetCameraPositionAndNotify(modifiedPos);
                        OnCameraHeightChange(Mathf.FloorToInt(newY));
                    }
                }
            }
        }

        protected virtual void DoMoveToSmoothly(Vector3 targetPos)
        {
            _moveToTargetPos = targetPos;
            _moveToStartPos = cameraTranslateTransform.position;

            _movingState.Transit(MovingState.MoveToSmoothly);
        }

        protected virtual void DoSpeedDown(Vector3 startSpeed)
        {
            float speed = startSpeed.magnitude;
            if (Math.Abs(speed) > 0.0001f && speed > speedDownMaxStartSpeed)
            {
                startSpeed *= speedDownMaxStartSpeed / speed;
            }

            _startSpeedWhenSpeedDown = startSpeed;
            _curSpeedWhenSpeedDown = _startSpeedWhenSpeedDown;

            _movingState.Transit(MovingState.SpeedDown);
        }

        protected virtual void StopMoving()
        {
            _movingState.Transit(MovingState.Idle);
        }

        /// <summary>
        ///     設定newPos到cameraTranslateTransform
        ///     如果與上次移動距離大於sendEventDistanceThreshold
        ///     會送出 MainSceneCameraMoveEvent
        /// </summary>
        /// <param name="newPos"></param>
        /// <param name="forceSendEvt">是否強制送出event</param>
        protected virtual void SetCameraPositionAndNotify(Vector3 newPos, bool forceSendEvt = false)
        {
            cameraTranslateTransform.position = newPos;

            if (enableRotation)
            {
                if (GetRotationAtY(newPos.y, out Vector3 newRotation))
                {
                    cameraRotationTransform.localRotation = Quaternion.Euler(newRotation);
                }
            }

            if (enableChangeFOV)
            {
                if (GetFOVAtY(newPos.y, out float newFOV))
                {
                    mainCamera.fieldOfView = newFOV;
                    if (uiCamera != null)
                        uiCamera.fieldOfView = newFOV;
                }
            }

            Vector3 curPos = cameraTranslateTransform.position;
            if (forceSendEvt
               || (curPos - _lastNotifyPosition).magnitude > notifyOverDistanceThreshold)
            {
                _lastNotifyPosition = curPos;
                OnOverNotifyThreshold();
            }
        }

        protected virtual void OnOverNotifyThreshold()
        {
        }

        protected virtual void OnCameraHeightChange(int newY)
        {
        }

        protected virtual void Update()
        {
            translateBound.DebugDraw();

            _simulateDeltaTime += Time.deltaTime;
            if (_simulateDeltaTime < simulateInterval)
            {
                return;
            }

            // 避免 Time.deltaTime 太大, 在使用Curve去模擬時, 會取值不平均的問題
            int simulateTime = Mathf.FloorToInt(_simulateDeltaTime / simulateInterval);
            _simulateDeltaTime -= simulateTime * simulateInterval;

            for (int i = 0; i < simulateTime; i++)
            {
                switch (_movingState.Tick())
                {
                    case MovingState.Idle:
                    {
                        if (_movingState.Entering)
                        {
                            if (CameraStateChange != null)
                            {
                                CameraStateChange.Invoke(CameraState.Idle);
                            }
                        }
                        break;
                    }
                    case MovingState.SpeedDown:
                    {
                        if (_movingState.Entering)
                        {
                            _deltaTime = 0;
                            if (CameraStateChange != null)
                            {
                                CameraStateChange.Invoke(CameraState.SpeedDownStart);
                            }
                        }

                        if (CameraStateChange != null)
                        {
                            CameraStateChange.Invoke(CameraState.SpeedDown);
                        }

                        if (_curSpeedWhenSpeedDown.magnitude > 0.001f)
                        {
                            // 曲線
                            _deltaTime += simulateInterval;

                            Vector3 resistance = _startSpeedWhenSpeedDown * speedDownMultiplier *
                                                 speedDownCurve.Evaluate(_deltaTime / speedDownDuration);
                            Vector3 newSpeed = _curSpeedWhenSpeedDown - resistance;

                            if (Vector3.Dot(newSpeed, _curSpeedWhenSpeedDown) > 0)
                            {
                                _curSpeedWhenSpeedDown = newSpeed;
                            }
                            else
                            {
                                // -到方向相反了, 直接停下
                                _curSpeedWhenSpeedDown = Vector3.zero;
                            }

                            // 線性
                            //float r = (speedDownDuration - deltaTime);
                            //if(r < 0)
                            //    r = 0;
                            //curSpeed = startSpeed * (r / speedDownDuration);

                            Vector3 oPos = cameraTranslateTransform.position;
                            Vector3 newPos = oPos - _curSpeedWhenSpeedDown * simulateInterval;
                            translateBound.ClosestPoint(newPos, out newPos);

                            SetCameraPositionAndNotify(newPos);
                        }
                        else
                        {
                            if (CameraStateChange != null)
                            {
                                CameraStateChange.Invoke(CameraState.SpeedDownEnd);
                            }
                            _movingState.Transit(MovingState.Idle);
                        }
                        break;
                    }
                    case MovingState.MoveToSmoothly:
                    {
                        if (_movingState.Entering)
                        {
                            _deltaTime = 0;
                            if (CameraStateChange != null)
                            {
                                CameraStateChange.Invoke(CameraState.MoveToSmoothlyStart);
                            }
                        }

                        if (CameraStateChange != null)
                        {
                            CameraStateChange.Invoke(CameraState.MoveToSmoothly);
                        }

                        var dir = (_moveToTargetPos - _moveToStartPos).normalized;
                        float totalDistance = (_moveToStartPos - _moveToTargetPos).sqrMagnitude;

                        float curDistance = (cameraTranslateTransform.position - _moveToTargetPos).sqrMagnitude;
                        if (curDistance > 0.1f && totalDistance > 0.1f)
                        {
                            _deltaTime += simulateInterval;

                            var dt = simulateInterval * dir * smoothMoveSpeed
                                     * smoothMoveCurve.Evaluate((totalDistance - curDistance) / totalDistance);
                            Vector3 newCurrentPos = cameraTranslateTransform.position + dt;
                            if (Vector3.Dot(_moveToTargetPos - newCurrentPos, dir) > 0)
                            {
                                SetCameraPositionAndNotify(newCurrentPos);
                            }
                            else
                            {
                                // 方向相反 超過了
                                SetCameraPositionAndNotify(_moveToTargetPos);

                                if (CameraStateChange != null)
                                {
                                    CameraStateChange.Invoke(CameraState.MoveToSmoothlyEnd);
                                }
                                _movingState.Transit(MovingState.Idle);
                            }
                        }
                        else
                        {
                            SetCameraPositionAndNotify(_moveToTargetPos);

                            if (CameraStateChange != null)
                            {
                                CameraStateChange.Invoke(CameraState.MoveToSmoothlyEnd);
                            }
                            _movingState.Transit(MovingState.Idle);
                        }

                        break;
                    }
                    case MovingState.TurningBack:
                    {
                        if (_movingState.Entering)
                        {
                            if (CameraStateChange != null)
                            {
                                CameraStateChange.Invoke(CameraState.TurningBackStart);
                            }
                        }

                        if (CameraStateChange != null)
                        {
                            CameraStateChange.Invoke(CameraState.TurningBack);
                        }

                        Vector3 pos = cameraTranslateTransform.position;
                        var diff = _curturningBackY - pos.y;
                        var diffAbs = Math.Abs(diff);
                        if (diffAbs > 0.001f) // != 0
                        {
                            if(diffAbs < turningBackSpeed)
                            {
                                Vector3 newPos = GetCameraForwardAtY(pos, _curturningBackY);
                                SetCameraPositionAndNotify(newPos);
                            }
                            else
                            {
                                float newY = pos.y + (diff > 0 ? turningBackSpeed : -turningBackSpeed);
                                Vector3 newPos = GetCameraForwardAtY(pos, newY);
                                translateBound.ClosestPoint(newPos, out newPos);
                                SetCameraPositionAndNotify(newPos);
                            }
                        }
                        else
                        {
                            if (CameraStateChange != null)
                            {
                                CameraStateChange.Invoke(CameraState.TurningBackEnd);
                            }
                            _movingState.Transit(MovingState.Idle);
                        }
                        break;
                    }
                    case MovingState.FollowTarget:
                    {
                        if (_movingState.Entering)
                        {
                            if (CameraStateChange != null)
                            {
                                CameraStateChange.Invoke(CameraState.FollowTargetStart);
                            }
                        }

                        if (CameraStateChange != null)
                        {
                            CameraStateChange.Invoke(CameraState.FollowTarget);
                        }

                        if (_followTargetCached == null)
                        {
                            _movingState.Transit(MovingState.Idle);
                            return;
                        }

                        float cameraY = cameraTranslateTransform.position.y;
                        Vector3 cameraPos = GetCameraForwardAtY(_followTargetCached.position, cameraY);
                        translateBound.ClosestPoint(cameraPos, out Vector3 modifyPos);

                        SetCameraPositionAndNotify(modifyPos);
                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        protected virtual void OnFingerSetIgnoreUseInput(LeanFinger finger) { }
    }
}