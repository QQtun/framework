using Core.Framework.Utility;
using UnityEngine;

namespace Core.Framework.Event.Property.PropertyObserve.Handler.Long
{
    /// <summary>
    /// Author: chengdundeng
    /// Date: 2019/11/28
    /// Desc: 數值會使用K,M,B單位
    /// </summary>
    public class LongToKmbFormat : IBindingInfoHandler
    {
        private static readonly string HandleTypeName = LongHandleType.KmbFormat.ToString();

        public PropertyType PropertyType
        {
            get { return PropertyType.Long; }
        }

        public string HandleType
        {
            get { return HandleTypeName; }
        }

        public IPropertyListener HandleBindingInfo(GameObject gameObject, PropertyBindInfo info)
        {
            IPropertyListener<long> listener = PropertyManager.Instance.Subscribe<long>(info.propertyKey,
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