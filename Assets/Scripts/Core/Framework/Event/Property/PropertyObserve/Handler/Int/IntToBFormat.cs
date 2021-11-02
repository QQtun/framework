using Core.Framework.Utility;
using UnityEngine;

namespace Core.Framework.Event.Property.PropertyObserve.Handler.Int
{
    /// <summary>
    /// Author: chengdundeng
    /// Date: 2019/11/28
    /// Desc: 數值超過10億會已B為單位, 以下為加上千分位逗號
    /// </summary>
    public class IntToBFormat : IBindingInfoHandler
    {
        private static readonly string HandleTypeName = IntHandleType.BFormatOrAddCommans.ToString();

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
                        info.text.text = StringConverter.ConvertToBOrAddCommas(value);
                    }
                });

            if (listener != null && info.text != null)
            {
                info.text.text = StringConverter.ConvertToBOrAddCommas(listener.Value);
            }

            return listener;
        }
    }
}