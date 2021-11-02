using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace HighlightingSystem
{
	public partial class Highlighter : MonoBehaviour
	{
		// Only these types of Renderers will be highlighted
		static public readonly List<System.Type> types = new List<System.Type>()
		{
			typeof(MeshRenderer), 
			typeof(SkinnedMeshRenderer), 
			typeof(SpriteRenderer), 
			//typeof(ParticleRenderer), 
			typeof(ParticleSystemRenderer), 
		};

		/// <summary>
		/// Renderers reinitialization. 
		/// Call this method if your highlighted object has changed it's materials, renderers or child objects.
		/// Can be called multiple times per update - renderers reinitialization will occur only once.
		/// </summary>
		public void ReinitMaterials()
		{
			renderersDirty = true;
		}
	
		/// <summary>
		/// Destroy this Highlighter component.
		/// </summary>
		public void Die()
		{
			Destroy(this);
		}

	}
}