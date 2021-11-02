using Core.Game.Utility;
using P5.Protobuf;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace Core.Game.Logic
{
    public partial class Sprite
    {
        private Coroutine mLoadMonsterCo;
        private GameObject mMonsterInfoUI;
        private Text mMonsterText;
        private Slider mMonsterHpSlider;
        private MonsterData mMonsterData;

        public void InitMonster(MonsterData data)
        {
            mMonsterData = data;
            Init(SpriteType.Monster, data.Id, $"Monster_{data.Id}", new Vector2Int(data.PosX, data.PosY));

            if (mLoadMonsterCo != null)
                CoroutineUtil.StopCoroutine(mLoadMonsterCo);
            mLoadMonsterCo = CoroutineUtil.StartCoroutine(LoadMonster());
        }

        private IEnumerator LoadMonster()
        {
            var h = Addressables.InstantiateAsync("Assets/PublicAssets/Test/TestBundle/Monster.prefab");
            var monsterInfoHnalde = Addressables.InstantiateAsync("Assets/PublicAssets/UI/MonsterInfo.prefab");
            yield return h;
            yield return monsterInfoHnalde;

            var monsterGo = h.Result;
            monsterGo.transform.SetParent(mTransform);
            monsterGo.transform.localPosition = Vector3.zero;
            mAnimator = monsterGo.GetComponent<Animator>() ?? monsterGo.AddComponent<Animator>();
            //for (int i = 0; i < mAnimator.parameterCount; i++)
            //{
            //    var param = mAnimator.parameters[i];
            //    mAnimatorParamDic[param.name] = param;
            //}
            mMonsterInfoUI = monsterInfoHnalde.Result;
            mMonsterInfoUI.transform.SetParent(mTransform);
            var pos = mMonsterInfoUI.transform.localPosition;
            pos.x = 0;
            pos.z = 0;
            mMonsterInfoUI.transform.localPosition = pos;

            // TODO
            mMonsterText = mMonsterInfoUI.GetComponentInChildren<Text>();
            mMonsterHpSlider = mMonsterInfoUI.GetComponentInChildren<Slider>();
            mMonsterText.text = Name;
            mMonsterHpSlider.value = (float)(mMonsterData.LifeValue / mMonsterData.MaxLifeValue);

            gameObject.SetActive(true);
            IsReady = true;
            mLoadMonsterCo = null;
        }
    }
}