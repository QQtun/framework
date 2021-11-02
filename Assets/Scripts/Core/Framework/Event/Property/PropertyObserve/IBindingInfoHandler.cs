using UnityEngine;

namespace Core.Framework.Event.Property.PropertyObserve
{
    /// <summary>
    /// Author: chengdundeng
    /// Date: 2019/11/12
    /// Desc: PropertyObserver 在inspector上 對應數值地處理方式 介面
    /// </summary>
    public interface IBindingInfoHandler
    {
        /// <summary>
        /// 資料類型
        /// </summary>
        PropertyType PropertyType { get; }

        /// <summary>
        /// 資料處理方式, 須填入PropertyType所對應到的Enum值
        /// </summary>
        string HandleType { get; }

        /// <summary>
        /// 如何處理資料並反映到目標物件
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="info">Inspector上編輯的屬性綁定資料</param>
        /// <returns></returns>
        IPropertyListener HandleBindingInfo(GameObject gameObject, PropertyBindInfo info);
    }
}