using Core.Framework.Utility;
using UnityEngine;

namespace Core.Framework.Event.Property.PropertyObserve.Handler.ULong
{
    /// <summary>
    /// Author: chengdundeng
    /// Date: 2019/11/29
    /// Desc: 數值會使用K,M,B單位
    /// </summary>
    public class ULongToKmbFormat : IBindingInfoHandler
    {
        private static readonly string HandleTypeName = ULongHandleType.KmbFormat.ToString();

        public PropertyType PropertyType
        {
            get { return PropertyType.ULong; }
        }

        public string HandleType
        {
            get { return HandleTypeName; }
        }

        public IPropertyListener HandleBindingInfo(GameObject gameObject, PropertyBindInfo info)
        {
            IPropertyListener<ulong> listener = PropertyManager.Instance.Subscribe<ulong>(info.propertyKey,
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