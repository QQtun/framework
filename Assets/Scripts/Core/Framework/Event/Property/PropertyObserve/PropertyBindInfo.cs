using System;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Framework.Event.Property.PropertyObserve
{
    /////////////////////////////////
    /// 能處理的資料類型
    public enum PropertyType
    {
        None,

        Int,
        Long,
        Float,
        String,
        Bool,
        TimeSpan,
        DateTime,
        Vector3,
        Vector2,
        Color,
        Quaternion,
        ULong,
        WorldMapPosition,
    }

    /////////////////////////////////
    /// 各資料類型的處理方式
    public enum BoolHandleType
    {
        None,

        ToText,
        ToToggle,
        Visible,
        IsInteractable,
        IsPlayAudio,
    }

    public enum DateTimeHandleType
    {
        None,

        ToText
    }

    public enum FloatHandleType
    {
        None,

        ToText,
        Floor,
        Ceiling,
        Round,
        SliderPercentage,
        ImageFillAmount,
        SetFloatValue
    }

    public enum IntHandleType
    {
        None,

        ToText,
        AddCommas,
        KmbFormat,
        RedHint,
        BFormatOrAddCommans
    }

    public enum LongHandleType
    {
        None,

        ToText,
        AddCommas,
        KmbFormat,
        BFormatOrAddCommans
    }

    public enum ULongHandleType
    {
        None,

        ToText,
        AddCommas,
        KmbFormat,
        BFormatOrAddCommas
    }

    public enum StringHandleType
    {
        None,

        ToText,
        AsImageSprite
    }

    public enum TimeSpanHandleType
    {
        None,

        ToText
    }

    public enum Vector3HandleType
    {
        None,

        ToLocalPosition,
    }

    public enum Vector2HandleType
    {
        None,

        ToAnchorPosition,
    }

    public enum ColorHandleType
    {
        None,

        ToImageColor,
    }

    public enum QuaternionHandleType
    {
        None,

        ToLocalRotation,
    }

    public enum WorldMapPositionHandleType
    {
        None,

        ToText
    }

    /////////////////////////////////
    /// Inspector上編輯的屬性綁定資料
    [Serializable]
    public class PropertyBindInfo
    {
        public string propertyKey;
        public string propertyType;
        public string handleType;

        public GameObject gameObject;
        public Image image;
        public Slider slider;
        public Text text;
        public Toggle toggle;
        public Transform transform;
        public RectTransform rectTransform;
        public Text postFix;

        public Selectable selectable;
        public string toStringForamt;
        public bool isUniversal;
    }
}