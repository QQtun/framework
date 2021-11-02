using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamerDepthManager : MonoBehaviour {
    [HideInInspector]
    public static List<CamerDepthManager> managerList = new List<CamerDepthManager>();
    [HideInInspector]
    public Camera _camera;
    private bool _outsideControl = false;

    void Start () {
        _camera = GetComponent<Camera>();
        if(_camera == null)
        {
 //           XMLDebug.LogWarning("Camera component missing!");
        }
        SetDepthMode();
    }
	
    /// <summary>
    /// 外部调用设置摄像机深度模式
    /// </summary>
    /// <param name="d"></param>
    public void OutSideSetDepthMode(DepthTextureMode d)
    {
        if(_camera)
        {
            _camera.depthTextureMode |= d;
            _outsideControl = true;
        }
    }

    /// <summary>
    /// 外部调用回复摄像机深度模式
    /// </summary>
    public void OutSidRestoreDepthMode()
    {
        _outsideControl = false;
        SetDepthMode();
    }

    public void SetDepthMode()
    {
        if (_camera == null)
            return;
        if (_outsideControl)
            return;
        bool needDepth = SetMainCameraDepth.ExistObjsNeedDepthTexture(_camera);
        if (needDepth)
        {
            _camera.depthTextureMode |= DepthTextureMode.Depth;
        }
        else
        {
            _camera.depthTextureMode &= ~DepthTextureMode.Depth;
        }
    }

    private void OnDisable()
    {
        managerList.Remove(this);
    }

    private void OnEnable()
    {
        managerList.Add(this);
    }

    public static void AutoSetDepthMode()
    {
        for(int i=0; i< managerList.Count; i++)
        {
            CamerDepthManager manager = managerList[i];
            manager.SetDepthMode();
        }
    }
}
