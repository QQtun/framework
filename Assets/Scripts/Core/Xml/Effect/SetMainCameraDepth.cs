using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetMainCameraDepth : MonoBehaviour
{
    private static Dictionary<int, int> _needDepthDict = new Dictionary<int, int>();
    private int layer;
    private void Start()
    {
        layer = gameObject.layer;
    }

    void OnBecameVisible()
    {
        if (_needDepthDict.ContainsKey(layer))
        {
            _needDepthDict[layer]++;
        }
        else
        {
            _needDepthDict.Add(layer, 1);
        }
        CamerDepthManager.AutoSetDepthMode();
    }

    void OnBecameInvisible()
    {
        if (_needDepthDict.ContainsKey(layer))
        {
            _needDepthDict[layer] = Mathf.Max(0, _needDepthDict[layer] - 1);
        }
        CamerDepthManager.AutoSetDepthMode();
    }

    public static bool ExistObjsNeedDepthTexture(Camera c)
    {
        bool exist = false;
        if (c == null)
            return exist;
        foreach (KeyValuePair<int, int> kv in _needDepthDict)
        {
            int layermask = 1 << kv.Key;
            if ((c.cullingMask & layermask) != 0 && kv.Value > 0)
            {
                exist = true;
                break;
            }
        }
        return exist;
    }
}
