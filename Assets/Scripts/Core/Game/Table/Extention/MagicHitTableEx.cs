// <auto-generated>
//     Generated by TableExportWindow.  DO NOT EDIT!
// </auto-generated>
using System.Collections.Generic;
namespace Core.Game.Table
{
    public partial class MagicHitTable
    {
        private Dictionary<int, MagicHit> mDic = null;
        public MagicHit Get(int id)
        {
            if(mDic == null)
            {
                mDic = new Dictionary<int, MagicHit>();
                foreach(var row in Rows)
                {
                    mDic.Add(row.HitID, row);
                }
            }
            mDic.TryGetValue(id, out var data);
            return data;
        }
    }
    public partial class MagicHit
    {
    }
}
