using UnityEngine;

namespace Core.Framework.Event.Property.PropertyObserve.Handler.Vector2
{
    /// <summary>
    /// Author: chengdundeng
    /// Date: 2019/11/29
    /// Desc: Set Vector2 To RectTransform AnchoredPosition Handler
    /// </summary>
    internal class Vector2ToAnchorPosition : IBindingInfoHandler
    {
        private static readonly string HandleTypeName = Vector2HandleType.ToAnchorPosition.ToString();

        public PropertyType PropertyType
        {
            get { return PropertyType.Vector2; }
        }

        public string HandleType
        {
            get { return HandleTypeName; }
        }

        public IPropertyListener HandleBindingInfo(GameObject gameObject, PropertyBindInfo info)
        {
            IPropertyListener<UnityEngine.Vector2> listener =
                PropertyManager.Instance.Subscribe<UnityEngine.Vector2>(info.propertyKey,
                    (lastValue, value) =>
                    {
                        if (info.rectTransform != null)
                        {
                            info.rectTransform.anchoredPosition = value;
                        }
                    });

            if (listener != null && info.rectTransform != null)
            {
                info.rectTransform.anchoredPosition = listener.Value;
            }

            return listener;
        }
    }
}