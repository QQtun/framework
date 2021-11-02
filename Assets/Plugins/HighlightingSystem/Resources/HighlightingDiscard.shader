Shader "Hidden/Highlighted/Discard" {
	Properties {

	}
	SubShader {
		Pass 
		{

          CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			#include "UnityCG.cginc"


          fixed4 frag() : COLOR {
              discard;
			  return fixed4(0, 0, 0, 0);
          }
          ENDCG

		}
	}
}
