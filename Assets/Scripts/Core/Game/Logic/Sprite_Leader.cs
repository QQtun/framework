using Core.Framework.Res;
using Core.Framework.Utility;
using Core.Game.Utility;
using P5.Protobuf;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace Core.Game.Logic
{
    public partial class Sprite
    {
        private Coroutine mLoadLeaderCo;
        private GameObject mRoleInfoUI;
        private Text mRoleText;

        public void InitLeader(RoleDetail data)
        {
            Init(SpriteType.Leader, data.RoleId, data.RoleName, new Vector2Int(data.PosX, data.PosY));

            if (mLoadLeaderCo != null)
                CoroutineUtil.StopCoroutine(mLoadLeaderCo);
            mLoadLeaderCo = CoroutineUtil.StartCoroutine(LoadLeader());
        }

        private IEnumerator LoadLeader()
        {
            var skeletonHandle = Addressables.InstantiateAsync("Assets/PublicAssets/Test/TestBundle/a_skeleton.prefab");
            var roleInfoHanle = Addressables.InstantiateAsync("Assets/PublicAssets/UI/RoleInfo.prefab");
            var weaponHandle = Addressables.InstantiateAsync("Assets/PublicAssets/Test/TestBundle/Weapon.prefab");
            var partsHandle = new List<AsyncOperationHandle>()
            {
                Addressables.InstantiateAsync("Assets/Arts/model/Equip/Prefab/1_body_05.prefab"),
                Addressables.InstantiateAsync("Assets/Arts/model/Equip/Prefab/1_face_00.prefab"),
                Addressables.InstantiateAsync("Assets/Arts/model/Equip/Prefab/1_hair_00.prefab"),
                Addressables.InstantiateAsync("Assets/Arts/model/Equip/Prefab/1_hand_00.prefab"),
                Addressables.InstantiateAsync("Assets/Arts/model/Equip/Prefab/1_shoe_05.prefab"),
                skeletonHandle,
                roleInfoHanle,
                weaponHandle,
            };

            //RuntimeAnimatorController ctrl = null;
            //ResourceManager.Instance.LoadAssetAsync<RuntimeAnimatorController>(
            //    "Assets/PublicAssets/Test/TestBundle/a_skeleton.controller",
            //    (c) =>
            //    {
            //        ctrl = c;
            //    });
            while (true)
            {
                bool allDone = true;
                for (int i = 0; i < partsHandle.Count; i++)
                {
                    if (!partsHandle[i].IsDone)
                    {
                        allDone = false;
                        break;
                    }
                }
                if (!allDone)
                    yield return null;
                else
                    break;
            }
            //yield return new WaitUntil(() => ctrl != null);
            gameObject.SetActive(true); // 必須在設定Animator之前打開 不然參數都白設了

            skeletonHandle.Result.transform.SetParent(mTransform);
            skeletonHandle.Result.transform.localPosition = Vector3.zero;
            List<GameObject> parts = new List<GameObject>();
            for (int i = 0; i < 5; i++)
            {
                parts.Add(partsHandle[i].Result as GameObject);
            }
            MeshHelper.MergeMeshes("a_skeleton", skeletonHandle.Result, parts);

            var weaponRoot = skeletonHandle.Result.transform.FindInChildren("RightWeapon_Bone01");
            var oldWPos = weaponHandle.Result.transform.localPosition;
            var oldWRot = weaponHandle.Result.transform.localRotation;
            weaponHandle.Result.transform.SetParent(weaponRoot);
            weaponHandle.Result.transform.localPosition = oldWPos;
            weaponHandle.Result.transform.localRotation = oldWRot;

            mAnimator = skeletonHandle.Result.GetComponent<Animator>() ?? skeletonHandle.Result.AddComponent<Animator>();
            IsBattling = true;

            var stateHalper = skeletonHandle.Result.GetComponent<AnimatorStateHelper>() ?? skeletonHandle.Result.AddComponent<AnimatorStateHelper>();
            stateHalper.OnStateChange += OnAnimatorStateChange;

            mRoleInfoUI = roleInfoHanle.Result;
            mRoleInfoUI.transform.SetParent(mTransform);
            var pos = mRoleInfoUI.transform.localPosition;
            pos.x = 0;
            pos.z = 0;
            mRoleInfoUI.transform.localPosition = pos;
            mRoleText = mRoleInfoUI.GetComponentInChildren<Text>();
            mRoleText.text = Name;

            IsBattling = true;

            IsReady = true;
            mLoadLeaderCo = null;
        }

    }
}