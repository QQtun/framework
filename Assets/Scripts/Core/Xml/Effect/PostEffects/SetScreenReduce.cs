using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SetScreenReduce : MonoBehaviour {

	// Use this for initialization
	void Start () {
#if !CLIENT
        Shader.SetGlobalFloat("_screenReduce", 1);		
#endif

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
