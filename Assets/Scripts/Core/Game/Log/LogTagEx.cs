using LogUtil;

namespace Core.Game.Log
{
    public abstract partial class LogTagEx : LogTag
    {
        public static readonly LogTag Resource = new LogTagType.CustomLogTag("Resource");
        public static readonly LogTag Effect = new LogTagType.CustomLogTag("Effect");
        public static readonly LogTag Audio = new LogTagType.CustomLogTag("Audio");
        public static readonly LogTag Time = new LogTagType.CustomLogTag("Time");
        public static readonly LogTag Loading = new LogTagType.CustomLogTag("Loading");
        public static readonly LogTag UI = new LogTagType.CustomLogTag("UI");
        public static readonly LogTag Lua = new LogTagType.CustomLogTag("Lua");

        protected LogTagEx(string tag) : base(tag)
        {
        }
    }

    namespace LogTagType
    {
        internal class CustomLogTag : LogTagEx
        {
            public CustomLogTag(string tag) : base(tag)
            {
            }
        }
    }
}