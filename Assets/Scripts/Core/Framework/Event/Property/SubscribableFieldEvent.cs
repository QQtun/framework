namespace Core.Framework.Event.Property
{
    public class SubscribableFieldEvent<T> : EventBase<SubscribableFieldEvent<T>>
    {
        public T LastValue
        {
            get { return Sender.LastValue; }
        }

        public T Value
        {
            get { return Sender.Value; }
        }

        public ISubscribableField<T> Sender { get; private set; }

        public static SubscribableFieldEvent<T> Allocate(ISubscribableField<T> sender)
        {
            SubscribableFieldEvent<T> obj = Allocate();
            obj.Sender = sender;
            obj.EventTag = null;
            return obj;
        }
    }
}