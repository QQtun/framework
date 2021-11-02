using Core.Framework.Utility;
using UnityEngine;

namespace Core.Framework.Event.Property.PropertyObserve.Handler.Long
{
    /// <summary>
    /// Author: chengdundeng
    /// Date: 2019/11/28
    /// Desc: Long To String With Commas As To UI Text Text Handler
    /// </summary>
    public class LongAddCommas : IBindingInfoHandler
    {
        private static readonly string HandleTypeName = LongHandleType.AddCommas.ToString();

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
                        info.text.text = StringConverter.ConvertIntToStringWithCommas(value);
                    }
                });

            if (listener != null && info.text != null)
            {
                info.text.text = StringConverter.ConvertIntToStringWithCommas(listener.Value);
            }

            return listener;
        }
    }
}