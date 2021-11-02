using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Core.Framework.Event.Property.PropertyObserve.Handler.String
{
    /// <summary>
    /// Author: chengdundeng
    /// Date: 2019/11/29
    /// Desc: Use String To Load Sprite And Set To Image Handler
    /// </summary>
    public class StringToImageSprite : IBindingInfoHandler
    {
        private static readonly string HandleTypeName = StringHandleType.AsImageSprite.ToString();

        public PropertyType PropertyType
        {
            get
            {
                return PropertyType.String;
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
            IPropertyListener<string> listener = PropertyManager.Instance.Subscribe<string>(info.propertyKey,
                (lastValue, value) =>
                {
                    if (info.image != null)
                    {
                        Addressables.LoadAssetAsync<Sprite>(value).Completed += handle =>
                        {
                            if (handle.Result == null)
                                return;

                            info.image.sprite = handle.Result;
                        };
                    }
                });

            if (listener != null && info.image != null)
            {
                Addressables.LoadAssetAsync<Sprite>(listener.Value).Completed += handle =>
                {
                    if (handle.Result == null)
                        return;

                    info.image.sprite = handle.Result;
                };
            }

            return listener;
        }
    }
}