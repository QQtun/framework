using Core.Framework.Module;
using Core.Game.Module;
using Core.Game.Utility;
using UnityEngine;

namespace Core.Game.Logic
{
    public partial class Sprite
    {
        public bool IsBattling
        {
            get => mAnimator.GetBool(AnimatorUtil.Param.IsBattling);
            set => mAnimator.SetBool(AnimatorUtil.Param.IsBattling, value);
        }

        private void OnAnimatorStateChange(AnimatorStateInfo curState, AnimatorStateInfo lastState)
        {
            if (curState.IsName(AnimatorUtil.State.Idle)
                || curState.IsName(AnimatorUtil.State.BattleIdle))
            {
                Action = ActionType.Stand;
            }
            else if (curState.IsName(AnimatorUtil.State.Run))
            {
                Action = ActionType.Run;
            }
            else if (curState.IsName(AnimatorUtil.State.RushStart)
                || curState.IsName(AnimatorUtil.State.RushLoop))
            {
                Action = ActionType.Rush;
            }
            else if (curState.IsName(AnimatorUtil.State.RushEnd))
            {
                Action = ActionType.RushEnd;
            }
            else if (curState.IsName(AnimatorUtil.State.Attack1)
                || curState.IsName(AnimatorUtil.State.Attack2)
                || curState.IsName(AnimatorUtil.State.Attack3)
                || curState.IsName(AnimatorUtil.State.Attack4))
            {
                Action = ActionType.Attack;
            }

            if (curState.IsName(AnimatorUtil.State.JumpStart))
            {
                mAnimator.SetBool(AnimatorUtil.Param.IsJumping, true);
            }

            if (lastState.IsName(AnimatorUtil.State.JumpEnd))
            {
                mAnimator.SetBool(AnimatorUtil.Param.IsJumping, false);
            }

            if (curState.IsName(AnimatorUtil.State.RushEnd))
            {
                mAnimator.SetBool(AnimatorUtil.Param.IsRush, false);
            }

            Vector2Int targetPos = Vector2Int.zero;
            if (LockTarget != null)
                targetPos = LockTarget.Coordinate;
            int actionData = 0;
            if(Action == ActionType.Attack)
            {
                if (curState.IsName(AnimatorUtil.State.Attack1))
                    actionData = 1;
                else if (curState.IsName(AnimatorUtil.State.Attack2))
                    actionData = 2;
                else if (curState.IsName(AnimatorUtil.State.Attack3))
                    actionData = 3;
                else if (curState.IsName(AnimatorUtil.State.Attack4))
                    actionData = 4;
            }
            Network.SendMsgHelper.SendSpriteAction(Rotation.eulerAngles.y, Action, Coordinate, targetPos, actionData);
        }

        public bool CanBreakActionByMagic()
        {
            var state = mAnimator.GetCurrentAnimatorStateInfo(0);
            //Debug.LogError($"IsInTransition={mAnimator.IsInTransition(0)} length={state.length} normalizedTime={state.normalizedTime}");
            // TODO
            if (state.IsName(AnimatorUtil.State.Attack1)
                || state.IsName(AnimatorUtil.State.Attack2)
                || state.IsName(AnimatorUtil.State.Attack3)
                || state.IsName(AnimatorUtil.State.Attack4))
            {
                if (mAnimator.IsInTransition(0))
                    return false;
                return state.normalizedTime > 0.35f;
            }
            return true;
        }

        public bool CanBreakActionByDodge()
        {
            return true;
        }

        public bool CanBreakActionByUserInput()
        {
            //Debug.LogError($"Action={Action}");

            if (Action != ActionType.Attack && Action != ActionType.Magic)
                return true;

            var state = mAnimator.GetCurrentAnimatorStateInfo(0);
            if (!mAnimator.IsInTransition(0))
            {
                return state.normalizedTime > 0.55f;
            }
            else
            {
                var nextState = mAnimator.GetNextAnimatorStateInfo(0);
                if (nextState.IsName(AnimatorUtil.State.Run))
                {
                    return true;
                }
                return false;
            }
        }

        public void StartRush()
        {
            if (mActionType.Current == ActionType.Run)
            {
                mAnimator.SetBool(AnimatorUtil.Param.IsWalk, false);
                mAnimator.SetBool(AnimatorUtil.Param.IsRush, true);
            }
        }

        public void SetMoveSpeed(int speed, bool forceChangeAction = false)
        {
            MoveSpeed = speed;
            //if(SpriteType == SpriteType.Leader)
            //    Debug.LogError("MoveSpeed=" + MoveSpeed);
            if (forceChangeAction)
            {
                var curState = mAnimator.GetCurrentAnimatorStateInfo(0);
                if (mAnimator.IsInTransition(0))
                {
                    var nextState = mAnimator.GetNextAnimatorStateInfo(0);
                    if (speed > 0
                        && !curState.IsName(AnimatorUtil.State.Run)
                        && !nextState.IsName(AnimatorUtil.State.Run)
                        && !curState.IsName(AnimatorUtil.State.RushStart)
                        && !nextState.IsName(AnimatorUtil.State.RushStart)
                        && !curState.IsName(AnimatorUtil.State.RushLoop)
                        && !nextState.IsName(AnimatorUtil.State.RushLoop))
                    {
                        mAnimator.SetTrigger(AnimatorUtil.Param.RunTrigger);
                    }
                    else if (speed == 0
                        && !curState.IsName(AnimatorUtil.State.Idle)
                        && !nextState.IsName(AnimatorUtil.State.Idle))
                    {
                        mAnimator.SetTrigger(AnimatorUtil.Param.IdleTrigger);
                    }
                }
                else
                {
                    if (speed > 0
                        && !curState.IsName(AnimatorUtil.State.Run)
                        && !curState.IsName(AnimatorUtil.State.RushStart)
                        && !curState.IsName(AnimatorUtil.State.RushLoop))
                    {
                        mAnimator.SetTrigger(AnimatorUtil.Param.RunTrigger);
                    }
                    else if (speed == 0 && !curState.IsName(AnimatorUtil.State.Idle))
                    {
                        mAnimator.SetTrigger(AnimatorUtil.Param.IdleTrigger);
                    }
                }
            }
        }
        public void SetAction(ActionType action, bool force)
        {
            switch (action)
            {
                case ActionType.None:
                    break;
                case ActionType.Stand:
                {
                    mAnimator.SetInteger(AnimatorUtil.Param.SpeedInt, 0);
                    mAnimator.SetBool(AnimatorUtil.Param.IsJumping, false);
                    mAnimator.SetBool(AnimatorUtil.Param.IsRush, false);
                    mAnimator.SetBool(AnimatorUtil.Param.IsWalk, false);
                    break; 
                }
                case ActionType.Walk:
                    break;
                case ActionType.Run:
                    break;
                case ActionType.Attack:
                {
                    mAnimator.SetTrigger(AnimatorUtil.Param.Attack1Trigger);
                    break;
                }
                case ActionType.Injured:
                    break;
                case ActionType.Magic:
                    break;
                case ActionType.Rush:
                    break;
                case ActionType.RushEnd:
                    break;
            }
        }
    }
}