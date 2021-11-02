using UnityEngine;
using System.Collections;
using HighlightingSystem;

public class HighlighterOccluder : HighlighterBase
{
	// 
	protected override void OnEnable()
	{
		base.OnEnable();
        if(h)
		    h.occluder = true;
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
			h.occluder = true;
	}
}
