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
			uniform sampler2D _CameraDepthTexture;
			
			//比较场景深度图和需要描边的深度图，裁剪需要描边的深度图被场景遮挡的部分
			//裁剪后的结果，保存在r通道，原始的需要描边的深度图保存在g通道(通过将原始深度值置为0，避免距离相机较远的时候，深度值和1相近导致识别不出边缘)
			fixed4 frag (v2f_img i) : SV_Target
			{
				float outlineDepth = tex2D(_MainTex, i.uv);
				float sceneDepth = tex2D(_CameraDepthTexture, i.uv);
		 
				if (outlineDepth < 1)
				{
					//if (outlineDepth - sceneDepth > 0.00045)
                    if (outlineDepth > sceneDepth)
					{
						return fixed4(1, 0, 0, 0.5);
					}
					else
					{
						return fixed4(0, 0, 0, 1);
					}
				}
				return fixed4(outlineDepth, outlineDepth, 0 ,1);
			}
			ENDCG
		}
	}
}
