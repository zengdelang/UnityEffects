Shader "DepthCull"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}

	SubShader
	{
		Tags { "RenderType" = "Opaque" }

		Pass
		{
			ZWrite On
			ZTest Always

			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag		

			#include "UnityCG.cginc"

			uniform sampler2D _MainTex;
			uniform sampler2D _SceneDepthTexture;
 
			fixed4 frag (v2f_img i) : SV_Target
			{
				float outlineDepth = tex2D(_MainTex, i.uv);
				float sceneDepth = tex2D(_SceneDepthTexture, i.uv);
		 
				if (outlineDepth < 1)
				{
					if (outlineDepth - sceneDepth > 0.00045)
					{
						return fixed4(1, 0, 0, 1);
					}
				}
				return tex2D(_MainTex, i.uv);
			}
			ENDCG
		}
	}
}
