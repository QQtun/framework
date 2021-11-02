// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/Highlighted/Distortion"
{
	Properties
	{
		[HideInInspector] _NoiseTex("Noise Texture",2D) = "white" {}
        [HideInInspector] _MaskTex("Mask Texture", 2D) = "white" {}
        [HideInInspector] _HeatForce ("Heat Force",range(0,100.0)) = 1
	}
	
	SubShader
	{
		ColorMask GB
		Pass
		{
        	Cull Off
        	ZWrite Off
        	Lighting Off 
		    Fog { Mode Off }
        
			CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _NoiseTex;
            float4 _NoiseTex_ST;
            float  _HeatForce;
            sampler2D _MaskTex;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 uvgrab : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);		
                o.uv = TRANSFORM_TEX(v.uv,_NoiseTex);
                o.uvgrab = ComputeGrabScreenPos(o.vertex);
                return o;
            }


            float4 frag (v2f i) : SV_Target
            {
                half4 offestcol1 = tex2D(_NoiseTex,i.uv);
                half4 offestcol2 = tex2D(_NoiseTex,i.uv);
                half4 maskCol = tex2D(_MaskTex, i.uv);
                half heatFocre = _HeatForce * maskCol.r;
                i.uvgrab.x += ((offestcol1.r + offestcol2.r) - 1) * heatFocre;
                i.uvgrab.y += ((offestcol1.g + offestcol2.g) - 1) * heatFocre;

	            float2 reuv = i.uvgrab.xy/i.uvgrab.w;

                return float4(0, reuv, 1);
            }
			ENDCG
		}
	}
	
	Fallback Off
}