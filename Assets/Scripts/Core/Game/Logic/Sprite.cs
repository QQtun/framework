using Core.Framework.Module;
using Core.Framework.Utility;
using Core.Game.Module;
using Core.Game.Scene;
using Core.Game.Table;
using Core.Game.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Game.Logic
{
    public partial class Sprite : MonoBehaviour, ISceneObject
    {
        private Animator mAnimator;
        //private Dictionary<string, AnimatorControllerParameter> mAnimatorParamDic = new Dictionary<string, AnimatorControllerParameter>();
        private FiniteState<ActionType> mActionType = new FiniteState<ActionType>(ActionType.Stand);
        private Vector2Int mCoordinate;
        private bool mCoordinateDirty;
        private Transform mTransform;
        private Quaternion mRotation;
        private bool mStartDead;
        private float mDeadEndTime;

        public int Id { get; private set; }
        public string Name { get; private set; }

        public GameObject GameObject => gameObject;

        public Vector2Int OrigCoordinate { get; private set; }
        public Vector2Int Coordinate { get => mCoordinate; set { mCoordinateDirty = true; mCoordinate = value; } }
        public int Direction { get; set; }
        public Quaternion Rotation { get => mRotation; set => mRotation = value; }
        public double Hp { get; set; }

        public int MoveSpeed
        {
            get
            {
                return mAnimator.GetInteger(AnimatorUtil.Param.SpeedInt);
            }
            private set
            {
                mAnimator.SetInteger(AnimatorUtil.Param.SpeedInt, value);
            }
        }

        public ActionType Action
        {
            get => mActionType.Current;
            private set
            {
                if (mActionType.Current == value)
                {
                    return;
                }
                mActionType.Transit(value, true);
            }
        }

        public SceneObjectType Type => SceneObjectType.Sprite;
        public SpriteType SpriteType { get; private set; }
        public bool IsReady { get; private set; }
        public Sprite LockTarget { get; set; }
        public float Radius { get; private set; }

        private void Awake()
        {
            mTransform = transform;
        }

        public void OnUpdate()
        {
            if (mCoordinateDirty)
            {
                var newPos = new Vector3(mCoordinate.x / 100f, mTransform.position.y, mCoordinate.y / 100f);
                var groundPos = Utility.GameUtil.GetGroundPos(newPos);
                newPos.y = groundPos.y;
                mTransform.position = newPos;
                mCoordinateDirty = false;
            }

            if(mTransform.rotation != mRotation)
            {
                mTransform.rotation = Quaternion.Slerp(mTransform.rotation, mRotation, Time.deltaTime * 15.0f);
            }

            if(mStartDead && Time.time >= mDeadEndTime)
            {
                //GameScene.Instance.Remove(this);
            }
        }

        private void Init(SpriteType type, int id, string name, Vector2Int pos)
        {
            mTransform = transform;
            SpriteType = type;
            Id = id;
            Name = name;
            Action = ActionType.Stand;
            OrigCoordinate = pos;
            Coordinate = pos;
        }

        public void StartDead()
        {
            //mStartDead = true;
            //mDeadEndTime = Time.time + Define.DeadDelay;
        }

        public bool AttackMagic(Magics skillData, Sprite target, int attackNum)
        {
            var skillModule = ModuleManager.Instance.GetModule<SkillModule>();
            LockTarget = target;

            // 技能移動
            Vector2Int dir;
            if (target != null)
                dir = target.Coordinate - Coordinate;
            else
                dir = Vector2Int.FloorToInt(DirectionUtil.GetVector2FromDir16(Direction)) * skillData.ClientMoveDistance;
            var curDis = dir.magnitude;
            Rotation = Quaternion.LookRotation(dir.ToVector3f(), Vector3.up);
            mTransform.rotation = Rotation;
            mAnimator.SetTrigger(AnimatorUtil.GetAttackTrigger(attackNum));
            Vector2Int moveTo = Coordinate;
            if (curDis > 75f)
            {
                Vector2 magicMovePos = Coordinate + dir.ToVector2f().normalized * Mathf.Min(curDis - 75f, skillData.ClientMoveDistance);
                moveTo = Vector2Int.FloorToInt(magicMovePos);
                SceneObjectMovingManager.Instance.StartMoving("Leader AttackMagic", this, moveTo, skillData.MoveSpeed, skillData.MoveDelay[0] / 1000f, true, 
                    (obj) => 
                    {
                        SetMoveSpeed(0, false);
                    });
            }

            if(skillData.CDTime > 0)
            {
                skillModule.AddCoolDown(skillData.ID);
            }

            if(target != null)
            {
                Network.SendMsgHelper.SendSpriteMagicCode((int)skillData.ID, target.Id, target.Coordinate);
                Network.SendMsgHelper.SendSpriteAttack(Coordinate, (int)skillData.ID, moveTo, target.Id, target.Coordinate);
            }
            else
            {
                Network.SendMsgHelper.SendSpriteMagicCode((int)skillData.ID, -1, moveTo);
                Network.SendMsgHelper.SendSpriteAttack(Coordinate, (int)skillData.ID, moveTo, -1, moveTo);
            }
            return true;
        }

        public bool DodgeMagic(Magics skillData, Vector3 dir)
        {
            var roleModule = ModuleManager.Instance.GetModule<RoleDataModule>();
            var skillModule = ModuleManager.Instance.GetModule<SkillModule>();
            var curDis = dir.magnitude;

            bool faceForward = TableGroup.ConfigTable.GetSpecialMagic(
                SpecialMagicIndex.NewDodge, roleModule.RoleDetail.Occupation).ID != skillData.ID;
            Rotation = Quaternion.LookRotation((faceForward ? 1 : -1) * dir, Vector3.up);
            mTransform.rotation = Rotation;
            mAnimator.SetTrigger(AnimatorUtil.GetDodgeTrigger(skillData.ID));

            if (skillData.CDTime > 0)
            {
                skillModule.AddCoolDown(skillData.ID);
            }

            Vector2Int moveTo = Vector2Int.FloorToInt(Coordinate.ToVector2f() + dir.ToVector2IgnoreY().normalized * skillData.ClientMoveDistance);
            
            SceneObjectMovingManager.Instance.StartMoving("Leader AttackMagic", this, moveTo, skillData.MoveSpeed, skillData.MoveDelay[0] / 1000f, faceForward,
            (obj) =>
            {
                SetMoveSpeed(0, false);
            });

            if (skillData.CDTime > 0)
            {
                skillModule.AddCoolDown(skillData.ID);
            }

            Network.SendMsgHelper.SendSpriteMagicCode((int)skillData.ID, -1, moveTo);
            Network.SendMsgHelper.SendSpriteAttack(Coordinate, (int)skillData.ID, moveTo, -1, moveTo);

            return true;
        }
    }
}