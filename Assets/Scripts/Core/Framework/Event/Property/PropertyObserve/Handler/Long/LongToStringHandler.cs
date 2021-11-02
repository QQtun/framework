using UnityEngine;

namespace Core.Framework.Event.Property.PropertyObserve.Handler.Long
{
    /// <summary>
    /// Author: chengdundeng
    /// Date: 2019/11/28
    /// Desc: Long To UI Text Handler
    /// </summary>
    public class LongToStringHandler : IBindingInfoHandler
    {
        private static readonly string HandleTypeName = LongHandleType.ToText.ToString();

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
                        info.text.text = value.ToString(info.toStringForamt);
                    }
                });

            if (listener != null && info.text != null)
            {
                info.text.text = listener.Value.ToString(info.toStringForamt);
            }

            return listener;
        }
    }
}