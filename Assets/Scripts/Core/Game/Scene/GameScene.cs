using Core.Framework.Module;
using Core.Framework.Scene;
using Core.Framework.UI;
using Core.Game.Controller;
using Core.Game.UI;
using Core.Game.Logic;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Core.Game.Network;
using P5.Protobuf;
using Sprite = Core.Game.Logic.Sprite;
using Core.Framework.Schedule;
using Core.Game.Utility;
using Core.Game.Module;
using Core.Game.Table;
//using UnityEngine.Rendering.Universal;

namespace Core.Game.Scene
{
    [SingletonPrefab("Prefabs/Singleton/GameScene")]
    public partial class GameScene : Singleton<GameScene>, ISceneEntry
    {
        private enum InitState
        {
            Start,
            Waiting,
        }

        private Framework.Utility.FiniteState<InitState> mState = new Framework.Utility.FiniteState<InitState>(InitState.Start);
        private UserInputCtrl mInputCtrl;
        private SceneObjectPool mObjectPool;
        private ThirdPersonCameraCtrl mCameraCtrl;
        private bool mPlayGame = false;
        private int mSceneCount;

        public Sprite Leader { get; private set; }
        public int MapId { get; private set; }
        public SceneObjectPool ObjectPool => mObjectPool;

        public void LoadScene(int mapId)
        {
            // TODO loadScene by map Id
            SceneManager.Instance.LoadScene("MainScene");
            MapId = mapId;
            mSceneCount++;
        }

        protected override void Init()
        {
            mInputCtrl = gameObject.GetComponent<UserInputCtrl>() ?? gameObject.AddComponent<UserInputCtrl>();
            mObjectPool = gameObject.GetComponent<SceneObjectPool>() ?? gameObject.AddComponent<SceneObjectPool>();
            SceneManager.Instance.AddEntry(this);

            ScheduleManager.Instance.Schedule(0.5f, SendLeaderMoving, this);
            GameConnection.Instance.OnReceivedMessage += OnReceiveMessage;
        }

        private void OnDestroy()
        {
            if (SceneManager.Instance != null)
                SceneManager.Instance.RemoveEntry(this);
        }

        private void Update()
        {
            mInputCtrl.HandleUserInput();
            LogicUpdate();
            if ((Time.frameCount % Define.AddSpriteFrameInterval) == 0)
            {
                LoadMonsterData();
            }
        }

        private void LogicUpdate()
        {
            LockObjectDic++;
            var iter = mNameToObjectDic.GetEnumerator();
            while(iter.MoveNext())
            {
                iter.Current.Value.OnUpdate();
            }
            iter.Dispose();
            LockObjectDic--;
        }

        void ISceneEntry.OnEntering(string from, string to, Dictionary<string, object> param)
        {
            mState.Transit(InitState.Start);
            var mainCamera = Camera.main;
            mCameraCtrl = mainCamera.GetComponent<ThirdPersonCameraCtrl>();
            //var cameraData = mainCamera.GetUniversalAdditionalCameraData();
            //cameraData.cameraStack.Add(UIManager.Instance.uiCamera);
        }

        int ISceneEntry.OnEnteringProcess(string from, string to, Dictionary<string, object> param)
        {
            if (to == "LoginScene")
                return 100;

            if(ModuleManager.Instance.ModuleInitializing)
            {
                return 0;
            }

            switch (mState.Tick())
            {
                case InitState.Start:
                {
                    UIManager.Instance.Open(UIName.MainUI);
                    InitLeader();
                    Addressables.InstantiateAsync($"Assets/PublicAssets/Test/TestBundle/yw_fqgc.prefab");
                    GameConnection.Instance.Send(ServerCmd.PlayGame, new PlayGame());
                    mPlayGame = true;
                    // TODO ....
                    mState.Transit(InitState.Waiting);
                    break;
                }

                case InitState.Waiting:
                {
                    if (!UIManager.Instance.IsOpened(UIName.MainUI))
                        return 0;
                    if (Leader == null || !Leader.IsReady)
                        return 0;
                    return 100;
                }
            }
            return 0;
        }

        public void OnAttackClick()
        {
            var roleModule = ModuleManager.Instance.GetModule<RoleDataModule>();
            var skillModule = ModuleManager.Instance.GetModule<SkillModule>();

            var skillData = TableGroup.ConfigTable.GetNextAttackMagic(skillModule.ContinuouslyAttackNum, roleModule.RoleDetail.Occupation);
            if (skillData == null)
            {
                Debug.LogError($"cant' find skill data AttackNum={skillModule.ContinuouslyAttackNum} Occupation={roleModule.RoleDetail.Occupation}");
                return;
            }

            Sprite target = Leader.LockTarget;
            // targetType
            if (target == null)
            {
                target = FindTargetNear(Leader.Coordinate, skillData) as Sprite;
            }

            // 檢查當前目標距離
            if (target != null)
            {
                var dir = target.Coordinate - Leader.Coordinate;
                var distance = dir.magnitude;
                if (skillModule.ContinuouslyAttackNum == 1)
                {
                    if (distance > int.Parse(skillData.AttackDistance) + target.Radius - 100) // 原廠這邊就是-100...
                    {
                        skillData = TableGroup.ConfigTable.GetSpecialMagic(SpecialMagicIndex.Walk, roleModule.RoleDetail.Occupation);
                    }
                }
                if (distance > Define.OutOfAttackRange)
                {
                    target = null;
                }
            }
            else
            {
                if (skillModule.ContinuouslyAttackNum == 1) 
                    skillData = TableGroup.ConfigTable.GetSpecialMagic(SpecialMagicIndex.Walk, roleModule.RoleDetail.Occupation);
            }

            // 檢查CD
            if (skillModule.IsCoolDown(skillData.ID))
                return;

            // TODO 是否配戴武器
            // 檢查 SkillType & 連擊
            if (skillData.SkillType == 1)
            {
                
            }

            // TODO 是否目前可以打斷
            if (!Leader.CanBreakActionByMagic())
                return;

            Leader.AttackMagic(skillData, target, skillModule.ContinuouslyAttackNum);
            if(skillData.CameraTrace)
            {
                mCameraCtrl.MoveToBack();
            }
            skillModule.ContinuouslyAttackNum++;
        }

        public void OnDodgeClick()
        {
            var roleModule = ModuleManager.Instance.GetModule<RoleDataModule>();
            var skillModule = ModuleManager.Instance.GetModule<SkillModule>();

            Magics magicData = null;
            var dir = mInputCtrl.GetCurrentDirection();
            if (!mInputCtrl.IsUserHolding())
                magicData = TableGroup.ConfigTable.GetSpecialMagic(SpecialMagicIndex.NewDodge, roleModule.RoleDetail.Occupation);
            else
                magicData = TableGroup.ConfigTable.GetSpecialMagic(SpecialMagicIndex.Dodge0, roleModule.RoleDetail.Occupation);

            // 檢查CD
            if (skillModule.IsCoolDown(magicData.ID))
                return;

            // TODO 是否目前可以打斷
            if (!Leader.CanBreakActionByDodge())
                return;

            Leader.DodgeMagic(magicData, dir);
        }
    }
}