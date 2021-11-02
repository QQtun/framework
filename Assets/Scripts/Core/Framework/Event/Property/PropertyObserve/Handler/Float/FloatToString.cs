using UnityEngine;

namespace Core.Framework.Event.Property.PropertyObserve.Handler.Float
{
    /// <summary>
    /// Author: chengdundeng
    /// Date: 2019/11/28
    /// Desc: Float To UI Text Handler
    /// </summary>
    public class FloatToString : IBindingInfoHandler
    {
        private static readonly string HandleTypeName = FloatHandleType.ToText.ToString();

        public PropertyType PropertyType
        {
            get
            {
                return PropertyType.Float;
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
            IPropertyListener<float> listener = PropertyManager.Instance.Subscribe<float>(info.propertyKey,
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