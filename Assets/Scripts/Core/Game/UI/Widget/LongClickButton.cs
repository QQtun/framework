using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Core.Game.UI.Widget
{
    public class LongClickButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [Serializable]
        public class LongClickEvent : UnityEvent
        {
        }

        public Animator animator;
        public string pressed = "Pressed";
        public string normal = "Normal";

        public float invokeInterval = 1f;
        public LongClickEvent onClick = new LongClickEvent();

        public bool isDownClick = true;  // down事件是否觸發click
        public bool isUpClick = false;   // up事件是否觸發click

        private bool isDown = false;
        private Stopwatch mTimer;

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            isDown = true;
            if (mTimer == null)
                mTimer = Stopwatch.StartNew();
            mTimer.Reset();
            mTimer.Start();
            animator.Play(pressed);

            if(isDownClick)
                onClick.Invoke();
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            isDown = false;
            animator.Play(normal);

            if(isUpClick)
                onClick.Invoke();
        }

        private void Update()
        {
            if (isDown && mTimer != null && mTimer.Elapsed.TotalSeconds > invokeInterval)
            {
                mTimer.Reset();
                mTimer.Start();

                onClick.Invoke();
            }
        }
    }
}