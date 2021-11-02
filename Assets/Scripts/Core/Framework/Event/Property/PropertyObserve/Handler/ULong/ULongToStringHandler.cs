using UnityEngine;

namespace Core.Framework.Event.Property.PropertyObserve.Handler.ULong
{
    /// <summary>
    /// Author: chengdundeng
    /// Date: 2019/11/29
    /// Desc: ULong To UI Text Handler
    /// </summary>
    public class ULongToStringHandler : IBindingInfoHandler
    {
        private static readonly string HandleTypeName = ULongHandleType.ToText.ToString();

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