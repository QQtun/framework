using UnityEngine;
using System.Collections;

namespace HighlightingSystem
{
	static public class ShaderPropertyID
	{
		#region PUBLIC FIELDS
		// Common
		static public int _MainTex { get; private set; }

		// HighlightingSystem
		static public int _Cutoff { get; private set; }
		static public int _Cull { get; private set; }

		// Dissolve
		static public int _TinColor { get; private set; }
		static public int _DissolorTex { get; private set; }
		static public int _DissolorTexUVMoveSpeed { get; private set; }
		static public int _DissolorUvTex { get; private set; }
		static public int _DissolorUvSpeed { get; private set; }
		static public int _RAmount { get; private set; }
		static public int _DissolorWith { get; private set; }
		static public int _DissColor { get; private set; }
		static public int _Emission { get; private set; }
		static public int _DissColorIlluminate { get; private set; }
		static public int _MaskThreshold { get; private set; }
		static public int _MaskTex { get; private set; }

		// GlowMask
		static public int _MainTexUvSpeed { get; private set; }
		static public int _BaseAlphaTex { get; private set; }
		static public int _BaseMoveSpeed { get; private set; }
		static public int _MaskMapTex { get; private set; }
		static public int _MaskUVSpeed { get; private set; }
		static public int _Color { get; private set; }
		static public int _ConstantColor { get; private set; }
		static public int _GlowScale { get; private set; }
		static public int _Param { get; private set; }

		// Distortion

		static public int _NoiseTex { get; private set; }
		static public int _HeatForce { get; private set; }
		//static public int _MaskTex { get; private set; }

		// Emission
		static public int _SelfIllumin { get; private set; }

		// Glow
		static public int _EmissionThreshold { get; private set; }

		#endregion

		#region PRIVATE FIELDS
		static private bool initialized = false;
		#endregion

		// 
		static public void Initialize()
		{
			if (initialized) { return; }

			_MainTex = Shader.PropertyToID("_MainTex");
			_Cutoff = Shader.PropertyToID("_Cutoff");
			_Cull = Shader.PropertyToID("_Cull");

			_TinColor = Shader.PropertyToID("_TinColor");
			_DissolorTex = Shader.PropertyToID("_DissolorTex");
			_DissolorTexUVMoveSpeed = Shader.PropertyToID("_DissolorTexUVMoveSpeed");			
			_DissolorUvTex = Shader.PropertyToID("_DissolorUvTex");
			_DissolorUvSpeed = Shader.PropertyToID("_DissolorUvSpeed");
			_RAmount = Shader.PropertyToID("_RAmount");			
			_DissolorWith = Shader.PropertyToID("_DissolorWith");
			_DissColor = Shader.PropertyToID("_DissColor");
			_Emission = Shader.PropertyToID("_Emission");			
			_DissColorIlluminate = Shader.PropertyToID("_DissColorIlluminate");
			_MaskThreshold = Shader.PropertyToID("_MaskThreshold");
			_MaskTex = Shader.PropertyToID("_MaskTex");			

			_MainTexUvSpeed = Shader.PropertyToID("_MainTexUvSpeed");
			_BaseAlphaTex = Shader.PropertyToID("_BaseAlphaTex");			
			_BaseMoveSpeed = Shader.PropertyToID("_BaseMoveSpeed");
			_MaskMapTex = Shader.PropertyToID("_MaskMapTex");
			_MaskUVSpeed = Shader.PropertyToID("_MaskUVSpeed");			
			_Color = Shader.PropertyToID("_Color");
			_ConstantColor = Shader.PropertyToID("_ConstantColor");
			_GlowScale = Shader.PropertyToID("_GlowScale");			
			_Param = Shader.PropertyToID("_Param");

			_NoiseTex = Shader.PropertyToID("_NoiseTex");
			_HeatForce = Shader.PropertyToID("_HeatForce");			

			_SelfIllumin = Shader.PropertyToID("_SelfIllumin");

			_EmissionThreshold = Shader.PropertyToID("_EmissionThreshold");

			initialized = true;
		}
	}
}