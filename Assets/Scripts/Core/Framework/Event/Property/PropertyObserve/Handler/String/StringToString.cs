using UnityEngine;

namespace Core.Framework.Event.Property.PropertyObserve.Handler.String
{
    /// <summary>
    /// Author: chengdundeng
    /// Date: 2019/11/29
    /// Desc: Set String To UI Text Handler
    /// </summary>
    public class StringToString : IBindingInfoHandler
    {
        private static readonly string HandleTypeName = StringHandleType.ToText.ToString();

        public PropertyType PropertyType
        {
            get
            {
                return PropertyType.String;
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
            IPropertyListener<string> listener = PropertyManager.Instance.Subscribe<string>(info.propertyKey,
                (lastValue, value) =>
                {
                    if (info.text != null)
                    {
                        info.text.text = value.ToString();
                    }
                });

            if (listener != null && info.text != null)
            {
                info.text.text = listener.Value;
            }

            return listener;
        }
    }
}