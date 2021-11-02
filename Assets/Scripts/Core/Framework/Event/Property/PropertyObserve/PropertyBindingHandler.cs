using Core.Framework.Event.Property.PropertyObserve.Handler.Bool;
using Core.Framework.Event.Property.PropertyObserve.Handler.Color;
using Core.Framework.Event.Property.PropertyObserve.Handler.DateTime;
using Core.Framework.Event.Property.PropertyObserve.Handler.Float;
using Core.Framework.Event.Property.PropertyObserve.Handler.Int;
using Core.Framework.Event.Property.PropertyObserve.Handler.Long;
using Core.Framework.Event.Property.PropertyObserve.Handler.Quaternion;
using Core.Framework.Event.Property.PropertyObserve.Handler.String;
using Core.Framework.Event.Property.PropertyObserve.Handler.TimeSpan;
using Core.Framework.Event.Property.PropertyObserve.Handler.ULong;
using Core.Framework.Event.Property.PropertyObserve.Handler.Vector2;
using Core.Framework.Event.Property.PropertyObserve.Handler.Vector3;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Framework.Event.Property.PropertyObserve
{
    public class PropertyBindingHandler
    {
        private static PropertyBindingHandler sInstance;

        private List<IBindingInfoHandler> mHandlerList;

        private PropertyBindingHandler()
        {
            mHandlerList = new List<IBindingInfoHandler>();

            // 加入各類型資料的處理方式

            mHandlerList.Add(new IntToStringHandler());
            mHandlerList.Add(new IntAddCommas());
            mHandlerList.Add(new IntToKmbFormat());
            //_handlerList.Add(new IntRedHint());
            mHandlerList.Add(new IntToBFormat());

            mHandlerList.Add(new LongToStringHandler());
            mHandlerList.Add(new LongAddCommas());
            mHandlerList.Add(new LongToKmbFormat());
            mHandlerList.Add(new LongToBFormat());

            mHandlerList.Add(new ULongToStringHandler());
            mHandlerList.Add(new ULongAddCommas());
            mHandlerList.Add(new ULongToKmbFormat());
            mHandlerList.Add(new ULongToBFormat());

            mHandlerList.Add(new FloatCeil());
            mHandlerList.Add(new FloatFloor());
            mHandlerList.Add(new FloatRound());
            mHandlerList.Add(new FloatToString());
            mHandlerList.Add(new FloatSliderPercentage());
            mHandlerList.Add(new FloatImageFillAmount());
            //_handlerList.Add(new FloatToSetValue());

            mHandlerList.Add(new StringToImageSprite());
            mHandlerList.Add(new StringToString());

            mHandlerList.Add(new BoolToString());
            mHandlerList.Add(new BoolToToggle());
            mHandlerList.Add(new BoolToVisible());
            mHandlerList.Add(new BoolToSelectable());

            mHandlerList.Add(new TimeSpanToString());

            mHandlerList.Add(new DateTimeToString());

            mHandlerList.Add(new ColorToImageColor());

            mHandlerList.Add(new QuaternionToLocalRotation());

            mHandlerList.Add(new Vector2ToAnchorPosition());

            mHandlerList.Add(new Vector3ToLocalPosition());
        }

        /// <summary>
        ///     處理在Inspector上編輯的屬性資料, 並反映到目標物件
        /// </summary>
        /// <param name="gameObject">PropertyObserver或PropertysObserver物件</param>
        /// <param name="info">Inspector上編輯的屬性綁定資料</param>
        /// <returns></returns>
        public static IPropertyListener HandleBindingInfo(GameObject gameObject, PropertyBindInfo info)
        {
            if (sInstance == null)
            {
                sInstance = new PropertyBindingHandler();
            }

            return sInstance.DoHandleBindingInfo(gameObject, info);
        }

        private IPropertyListener DoHandleBindingInfo(GameObject gameObject, PropertyBindInfo info)
        {
            List<IBindingInfoHandler>.Enumerator iter = mHandlerList.GetEnumerator();
            while (iter.MoveNext())
            {
                IBindingInfoHandler handler = iter.Current;
                if (handler == null)
                {
                    continue;
                }
                if (handler.PropertyType.ToString() == info.propertyType
                   && handler.HandleType == info.handleType)
                {
                    return handler.HandleBindingInfo(gameObject, info);
                }
            }
            iter.Dispose();

            LogUtil.Debug.LogWarningFormat("Property( type:{0}, name:{1}, handle:{2} ) don't handle!",
                info.propertyType, info.propertyKey, info.handleType, LogUtil.LogTag.EventSystem);

            return null;
        }
    }
}