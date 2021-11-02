using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HighlightingSystem;

public class HighlighterDistortion : HighlighterBase
{
	protected virtual void OnEnable()
	{
        base.OnEnable();
        if (h)
            h.distortion = true;
	}
	
	// 
	protected virtual void Start()
	{
		
	}
	
	// 
	protected virtual void Update()
	{

	}
	
	// 
	protected virtual void OnValidate()
	{
        base.OnValidate();
		if (h)
			h.distortion = true;
	}

    protected virtual void OnDisable()
    {

    }
}
