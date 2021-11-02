using Core.Framework.Utility;
using UnityEngine;

namespace Core.Framework.Event.Property.PropertyObserve.Handler.ULong
{
    /// <summary>
    /// Author: chengdundeng
    /// Date: 2019/11/29
    /// Desc: ULong To String With Commas As To UI Text Text Handler
    /// </summary>
    public class ULongAddCommas : IBindingInfoHandler
    {
        private static readonly string HandleTypeName = ULongHandleType.AddCommas.ToString();

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