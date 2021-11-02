using UnityEngine;

namespace Core.Framework.Event.Property.PropertyObserve.Handler.Int
{
    /// <summary>
    /// Author: chengdundeng
    /// Date: 2019/11/28
    /// Desc: Int To UI Text Handler
    /// </summary>
    public class IntToStringHandler : IBindingInfoHandler
    {
        private static readonly string HandleTypeName = IntHandleType.ToText.ToString();

        public PropertyType PropertyType
        {
            get { return PropertyType.Int; }
        }

        public string HandleType
        {
            get { return HandleTypeName; }
        }

        public IPropertyListener HandleBindingInfo(GameObject gameObject, PropertyBindInfo info)
        {
            IPropertyListener<int> listener = PropertyManager.Instance.Subscribe<int>(info.propertyKey,
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