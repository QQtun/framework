using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 特效点光源动态脚本
/// </summary>
public class PointLightEffectSet : MonoBehaviour {
    public float pointStart = 0.1f;
    public float pointMiddle = 0.1f;
    public float pointEnd = 0.4f;
    private Light pointlight;
    private float maxIntensity = 0;
    private float tempIntensity = 0;
    private float tempTime = 0;

    void Awake()
    {
        pointlight = this.GetComponent<Light>();
        maxIntensity = pointlight.intensity;
    }
	void Update () {

        tempTime += Time.deltaTime;

        if (tempTime < pointStart)
        {
            tempIntensity = maxIntensity * tempTime / pointStart;
        }
        else if (tempTime >= pointStart && tempTime <= pointStart + pointMiddle)
        {
            tempIntensity = maxIntensity;
        }
        else if (tempTime > pointStart + pointMiddle && tempTime <= pointStart + pointMiddle + pointEnd)
        {
            tempIntensity = maxIntensity * (1 - (tempTime - pointStart - pointMiddle) / pointEnd);
        }
        else
        {
            tempIntensity = 0.0f;
        }

        pointlight.intensity = tempIntensity;
    }

    void OnEnable()
    {
        tempTime = 0.0f;
        tempIntensity = 0.0f;
        pointlight.intensity = tempIntensity;
    }

    void OnDisable()
    {
        tempTime = 0.0f;
        tempIntensity = 0.0f;
        pointlight.intensity = tempIntensity;
    }

}
