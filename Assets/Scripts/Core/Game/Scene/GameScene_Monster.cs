using Core.Game.Logic;
using Core.Game.Network;
using P5.Protobuf;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Sprite = Core.Game.Logic.Sprite;

namespace Core.Game.Scene
{
    public partial class GameScene
    {
        private Dictionary<int, MonsterData> mMonsterDataDic = new Dictionary<int, MonsterData>();
        private List<MonsterData> mPreparingMonsterDatas = new List<MonsterData>();

        private void LoadMonsterData()
        {
            if (mPreparingMonsterDatas.Count == 0)
                return;

            int count = Define.LoadMonsterPerFrame;
            while(mPreparingMonsterDatas.Count > 0 && count > 0)
            {
                count--;
                var monster = mPreparingMonsterDatas[0];
                mPreparingMonsterDatas.RemoveAt(0);
                var sprite = FindById(monster.Id) as Sprite;
                if(sprite != null)
                {
                    LogUtil.Debug.Log($"monster exist ! roleId={monster.Id}");
                    return;
                }

                if(TryGetPreparingObj(monster.Id, out var obj))
                {
                    sprite = obj as Sprite;
                    sprite.Hp = monster.LifeValue;
                    return;
                }

                var name = $"Monster_{monster.Id}";
                var go = new GameObject(name);
                var monsterSprite = go.AddComponent<Sprite>();
                monsterSprite.InitMonster(monster);
                AddPreparingObj(monsterSprite);
                SendMsgHelper.SendLoadSpriteData(monster.Id);
            }
        }
    }
}