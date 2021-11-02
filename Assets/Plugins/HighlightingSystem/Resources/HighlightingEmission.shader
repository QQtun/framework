// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/Highlighted/Emission" {
	Properties {
		[HideInInspector] _SelfIllumin ("Self Illumin", 2D) = "white" {}
		[HideInInspector] _EmissionThreshold ("EmissionThreshold", Range(0, 0.98)) = 0.98
	}
	SubShader {
		ColorMask R
		Pass 
		{
			Lighting Off
			Fog { Mode Off }
			//Blend One Zero

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;

			sampler2D _SelfIllumin;
			float4 _SelfIllumin_ST;
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
				o.uv = TRANSFORM_TEX (v.texcoord, _SelfIllumin);
				return o;
			}
			
			fixed4 frag(v2f i) : COLOR { 	
				fixed4 illC = tex2D(_SelfIllumin, i.uv);
				return fixed4( illC.r * _EmissionThreshold, 0, 0, 1);
			}
			ENDCG
		}
	}
}
