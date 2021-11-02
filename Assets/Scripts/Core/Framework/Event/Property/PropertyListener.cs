using System;

namespace Core.Framework.Event.Property
{
    /// <summary>
    /// Author: chengdundeng
    /// Date: 2019/11/27
    /// Desc: 可訂閱Field介面
    /// </summary>
    public partial class PropertyManager
    {
        private partial class PropertyListener<T>
        {
            private IEventListener<SubscribableFieldEvent<T>> mEventListener;

            private Action<T, T> mOnPropertyChange;
            private ISubscribableField<T> mSubscribableField;

            public T LastValue
            {
                get { return mSubscribableField != null ? mSubscribableField.LastValue : default(T); }
            }

            public T Value
            {
                get { return mSubscribableField != null ? mSubscribableField.Value : default(T); }
            }

            public bool IsUnsubscribed { get; private set; }

            public string PropertyName { get; private set; }

            public PropertyListener(string propertyName, Action<T, T> onPropertyChange)
            {
                IsUnsubscribed = false;
                PropertyName = propertyName;
                mOnPropertyChange = onPropertyChange;
            }

            public void Unsubscribe()
            {
                IsUnsubscribed = true;

                if (mEventListener != null)
                {
                    mEventListener.Unregister();
                    mEventListener = null;
                }

                mOnPropertyChange = null;
                mSubscribableField = null;

                Instance.Unsubscribe(this);
            }

            public void Subscribe(ISubscribableField field)
            {
                Subscribe(field as ISubscribableField<T>);
            }

            public void Subscribe(ISubscribableField<T> field)
            {
                if (field == null)
                {
                    return;
                }

                if (IsUnsubscribed)
                {
                    return;
                }

                if (mOnPropertyChange == null)
                {
                    return;
                }

                if (mSubscribableField != null)
                {
                    // 重新綁定 field
                    mEventListener.Unregister();
                    mSubscribableField = null;
                    mEventListener = null;
                }

                mSubscribableField = field;

                mEventListener = field.AddValueChangeListener(
                    (pre, cur) =>
                    {
                        if (mOnPropertyChange == null)
                        {
                            return;
                        }
                        mOnPropertyChange.Invoke(pre, cur);
                    });
            }
        }
    }
}