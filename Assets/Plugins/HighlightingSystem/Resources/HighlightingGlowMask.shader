// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/Highlighted/GlowMask"
{
	Properties
	{
		[HideInInspector] _MainTex("Main Tex", 2D) = "white" {}
		[HideInInspector] _MainTexUvSpeed("MainUVSpeed", Vector) = (0,0,0,0) 
		[HideInInspector] _BaseAlphaTex ("BaseAlpha", 2D) = "white" {}
		[HideInInspector] _BaseMoveSpeed("BaseMoveSpeed", Vector) = (0,0,0)
		[HideInInspector] _MaskMapTex ("MaskMap", 2D) = "white" {}
        [HideInInspector] _MaskUVSpeed("MaskUVSpeed", Vector) = (0,0,0) 
	    [HideInInspector] _Color ("Color", Color) = (0,0,0,0)		
        [HideInInspector] _ConstantColor("ConstantColor", Color) = (0,0,0,0)
        [HideInInspector] _GlowScale("GlowScale", float) = 0
        [HideInInspector] _Param("Param", Range(0, 1)) = 0.5
		[HideInInspector] _Cutoff ("", Float) = 0.5		
		[HideInInspector] _Cull ("", Int) = 2	
	}
	
	SubShader
	{
		ColorMask R
		Pass
		{
			Lighting Off
			Fog { Mode Off }
			Cull [_Cull]
			Blend One Zero
			ZWrite Off
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
			half4 _MainTexUvSpeed;
            sampler2D _BaseAlphaTex;
            float4 _BaseAlphaTex_ST;
			half3 _BaseMoveSpeed;
            sampler2D _MaskMapTex;
            float4 _MaskMapTex_ST;
            float3 _MaskUVSpeed;
            float _GlowScale;
            float _Param;
			float4 _Color;
            float4 _ConstantColor;
			float _Cutoff;

			struct appdata 
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};
			struct v2f 
			{
				float4 pos : POSITION;
				half2 uv : TEXCOORD0;	
                half2 mask_uv : TEXCOORD1;
                half2 base_uv : TEXCOORD2;
			};

			v2f vert(appdata v) 
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.mask_uv = TRANSFORM_TEX(v.texcoord, _MaskMapTex);
                o.base_uv = TRANSFORM_TEX(v.texcoord, _BaseAlphaTex);
				return o;
			}		
			half4 frag(v2f i) :COLOR 
			{
                // baseAlpha贴图颜色
				half2 base_uv = i.base_uv;
				base_uv.x += _Time.y * _BaseMoveSpeed.x;
				base_uv.y += _Time.y * _BaseMoveSpeed.y;
                half4 baseAlphaCol = tex2D(_BaseAlphaTex, base_uv);
                // 读取遮罩贴图颜色
                half2 maskUV = i.mask_uv - _MaskUVSpeed.xy * _Time.y;
                half4 maskCol = tex2D(_MaskMapTex, maskUV);
                fixed4 col = _Color * _GlowScale;				       
                float oneSubmaskColor = 1 - maskCol.g;
                float f = step( oneSubmaskColor, _Param);
                // Main贴图的颜色
				half2 mainTexUV = i.uv;
				mainTexUV.x += _MainTexUvSpeed.x * _Time.y;
				mainTexUV.y += _MainTexUvSpeed.y * _Time.y;
                half4 mainCol = tex2D(_MainTex, mainTexUV);
                half4 finalCol = (mainCol * baseAlphaCol + half4(col.rgb, 0)*2) * _ConstantColor * f;
				clip(finalCol.a - _Cutoff);

				return half4(finalCol.r+finalCol.g+finalCol.b > 0 ? 1 : 0 , 0 , 0 , 1);
			}	
			ENDCG
		}
	}
	
	Fallback Off
}