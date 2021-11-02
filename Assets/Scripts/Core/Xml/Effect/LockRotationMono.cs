using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockRotationMono : MonoBehaviour {
    public Quaternion _Rotation;
	// Use this for initialization
	void Start () {
        _Rotation = transform.localRotation;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void LateUpdate()
    {
        transform.rotation = _Rotation;
    }
}
