// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/Highlighted/Dissolve"
{
	Properties
	{
		[HideInInspector] _MainTex ("", 2D) = "" {}
		[HideInInspector]_TinColor ("", Color) = (1,1,1,1)
		[HideInInspector]_DissolorTex ("", 2D) = "white" {}
		[HideInInspector]_DissolorTexUVMoveSpeed ("",Vector) = (0,0,0,0)
		[HideInInspector]_DissolorUvTex ("", 2D) = "white" {}
		[HideInInspector]_DissolorUvSpeed ("",Vector ) = (0.5,0.5,0,0)		
		[HideInInspector]_RAmount ("", Range (0, 1)) = 0.5		
		[HideInInspector]_DissolorWith("", float) = 0.1
		[HideInInspector]_DissColor ("", Color) = (1,1,1,1)
		[HideInInspector]_Emission ("", Range(0,8)) = 1
		[HideInInspector]_DissColorIlluminate ("", Range (0, 4)) = 1
		[HideInInspector] _Cull ("", Int) = 2	
		[HideInInspector] _MaskTex("", 2D) = "white" {}
		[HideInInspector] _MaskThreshold("", Range(0, 1.1)) = 1.1
	}
	
	SubShader
	{
		ColorMask R
		Pass
		{
			Lighting Off
			Fog { Mode Off }
			ZWrite Off
			Cull [_Cull]
			Blend One Zero
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"

			uniform sampler2D _MainTex;
			uniform float4 _MainTex_ST;
			uniform fixed _Cutoff;
			sampler2D _DissolorTex;
			float4 _DissolorTex_ST;
			sampler2D _DissolorUvTex;
			float4 _DissolorUvTex_ST;
		    half4 _DissolorUvSpeed;
			half4 _DissolorTexUVMoveSpeed;
			
			half _RAmount;
			
			half _DissolorWith;
			fixed4 _DissColor;
			fixed4 _TinColor;
			half _Emission;
			half _DissColorIlluminate;
			sampler2D _MaskTex;
			half _MaskThreshold;
						
			struct appdata_vert_tex
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				fixed4 color : COLOR;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 texcoord : TEXCOORD0;
				fixed alpha : TEXCOORD1;
				half2 texcoord1 : TEXCOORD2;
				half2 texcoord2 : TEXCOORD3;
				fixed4 color : COLOR;
			};

			v2f vert(appdata_vert_tex v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.texcoord1 = TRANSFORM_TEX(v.texcoord, _DissolorTex);
				o.texcoord2 = TRANSFORM_TEX(v.texcoord, _DissolorUvTex);
				o.alpha = v.color.a;
				o.color = v.color;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				half4 maskCol = tex2D(_MaskTex,i.texcoord);
				if(maskCol.r >= _MaskThreshold)
					discard;

				half4 mainCol = tex2D(_MainTex, i.texcoord);
				//clip(mainCol.a - _Cutoff);
				float2 dissolorTexUV = i.texcoord1;
				dissolorTexUV.x += _DissolorTexUVMoveSpeed.x * _Time.y;
				dissolorTexUV.y += _DissolorTexUVMoveSpeed.y * _Time.y;
				half4 DissolorTexCol = tex2D(_DissolorTex,dissolorTexUV);

				//return mainCol * DissolorTexCol;
				
				float2 uv = i.texcoord2;
				uv.x += _DissolorUvSpeed.x * _Time.y;
				uv.y += _DissolorUvSpeed.y * _Time.y;
				fixed4 UvCol = tex2D(_DissolorUvTex, uv);
				
				half clipVauleR = DissolorTexCol.r - _RAmount;
				half Clip = 0;
				if(clipVauleR <= 0)
				{
					if(clipVauleR > -_DissolorWith)
					{
						//插值颜色过度
							float t = clipVauleR / -_DissolorWith;
							t = saturate(t);  
							mainCol = lerp(mainCol, _DissColor * _DissColorIlluminate * UvCol, t);	
							Clip = _RAmount == 1 ? -1 : 0;
					}
					else
					{
						Clip = -1;
					}
				
				}
				clip(Clip);
				mainCol.rgb = mainCol.rgb * _Emission * _TinColor.rgb * i.color.rgb;
				return half4(mainCol.r+mainCol.g+mainCol.b > 0 ? 1 : 0 , 0 , 0 , 1);
			}
			ENDCG
		}
	}
	
	Fallback Off
}