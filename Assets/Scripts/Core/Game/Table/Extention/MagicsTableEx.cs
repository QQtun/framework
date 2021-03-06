// <auto-generated>
//     Generated by TableExportWindow.  DO NOT EDIT!
// </auto-generated>
using System.Collections.Generic;
namespace Core.Game.Table
{
    public partial class MagicsTable
    {
        private Dictionary<uint, Magics> mDic = null;
        public Magics Get(uint id)
        {
            if(mDic == null)
            {
                mDic = new Dictionary<uint, Magics>();
                foreach(var row in Rows)
                {
                    mDic.Add(row.ID, row);
                }
            }
            mDic.TryGetValue(id, out var data);
            return data;
        }
    }
    public partial class Magics
    {
        private MagicHit mHitId = null;
        public MagicHit HitIdSetting
        {
            get 
            {
                if(mHitId == null)
                {
                    mHitId = TableGroup.MagicHitTable.Get(HitId);
                }
                return mHitId;
            }
        }
    }
}
