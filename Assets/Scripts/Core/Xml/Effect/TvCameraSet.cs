using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TvCameraSet : MonoBehaviour {
    private Camera tvcamera;
    void Awake()
    {
        tvcamera = this.GetComponent<Camera>();
    }

	void Start () {
        tvcamera.enabled = true;
    }
}
