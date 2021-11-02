using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Core.Game.UI.Widget
{
    public class Joystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [System.Serializable] public class JoystickEvent : UnityEvent<Vector2> { }

        public float maxRadius = 100; //Handle 最大移動半徑
        public bool dynamic = true; // 是否移動搖桿位置
        public Canvas rootCanvas;
        public RectTransform root;
        public RectTransform handle; //搖桿
        public RectTransform backGround; //背景

        public JoystickEvent onValueChanged = new JoystickEvent();
        public JoystickEvent onPointerDown = new JoystickEvent();
        public JoystickEvent onPointerUp = new JoystickEvent();

        private Vector3 mBackGroundOriginLocalPostion;
        private int mFingerId = int.MinValue; //当前触发摇杆的 pointerId ，预设一个永远无法企及的值
        private Vector2 mHandleOriginalPos;

        public Vector2 Value { get; private set; }

        public bool IsDraging { get { return mFingerId != int.MinValue; } }

        public bool DynamicJoystick
        {
            set
            {
                if (dynamic != value)
                {
                    dynamic = value;
                    ConfigJoystick();
                }
            }
            get
            {
                return dynamic;
            }
        }

        private void Awake()
        {
            mBackGroundOriginLocalPostion = backGround.localPosition;
            if(rootCanvas == null)
            {
                rootCanvas = GetComponentInParent<Canvas>();
            }
        }

        private void Update()
        {
            if (IsDraging)
            {
                Value = (handle.anchoredPosition - mHandleOriginalPos) / maxRadius;
                onValueChanged.Invoke(Value);
                //Debug.Log(value);
            }
        }

        private void OnDisable()
        {
            RestJoystick(); //意外被 Disable 各单位需要被重置
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            if (eventData.pointerId < -1 || IsDraging) return; //适配 Touch：只响应一个Touch；适配鼠标：只响应左键
            mFingerId = eventData.pointerId;
            if (dynamic)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(root, eventData.position, rootCanvas.worldCamera, out var anchoredPos);
                backGround.anchoredPosition = anchoredPos;
            }
            RectTransformUtility.ScreenPointToLocalPointInRectangle(backGround, eventData.position, rootCanvas.worldCamera, out mHandleOriginalPos);
            onPointerDown.Invoke(eventData.position);
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (mFingerId != eventData.pointerId) return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(backGround, eventData.position, rootCanvas.worldCamera, out var anchoredPos);
            Vector2 direction = anchoredPos - mHandleOriginalPos;

            float radius = Mathf.Clamp(Vector3.Magnitude(direction), 0, maxRadius);
            Vector2 newAnchoredPos = mHandleOriginalPos + direction.normalized * radius;

            handle.anchoredPosition = newAnchoredPos;
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            if (mFingerId != eventData.pointerId) return;//正确的手指抬起时才会重置摇杆；
            RestJoystick();
            onPointerUp.Invoke(eventData.position);
        }

        private void RestJoystick()
        {
            backGround.localPosition = mBackGroundOriginLocalPostion;
            handle.localPosition = Vector3.zero;
            mFingerId = int.MinValue;
            Value = Vector2.zero;
        }

        private void ConfigJoystick() //配置动态/静态摇杆
        {
            if (!dynamic) mBackGroundOriginLocalPostion = backGround.localPosition;
            GetComponent<Image>().raycastTarget = dynamic;
            handle.GetComponent<Image>().raycastTarget = !dynamic;
        }
    }
}
