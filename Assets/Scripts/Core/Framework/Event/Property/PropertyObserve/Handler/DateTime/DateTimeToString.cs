using UnityEngine;

namespace Core.Framework.Event.Property.PropertyObserve.Handler.DateTime
{
    /// <summary>
    /// Author: chengdundeng
    /// Date: 2019/11/28
    /// Desc: DateTime To UI Text Handler
    /// </summary>
    internal class DateTimeToString : IBindingInfoHandler
    {
        private static readonly string HandleTypeName = DateTimeHandleType.ToText.ToString();

        public PropertyType PropertyType
        {
            get
            {
                return PropertyType.DateTime;
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
            IPropertyListener<System.DateTime> listener = PropertyManager.Instance.Subscribe<System.DateTime>(info.propertyKey,
                (lastValue, value) =>
                {
                    if (info.text != null)
                    {
                        if (info.isUniversal)
                        {
                            info.text.text = value.ToUniversalTime().ToString();
                        }
                        else
                        {
                            info.text.text = value.ToString();
                        }
                    }
                });

            if (listener != null && info.text != null)
            {
                if (info.isUniversal)
                {
                    info.text.text = listener.Value.ToUniversalTime().ToString();
                }
                else
                {
                    info.text.text = listener.Value.ToString();
                }
            }

            return listener;
        }
    }
}