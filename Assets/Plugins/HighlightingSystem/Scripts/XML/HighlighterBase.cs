using UnityEngine;
using System.Collections;
using HighlightingSystem;

public class HighlighterBase : MonoBehaviour
{
	protected Highlighter h;

	// 
	protected virtual void Awake()
	{
        h = GetComponent<Highlighter>();
        if (h == null) { h = gameObject.AddComponent<Highlighter>(); }
    }
	
	// 
	protected virtual void OnEnable()
	{

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

	}
}