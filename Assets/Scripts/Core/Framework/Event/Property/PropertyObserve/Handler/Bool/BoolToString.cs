using UnityEngine;

namespace Core.Framework.Event.Property.PropertyObserve.Handler.Bool
{
    /// <summary>
    /// Author: chengdundeng
    /// Date: 2019/11/28
    /// Desc: Bool To UI Text Handler
    /// </summary>
    public class BoolToString : IBindingInfoHandler
    {
        private static readonly string HandleTypeName = BoolHandleType.ToText.ToString();

        public PropertyType PropertyType
        {
            get
            {
                return PropertyType.Bool;
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
            IPropertyListener<bool> listener = PropertyManager.Instance.Subscribe<bool>(info.propertyKey,
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