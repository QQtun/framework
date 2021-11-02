using LogUtil;

namespace Core.Framework.Network
{    
    public interface IMessageNameConverter
    {
        string Convert(int messageId);
    }

    public class MessageNameConverter 
    {
        public static IMessageNameConverter Converter { get; set; }

        public static string Convert(int messageId)
        {
            if(Converter != null)
            {
                return Converter.Convert(messageId);
            }
            Debug.LogError("Set Converter first !!");
            return messageId.ToString();
        }
    }
}