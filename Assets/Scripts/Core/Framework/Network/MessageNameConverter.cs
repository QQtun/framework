using LogUtil;

namespace Core.Framework.Network
{    
    public interface IMessageNameConverter
    {
        string Convert(int messageId);
    }

    public class MessageNameConverter 
    {
        public static IMessageNameConverter Delegate { get; set; }

        public static string Convert(int messageId)
        {
            if(Delegate != null)
            {
                return Delegate.Convert(messageId);
            }
            //Debug.LogError("Set Converter first !!");
            return messageId.ToString();
        }
    }
}