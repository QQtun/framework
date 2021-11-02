using Core.Framework.Network;
using System.Collections.Generic;

namespace Core.Game.Network
{
    public class MessageID2String : IMessageNameConverter
    {
        private Dictionary<int, string> mIdToName = new Dictionary<int, string>();

        public string Convert(int messageId)
        {
            if (mIdToName.TryGetValue(messageId, out var name))
                return name;

            ServerCmd serverCmd = (ServerCmd)messageId;
            name = serverCmd.ToString();
            mIdToName.Add(messageId, name);
            return name;
        }
    }
}