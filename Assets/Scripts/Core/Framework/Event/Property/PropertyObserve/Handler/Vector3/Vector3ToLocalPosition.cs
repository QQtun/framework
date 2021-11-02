using UnityEngine;

namespace Core.Framework.Event.Property.PropertyObserve.Handler.Vector3
{
    /// <summary>
    /// Author: chengdundeng
    /// Date: 2019/11/29
    /// Desc: Set Vector3 To RectTransform AnchoredPosition Handler
    /// </summary>
    public class Vector3ToLocalPosition : IBindingInfoHandler
    {
        private static readonly string HandleTypeName = Vector3HandleType.ToLocalPosition.ToString();

        public PropertyType PropertyType
        {
            get { return PropertyType.Vector3; }
        }

        public string HandleType
        {
            get { return HandleTypeName; }
        }

        public IPropertyListener HandleBindingInfo(GameObject gameObject, PropertyBindInfo info)
        {
            IPropertyListener<UnityEngine.Vector3> listener =
                PropertyManager.Instance.Subscribe<UnityEngine.Vector3>(info.propertyKey,
                    (lastValue, value) =>
                    {
                        if (info.transform != null)
                        {
                            info.transform.localPosition = value;
                        }
                    });

            if (listener != null && info.transform != null)
            {
                info.transform.localPosition = listener.Value;
            }

            return listener;
        }
    }
}