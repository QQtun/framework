// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/Highlighted/Glow" {
	Properties {
		[HideInInspector] _MainTex ("Base (RGB)", 2D) = "white" {}
		[HideInInspector] _EmissionThreshold ("EmissionThreshold", Range(0, 0.98)) = 0.98
	}
	SubShader {
		ColorMask R
		Pass 
		{
			Lighting Off
			Fog { Mode Off }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
			half _EmissionThreshold;

			struct a2v {
				float4 vertex : POSITION;
				float4 texcoord : TEXCOORD0;
			}; 
			
			struct v2f {
				float4 pos : POSITION;
				half2 uv : TEXCOORD0;
			};
			
			v2f vert (a2v v) {
				v2f o;							
				o.pos = UnityObjectToClipPos( v.vertex);
				o.uv = TRANSFORM_TEX (v.texcoord, _MainTex);
				return o;
			}
			
			fixed4 frag(v2f i) : COLOR { 	
				return fixed4( _EmissionThreshold, 0, 0, 1);
			}
			ENDCG
		}
	}
}
