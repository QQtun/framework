namespace Core.Framework.Event
{
    /// <summary>
    /// Author: chengdundeng
    /// Date: 2019/11/27
    /// Desc: 因為使用者經常使用int當作EventTag但是又不希望int轉object而產生垃圾，所以在這邊做類別隱性轉換
    /// </summary>
    public struct EventTagData
    {
        public enum Type
        {
            None, Value, Object
        }

        public static readonly EventTagData None = new EventTagData();

        public Type type;
        public object tagObject;
        public int tagValue;
        public int hashCode;

        public static EventTagData Tag(int value)
        {
            return new EventTagData()
            {
                type = Type.Value,
                tagValue = value,
                hashCode = value,
            };
        }

        public static EventTagData Tag(object value)
        {
            if (value == null)
            {
                return None;
            }

            return new EventTagData()
            {
                type = Type.Object,
                tagObject = value,
                hashCode = value.GetHashCode()
            };
        }

        public static implicit operator EventTagData(string value)
        {
            return Tag(value);
        }

        public static implicit operator EventTagData(int value)
        {
            return Tag(value);
        }
    }
}