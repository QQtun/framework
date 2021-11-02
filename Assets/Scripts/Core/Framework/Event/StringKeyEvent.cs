namespace Core.Framework.Event
{
    public class StringKeyEvent : EventBase<StringKeyEvent>
    {
        public object Data { get; private set; }

        public static StringKeyEvent Allocate(string key, object data)
        {
            var evt = Allocate();
            evt.EventTag = key;
            evt.Data = data;
            return evt;
        }
    }
}