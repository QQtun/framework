using Core.Framework.Utility;
using UnityEngine;

namespace Core.Framework.Event.Property.PropertyObserve.Handler.Int
{
    /// <summary>
    /// Author: chengdundeng
    /// Date: 2019/11/28
    /// Desc: 數值會使用K,M,B單位
    /// </summary>
    public class IntToKmbFormat : IBindingInfoHandler
    {
        private static readonly string HandleTypeName = IntHandleType.KmbFormat.ToString();

        public PropertyType PropertyType
        {
            get
            {
                return PropertyType.Int;
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
            IPropertyListener<int> listener = PropertyManager.Instance.Subscribe<int>(info.propertyKey,
                (lastValue, value) =>
                {
                    if (info.text != null)
                    {
                        info.text.text = StringConverter.ConvertToKmb(value);
                    }
                });

            if (listener != null && info.text != null)
            {
                info.text.text = StringConverter.ConvertToKmb(listener.Value);
            }

            return listener;
        }
    }
}