using Core.Framework.Module;
using Core.Framework.Network;
using Core.Framework.Network.Data;
using Core.Game.Table;
using Core.Game.Utility;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Game.Module
{
    [Module]
    public class SkillModule : NetworkModule
    {
        private struct CDItem
        {
            public uint id;
            public long startTick;
            public long cooldownTick;

            public long RemainingTick()
            {
                var now = ServerTimeUtil.MillisecondNow;
                var ret = cooldownTick - (now - startTick);
                return ret > 0 ? ret : 0;
            }
        }

        private List<int> mDependencies = new List<int>()
        {
        };

        private List<int> mMsgIds = new List<int>()
        {
        };

        private Dictionary<uint, CDItem> mSkillIdToCDItemDic = new Dictionary<uint, CDItem>();

        private int mContinuouslyAttackNum = 1;
        private float mLastAttackTime = float.MinValue;

        public override int Id => (int)ModuleId.SkillModule;

        public int ContinuouslyAttackNum
        {
            get
            {
                if (Time.time > mLastAttackTime + Logic.Define.ContinuouslyAttackInterval)
                    mContinuouslyAttackNum = 1;
                return mContinuouslyAttackNum;
            }
            set
            {
                mContinuouslyAttackNum = value;
                mLastAttackTime = Time.time;

                if (mContinuouslyAttackNum > 4)
                    mContinuouslyAttackNum = ((mContinuouslyAttackNum - 1) % 4) + 1;
                if (mContinuouslyAttackNum < 1)
                    mContinuouslyAttackNum = 1;
            }
        }

        public override List<int> DependencyModules()
        {
            return mDependencies;
        }

        public override List<int> GetMessageIds()
        {
            return mMsgIds;
        }

        public override void OnApplicationPause(bool pauseStatus)
        {
        }

        public override void OnConnected()
        {
        }

        public override void OnDestroy()
        {
        }

        public override void OnDisconnected(DisconnectReason reason)
        {
        }

        public override void OnGameInit()
        {
        }

        public override void OnReceivedMessage(Message msg)
        {
        }

        public override void OnStart()
        {
        }

        public override void OnUpdate()
        {
        }

        public bool IsCoolDown(uint skillId)
        {
            if(mSkillIdToCDItemDic.TryGetValue(skillId, out var item))
            {
                var now = ServerTimeUtil.MillisecondNow;
                if (now > item.startTick + item.cooldownTick)
                    return false;
                return true;
            }
            return false;
        }

        public bool AddCoolDown(uint skillId)
        {
            var skillData = TableGroup.MagicsTable.Get(skillId);
            if(skillData != null)
            {
                var item = new CDItem()
                {
                    id = skillId,
                    startTick = ServerTimeUtil.MillisecondNow,
                    cooldownTick = skillData.CDTime * 1000
                };

                if (item.cooldownTick > 0)
                {
                    mSkillIdToCDItemDic[skillId] = item;
                    return true;
                }
            }
            return false;
        }
    }
}