using Core.Game.Logic;
using Google.Protobuf.Collections;

namespace Core.Game.Table
{
    public partial class ConfigTable
    {
        public Magics GetNextAttackMagic(int curAttackNum, int occupation)
        {
            RepeatedField<int> list = null;
            if (occupation == 1)
                 list = SpecialMagic1;

            int magicId = -1;
            int index = (int)SpecialMagicIndex.Attack1 + curAttackNum - 1;
            if (list != null && list.Count > index)
            {
                magicId = list[index];
            }
            if(magicId > 0)
            {
                return TableGroup.MagicsTable.Get((uint)magicId);
            }
            return null;
        }

        public Magics GetSpecialMagic(SpecialMagicIndex index, int occupation)
        {
            RepeatedField<int> list = null;
            if (occupation == 1)
                list = SpecialMagic1;

            int magicId = -1;
            if (list != null && list.Count > (int)index)
            {
                magicId = list[(int)index];
            }
            if (magicId > 0)
            {
                return TableGroup.MagicsTable.Get((uint)magicId);
            }
            return null;
        }
    }
}
