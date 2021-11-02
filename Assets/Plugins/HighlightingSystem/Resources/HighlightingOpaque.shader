// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/Highlighted/Opaque"
{
	Properties
	{

	}
	
	SubShader
	{
		Pass
		{
			Lighting Off
			Fog { Mode Off }
			ZWrite On
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"
			
			struct appdata_vert
			{
				float4 vertex : POSITION;
			};
			
			float4 vert(appdata_vert v) : SV_POSITION
			{
				return UnityObjectToClipPos(v.vertex);
			}
			
			fixed4 frag() : SV_Target
			{
				return fixed4(0,0,0,1);
			}
			ENDCG
		}
	}
	Fallback Off
}