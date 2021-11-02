using UnityEngine;

namespace Core.Framework.Event.Property.PropertyObserve.Handler.Float
{
    /// <summary>
    /// Author: chengdundeng
    /// Date: 2019/11/28
    /// Desc: Float To Be Image Fill Value Handler
    /// </summary>
    internal class FloatImageFillAmount : IBindingInfoHandler
    {
        private static readonly string HandleTypeName = FloatHandleType.ImageFillAmount.ToString();

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
                    if (info.image != null)
                    {
                        info.image.fillAmount = value;
                    }
                });

            if (listener != null && info.image != null)
            {
                info.image.fillAmount = listener.Value;
            }

            return listener;
        }
    }
}