using Core.Framework.Module;
using Core.Game.Logic;
using Core.Game.Module;
using UnityEngine;
using Sprite = Core.Game.Logic.Sprite;

namespace Core.Game.Scene
{
    public partial class GameScene
    {
        private void InitLeader()
        {
            var roleModule = ModuleManager.Instance.GetModule<RoleDataModule>();
            var roleDetail = roleModule.RoleDetail;
            var go = new GameObject("Leader");
            go.SetActive(false);
            Leader = go.AddComponent<Sprite>();
            Leader.InitLeader(roleDetail);
            mInputCtrl.leader = Leader;
            mCameraCtrl.StartFallow(Leader.GameObject.transform);
            Add(Leader);
        }
    }
}