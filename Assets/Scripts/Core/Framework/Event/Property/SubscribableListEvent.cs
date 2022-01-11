namespace Core.Framework.Event.Property
{
    public class SubscribableListItemAddEvent<T> : EventBase<SubscribableListItemAddEvent<T>>
    {
        public SubscribableList<T> List { get; private set; }

        public static SubscribableListItemAddEvent<T> Allocate(SubscribableList<T> list)
        {
            var ret = Allocate();
            ret.List = list;
            return ret;
        }
    }

    public class SubscribableListItemRepalceEvent<T> : EventBase<SubscribableListItemRepalceEvent<T>>
    {
        public SubscribableList<T> List { get; private set; }
        public int Index { get; private set; }

        public static SubscribableListItemRepalceEvent<T> Allocate(SubscribableList<T> list, int index)
        {
            var ret = Allocate();
            ret.List = list;
            ret.Index = index;
            return ret;
        }
    }

    public class SubscribableListItemInsertEvent<T> : EventBase<SubscribableListItemInsertEvent<T>>
    {
        public SubscribableList<T> List { get; private set; }
        public int Index { get; private set; }

        public static SubscribableListItemInsertEvent<T> Allocate(SubscribableList<T> list, int index)
        {
            var ret = Allocate();
            ret.List = list;
            ret.Index = index;
            return ret;
        }
    }

    public class SubscribableListItemRemoveEvent<T> : EventBase<SubscribableListItemRemoveEvent<T>>
    {
        public SubscribableList<T> List { get; private set; }
        public int Index { get; private set; }
        public T RemovedItem { get; private set; }

        public static SubscribableListItemRemoveEvent<T> Allocate(SubscribableList<T> list, int index, T item)
        {
            var ret = Allocate();
            ret.List = list;
            ret.Index = index;
            ret.RemovedItem = item;
            return ret;
        }
    }

    public class SubscribableListClearEvent<T> : EventBase<SubscribableListClearEvent<T>>
    {
        public SubscribableList<T> List { get; private set; }
        public int LastCount { get; private set; }

        public static SubscribableListClearEvent<T> Allocate(SubscribableList<T> list, int lastCount)
        {
            var ret = Allocate();
            ret.List = list;
            ret.LastCount = lastCount;
            return ret;
        }
    }
}
