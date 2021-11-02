namespace Core.Game.UI.Widget
{
    using UnityEngine;
    using UnityEngine.Events;


    public class JoystickLeanTouch : MonoBehaviour
    {
        [System.Serializable]
        public class JoystickEvent : UnityEvent { }

        [System.Serializable]
        public class JoystickDragEvent : UnityEvent<Vector2> { }


        public float maxRadius = 100; //Handle 最大移動半徑

        public bool dynamic = true; // 是否移動搖桿位置
        public Canvas rootCanvas;
        public RectTransform root;
        public RectTransform handle; //搖桿
        public RectTransform backGround; //背景

        public Vector2 touchRectMin;
        public Vector2 touchRectMax;

        public JoystickDragEvent onValueChanged = new JoystickDragEvent();
        public JoystickEvent onFingerDown = new JoystickEvent();
        public JoystickEvent onFingerUp = new JoystickEvent();

        private Vector3 mBackGroundOriginLocalPostion;
        private Vector2 mOriginalHandlePossition;
        private int mFingerId = int.MinValue; // 當前手指

        public bool IsDraging { get { return mFingerId != int.MinValue; } }
        public bool DynamicJoystick
        {
            get => dynamic;
            set => dynamic = value;
        }

        private void Awake()
        {
            mBackGroundOriginLocalPostion = backGround.localPosition;
            mOriginalHandlePossition = handle.anchoredPosition;
        }

        private void OnDisable()
        {
            RestJoystick();
            Lean.Touch.LeanTouch.OnFingerDown -= OnFingerDown;
            Lean.Touch.LeanTouch.OnFingerUp -= OnFingerUp;
            Lean.Touch.LeanTouch.OnFingerUpdate -= OnFingerUpdate;
        }

        private void OnEnable()
        {
            Lean.Touch.LeanTouch.OnFingerDown += OnFingerDown;
            Lean.Touch.LeanTouch.OnFingerUp += OnFingerUp;
            Lean.Touch.LeanTouch.OnFingerUpdate += OnFingerUpdate;
        }

        private void OnFingerDown(Lean.Touch.LeanFinger finger)
        {
            if (mFingerId >= 0)
                return;
            if (finger.StartedOverGui)
                return;

            mFingerId = finger.Index;

            if(dynamic)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(root, finger.ScreenPosition, rootCanvas.worldCamera, out var anchoredPos);
                backGround.anchoredPosition = anchoredPos;
            }

            onFingerDown.Invoke();
        }

        private void OnFingerUpdate(Lean.Touch.LeanFinger finger)
        {
            if (finger.Index != mFingerId)
                return;

            var screenPos = finger.ScreenPosition;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                backGround, screenPos, rootCanvas.worldCamera, out var anchoredPos);

            var diff = anchoredPos - backGround.anchoredPosition;

            if (diff.magnitude > maxRadius)
                anchoredPos = backGround.anchoredPosition + diff.normalized * maxRadius;

            handle.anchoredPosition = anchoredPos;

            onValueChanged.Invoke(diff / maxRadius);
        }

        private void OnFingerUp(Lean.Touch.LeanFinger finger)
        {
            if (finger.Index != mFingerId)
                return;

            RestJoystick();

            onFingerUp.Invoke();
        }

        private void RestJoystick()
        {
            backGround.localPosition = mBackGroundOriginLocalPostion;
            handle.localPosition = Vector3.zero;
            mFingerId = int.MinValue; 
        }
    }
}
