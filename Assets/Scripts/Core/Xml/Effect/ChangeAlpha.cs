using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CurveInfo
{
    public string name;
    public AnimationCurve curve;
}
/// <summary>
/// 用于动态改变技能特效中的模型的透明度
/// </summary>
public class ChangeAlpha : MonoBehaviour {
    /// <summary>
    /// 初始的透明度
    /// </summary>
    [Range(0,1)]
    public float initAlpha = 0;
    /// <summary>
    /// 用于调整透明度改变曲线
    /// </summary>
    public AnimationCurve defaultCurve;
    /// <summary>
    /// 改变取消数组，可以每一个动作对应一个
    /// </summary>
    public CurveInfo[] alphaCurveArray;
    /// <summary>
    /// 当前Curve
    /// </summary>
    AnimationCurve _curCurve;
    /// <summary>
    /// 当前的动作的长度
    /// </summary>
    float _curClipLength;
    /// <summary>
    /// 所有渲染组件列表
    /// </summary>
    List<Material> _matList = new List<Material>();
    /// <summary>
    /// 创建时间，用于计算当前的透明度
    /// </summary>
    [HideInInspector]
    public float _startTime = 0;

    Animation _animation;
    string _curClipName;
    List<AnimationState> _stateList = new List<AnimationState>();

    private void OnEnable()
    {
        _startTime = Time.time;
        _curClipName = null;
    }

    void Start ()
    {
        _animation = GetComponent<Animation>();
        _curClipName = null;
        _stateList.Clear();
        foreach (AnimationState s in _animation)
        {
            _stateList.Add(s);
        }
        //_curCurve = GetCurve(_animation.clip.name);
        //_curClipLength = _animation.clip.length;

        _startTime = Time.time;
        _matList.Clear();
        Renderer[] allRender = GetComponentsInChildren<Renderer>();
        for (int i = 0; i < allRender.Length; i++)
        {
            for (int j = 0; j < allRender[i].sharedMaterials.Length; j++)
            {
                Material m = allRender[i].sharedMaterials[j];
                if (m)
                {
                    _matList.Add(m);
                }
            }
        }
        SetAlpha(initAlpha);
	}

    AnimationState GetCurAniName()
    {
        if (_animation.isPlaying == false)
            return null;
        for (int i = 0; i < _stateList.Count; i++)
        {
            if (_animation.IsPlaying(_stateList[i].name))
            {
                return _stateList[i];
            }
        }
        return null;
    }
	
    void Update()
    {
        if (_animation.isPlaying == false)
            return;

        if(_curClipName == null)
        {
            AnimationState aniState = GetCurAniName();
            _curClipName = aniState.name;
            _curCurve = GetCurve(aniState.name);
            _curClipLength = aniState.clip.length;
            _startTime = Time.time;
        }

        float t = Time.time - _startTime;
        float tPercent = t / _curClipLength;
        if (tPercent > 1)
            return;


        float alpha = _curCurve.Evaluate(tPercent);
        //if (_curClipName == "b_n_attack_01")
        //{
        //    Debug.LogErrorFormat("ID:{0}, t:{1}, length:{2}, per:{3}, alpha:{4}", GetInstanceID(), t, _curClipLength, tPercent, alpha);
        //}
        SetAlpha(alpha);
	}

    void SetAlpha(float a)
    {
        for (int i = 0; i < _matList.Count; i++)
        {
            if (_matList[i])
            {
                if (_matList[i].HasProperty("_Alpha"))
                {
                    _matList[i].SetFloat("_Alpha", a);
                    }
                }
            }
        }

    AnimationCurve GetCurve(string name)
    {
        for (int i = 0; i < alphaCurveArray.Length; i++)
        {
            if (alphaCurveArray[i].name == name)
            {
                return alphaCurveArray[i].curve;
            }
        }
        return defaultCurve;
    }
}
