using Core.Framework.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Core.Framework.Event.Property
{
    [Serializable]
    public class SubscribableList<T> : IList<T>
    {
        [SerializeField, ReadOnly]
        private List<T> mList;

        private EventSystem mEventSystem;

        public T this[int index]
        {
            get => mList[index];
            set
            {
                if (UnityEqualityComparer.GetDefault<T>().Equals(mList[index], value))
                {
                    return;
                }

                mList[index] = value;

                NotifyItemReplaced(index);
            }
        }

        public int Count => mList.Count;

        public bool IsReadOnly => false;

        private EventSystem EventSystem => mEventSystem ?? EventSystem.Instance;

        public SubscribableList()
        {
            mList = new List<T>();
        }

        public SubscribableList(EventSystem eventSystem)
        {
            mList = new List<T>();
            mEventSystem = eventSystem;
        }

        public SubscribableList(IEnumerable<T> collection, EventSystem eventSystem = null)
        {
            mList = new List<T>(collection);
            mEventSystem = eventSystem;
        }

        public SubscribableList(int capacity, EventSystem eventSystem = null)
        {
            mList = new List<T>(capacity);
            mEventSystem = eventSystem;
        }

        public void Add(T item)
        {
            mList.Add(item);
            NotifyItemAdded();
        }

        public void AddRange(IEnumerable<T> collection)
        {
            foreach (var item in collection)
            {
                Add(item);
            }
        }

        public bool Exists(Predicate<T> match)
        {
            return mList.Exists(match);
        }

        public T Find(Predicate<T> match)
        {
            return mList.Find(match);
        }

        public List<T> FindAll(Predicate<T> match)
        {
            return mList.FindAll(match);
        }

        public int FindIndex(Predicate<T> match)
        {
            return mList.FindIndex(match);
        }

        public void Clear()
        {
            var lastCount = mList.Count;
            mList.Clear();
            NotifyListCleared(lastCount);
        }

        public bool Contains(T item)
        {
            return mList.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            mList.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return mList.GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return mList.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            mList.Insert(index, item);
            NotifyItemInsert(index);
        }

        public bool Remove(T item)
        {
            var index = mList.IndexOf(item);
            if (index >= 0)
            {
                RemoveAt(index);
                return true;
            }
            return false;
        }

        public void RemoveAt(int index)
        {
            T item = default(T);
            if(index >= 0 && index < mList.Count)
            {
                item = mList[index];
            }
            mList.RemoveAt(index);
            NotifyItemRemoved(index, item);
        }

        public int RemoveAll(Predicate<T> match)
        {
            var mathList = FindAll(match);
            foreach (var item in mathList)
            {
                Remove(item);
            }
            return mathList.Count;
        }

        public bool TrueForAll(Predicate<T> match)
        {
            return mList.TrueForAll(match);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return mList.GetEnumerator();
        }

        public IEventListener<SubscribableListItemAddEvent<T>> SubscribeItemAddedEvent(
            Action<SubscribableListItemAddEvent<T>> callback)
        {
            return EventSystem?.RegisterListenerWithTag(callback, EventTagData.Tag(this));
        }
        public IEventListener<SubscribableListItemRepalceEvent<T>> SubscribeItemReplacedEvent(
            Action<SubscribableListItemRepalceEvent<T>> callback)
        {
            return EventSystem?.RegisterListenerWithTag(callback, EventTagData.Tag(this));
        }
        public IEventListener<SubscribableListItemInsertEvent<T>> SubscribeItemInsertedEvent(
            Action<SubscribableListItemInsertEvent<T>> callback)
        {
            return EventSystem?.RegisterListenerWithTag(callback, EventTagData.Tag(this));
        }
        public IEventListener<SubscribableListItemRemoveEvent<T>> SubscribeItemRemovedEvent(
            Action<SubscribableListItemRemoveEvent<T>> callback)
        {
            return EventSystem?.RegisterListenerWithTag(callback, EventTagData.Tag(this));
        }
        public IEventListener<SubscribableListClearEvent<T>> SubscribeItemClearedEvent(
            Action<SubscribableListClearEvent<T>> callback)
        {
            return EventSystem?.RegisterListenerWithTag(callback, EventTagData.Tag(this));
        }

        private void NotifyItemAdded()
        {
            EventSystem?.SendEvent(SubscribableListItemAddEvent<T>.Allocate(this).SetEventTag(EventTagData.Tag(this)));
        }

        private void NotifyItemReplaced(int index)
        {
            EventSystem?.SendEvent(SubscribableListItemRepalceEvent<T>.Allocate(this, index).SetEventTag(EventTagData.Tag(this)));
        }

        private void NotifyItemInsert(int index)
        {
            EventSystem?.SendEvent(SubscribableListItemInsertEvent<T>.Allocate(this, index).SetEventTag(EventTagData.Tag(this)));
        }

        private void NotifyItemRemoved(int index, T item)
        {
            EventSystem?.SendEvent(SubscribableListItemRemoveEvent<T>.Allocate(this, index, item).SetEventTag(EventTagData.Tag(this)));
        }

        private void NotifyListCleared(int lastCount)
        {
            EventSystem?.SendEvent(SubscribableListClearEvent<T>.Allocate(this, lastCount).SetEventTag(EventTagData.Tag(this)));
        }
    }
}