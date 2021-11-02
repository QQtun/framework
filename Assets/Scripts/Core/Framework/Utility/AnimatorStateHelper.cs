using System;
using UnityEngine;

namespace Core.Framework.Utility
{
    [RequireComponent(typeof(Animator))]
    public class AnimatorStateHelper : MonoBehaviour
    {
        private Animator mAnimator;
        private AnimatorStateInfo mCurrentState;
        private AnimatorStateInfo mNextState;

        public event Action<AnimatorStateInfo, AnimatorStateInfo> OnStateChange;
        public event Action<AnimatorStateInfo, AnimatorStateInfo> OnStartTransition;

        private void Awake()
        {
            mAnimator = GetComponent<Animator>();
            mCurrentState = mAnimator.GetCurrentAnimatorStateInfo(0);
            if (mAnimator.IsInTransition(0))
                mNextState = mAnimator.GetNextAnimatorStateInfo(0);
        }

        private void Update()
        {
            AnimatorStateInfo currentState = mAnimator.GetCurrentAnimatorStateInfo(0);
            if (mAnimator.IsInTransition(0))
            {
                var nextState = mAnimator.GetNextAnimatorStateInfo(0);
                if (mCurrentState.fullPathHash == currentState.fullPathHash
                    && nextState.fullPathHash != mNextState.fullPathHash)
                {
                    mNextState = nextState;
                    OnStartTransition?.Invoke(mCurrentState, mNextState);
                    //Debug.Log($"start transition cur={mCurrentState.fullPathHash} , next={mNextState.fullPathHash}");
                }
                else if (mCurrentState.fullPathHash != currentState.fullPathHash)
                {
                    var lastState = mCurrentState;
                    mCurrentState = currentState;
                    OnStateChange?.Invoke(mCurrentState, lastState);
                    //Debug.Log($"state change cur={mCurrentState.fullPathHash} , last={lastState.fullPathHash}");
                }
            }
            else
            {
                if (mCurrentState.fullPathHash != currentState.fullPathHash)
                {
                    var lastState = mCurrentState;
                    mCurrentState = currentState;
                    OnStateChange?.Invoke(mCurrentState, lastState);
                    //Debug.Log($"state change cur={mCurrentState.fullPathHash} , last={lastState.fullPathHash}");
                }
            }
        }
    }
}