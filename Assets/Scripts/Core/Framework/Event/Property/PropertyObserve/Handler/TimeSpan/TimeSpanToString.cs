using UnityEngine;

namespace Core.Framework.Event.Property.PropertyObserve.Handler.TimeSpan
{
    /// <summary>
    /// Author: chengdundeng
    /// Date: 2019/11/29
    /// Desc: Set TimeSpan To UI Text Handler
    /// </summary>
    public class TimeSpanToString : IBindingInfoHandler
    {
        private static readonly string HandleTypeName = TimeSpanHandleType.ToText.ToString();

        public PropertyType PropertyType
        {
            get
            {
                return PropertyType.TimeSpan;
            }
        }

        public string HandleType
        {
            get
            {
                return HandleTypeName;
            }
        }

        public IPropertyListener HandleBindingInfo(GameObject gameObject, PropertyBindInfo info)
        {
            IPropertyListener<System.TimeSpan> listener = PropertyManager.Instance.Subscribe<System.TimeSpan>(info.propertyKey,
                (lastValue, value) =>
                {
                    if (info.text != null)
                    {
                        info.text.text = value.ToString();
                    }
                });

            if (listener != null && info.text != null)
            {
                info.text.text = listener.Value.ToString();
            }

            return listener;
        }
    }
}