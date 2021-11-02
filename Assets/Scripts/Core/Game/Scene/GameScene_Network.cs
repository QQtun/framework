using Core.Framework.Network.Data;
using Core.Framework.Utility;
using Core.Game.Logic;
using Core.Game.Network;
using Core.Game.Table;
using Core.Game.Utility;
using P5.Protobuf;

namespace Core.Game.Scene
{
    public partial class GameScene
    {
        private void OnReceiveMessage(Message msg)
        {
            switch((ServerCmd) msg.MessageId)
            {
                case ServerCmd.MonsterDataNotify:
                {
                    OnRecvMonsterDataNotify(msg);
                    break;
                }
                case ServerCmd.LoadSpriteData:
                {
                    OnRecvLoadSpriteData(msg);
                    break;
                }
                case ServerCmd.SpriteLeave:
                {
                    OnRecvSpriteLeave(msg);
                    break;
                }
                case ServerCmd.SpriteMove:
                {
                    OnRecvSpriteMove(msg);
                    break;
                }

                case ServerCmd.SpriteAction:
                {
                    OnRecvSpriteAction(msg);
                    break;
                }
                case ServerCmd.SpriteMagicCode:
                {
                    OnRecvSpriteMagicCode(msg);
                    break;
                }
                case ServerCmd.SpriteInjure:
                {
                    OnRecvSpriteInjure(msg);
                    break;
                }
                case ServerCmd.SpriteHited:
                {
                    OnRecvSpriteHited(msg);
                    break;
                }
            }
        }

        private void OnRecvMonsterDataNotify(Message msg)
        {
            var monster = msg.GetData<MonsterData>();
            mMonsterDataDic[monster.MasterRoleId] = monster;
            if (monster.LifeValue > 0)
            {
                var exist = mPreparingMonsterDatas.FindIndex(d => d.Id == monster.Id);
                if (exist >= 0)
                    mPreparingMonsterDatas.RemoveAt(exist);
                mPreparingMonsterDatas.Add(monster);
            }
        }

        private void OnRecvLoadSpriteData(Message msg)
        {
            var rsp = msg.GetData<LoadSpriteDataResponse>();
            var obj = PopPreparingObj(rsp.RoleId) as Sprite;
            Add(obj);
        }

        private void OnRecvSpriteMove(Message msg)
        {
            var ntf = msg.GetData<SpriteNotifyOtherMoveData>();
            var sprite = FindById(ntf.RoleId) as Sprite;
            if (sprite != null && sprite.IsReady)
            {
                sprite.SetMoveSpeed(UnityEngine.Mathf.RoundToInt((float)(ntf.MoveCost * 100)), true);
                if (sprite.SpriteType == SpriteType.Monster)
                {
                    Sprite target = null;
                    if (ntf.TargetId > 0)
                    {
                        target = FindById(ntf.TargetId) as Sprite;
                    }
                    if (target != null)
                    {
                        sprite.LockTarget = target;
                    }
                }
                SceneObjectMovingManager.Instance.StartMoving("SpriteMove", sprite, new UnityEngine.Vector2Int(ntf.ToX, ntf.ToY), true,
                    (obj)=>
                    {
                        sprite.SetMoveSpeed(0, true);
                    });
            }
        }

        private void OnRecvSpriteLeave(Message msg)
        {
            var leave = msg.GetData<SpriteLeave>();
            var sprite = FindById(leave.RoleId) as Sprite;
            if (sprite != null)
            {
                sprite.Hp = leave.LifeValue;
                sprite.StartDead();
            }
            else
            {
            }
        }

        private void OnRecvSpriteAction(Message msg)
        {
            var rsp = msg.GetData<RoleAction>();
            //LogUtil.Debug.LogError($"OnRecvSpriteAction={rsp}");
            var sprite = FindById(rsp.RoleId) as Sprite;
            if (sprite.SpriteType == SpriteType.Monster)
            {
                sprite.SetAction((ActionType)rsp.Action, true);
                if (rsp.Action == (int) ActionType.Attack && rsp.TargetX > 0 && rsp.TargetY > 0)
                {
                    sprite.Rotation = UnityEngine.Quaternion.LookRotation(
                        (new UnityEngine.Vector2Int(rsp.TargetX , rsp.TargetY) - sprite.Coordinate).ToVector3f());
                }
            }
        }

        private void OnRecvSpriteMagicCode(Message msg)
        {
            var rsp = msg.GetData<MagicCodeData>();
            //LogUtil.Debug.LogError($"OnRecvSpriteMagicCode={rsp}");
        }

        private void OnRecvSpriteInjure(Message msg)
        {
            var rsp = msg.GetData<InjuredData>();
            //LogUtil.Debug.LogError($"OnRecvSpriteInjurek={rsp}");
            if(rsp.HitToGridX > 0 && rsp.HitToGridY > 0)
            {
                var magicData = TableGroup.MagicsTable.Get((uint)rsp.MagicCode);
                var injuredSprite = FindById(rsp.InjuredRoleId) as Sprite;
                var attackSprite = FindById(rsp.AttackerRoleId) as Sprite;
                if(attackSprite != null && injuredSprite != null)
                {
                    injuredSprite.Rotation = UnityEngine.Quaternion.LookRotation(
                        (attackSprite.Coordinate - injuredSprite.Coordinate).ToVector3f());
                }
                var speed = magicData.HitIdSetting.HitDistance.Count >= 3 ? magicData.HitIdSetting.HitDistance[2] : 0;
                SceneObjectMovingManager.Instance.StartMoving("Sprite_Injure", injuredSprite, 
                    new UnityEngine.Vector2Int(rsp.HitToGridX, rsp.HitToGridY),
                    speed, 0, false);
                injuredSprite.Hp = rsp.InjuredRoleLife;
            }
        }

        private void OnRecvSpriteHited(Message msg)
        {
            var rsp = msg.GetData<RoleHited>();
            //LogUtil.Debug.LogError($"OnRecvSpriteHited={rsp}");
        }
    }
}