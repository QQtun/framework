using Core.Framework.Camera.Shadow;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 此為測試文件，
/// 用來觸發ShadowProjector的
/// StartShadow
/// StopShadow
/// </summary>
public class TestShadow : MonoBehaviour
{
    //渲染角色
    public GameObject leader;

    //ShadowProjector物件
    public ShadowProjector shadow;

    //渲染角色 Layer
    public string LayerName;

    //開始渲染影子
    public bool enter = false;

    //關閉渲染影子
    public bool stop = false;

    // Update is called once per frame
    void Update()
    {
        if (enter)
        {
            // leader.transform.GetChild(0).gameObject.layer = LayerMask.NameToLayer(LayerName);
            shadow.StartShadow(leader, LayerName);
            enter = false;
        }

        if (stop)
        {
            shadow.StopShadow();
            stop = false;
        }
    }
}
