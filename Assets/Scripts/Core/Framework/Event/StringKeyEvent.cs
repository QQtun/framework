namespace Core.Framework.Event
{
    public class StringKeyEvent : EventBase<StringKeyEvent>
    {
        public object Data { get; private set; }
        public string Key { get; private set; }

        public static StringKeyEvent Allocate(string key, object data)
        {
            var evt = Allocate();
            evt.Key = key;
            evt.EventTag = key;
            evt.Data = data;
            return evt;
        }
    }
}