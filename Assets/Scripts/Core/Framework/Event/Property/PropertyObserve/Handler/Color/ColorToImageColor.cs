using UnityEngine;

namespace Core.Framework.Event.Property.PropertyObserve.Handler.Color
{
    /// <summary>
    /// Author: chengdundeng
    /// Date: 2019/11/28
    /// Desc: Color To Image Color Handler
    /// </summary>
    public class ColorToImageColor : IBindingInfoHandler
    {
        private static readonly string HandleTypeName = ColorHandleType.ToImageColor.ToString();

        public PropertyType PropertyType
        {
            get { return PropertyType.Color; }
        }

        public string HandleType
        {
            get { return HandleTypeName; }
        }

        public IPropertyListener HandleBindingInfo(GameObject gameObject, PropertyBindInfo info)
        {
            IPropertyListener<UnityEngine.Color> listener =
                PropertyManager.Instance.Subscribe<UnityEngine.Color>(info.propertyKey,
                    (lastValue, value) =>
                    {
                        if (info.image != null)
                        {
                            info.image.color = value;
                        }
                    });

            if (listener != null && info.image != null)
            {
                info.image.color = listener.Value;
            }

            return listener;
        }
    }
}