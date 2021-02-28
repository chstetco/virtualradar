Shader "Hidden/CopyDepthShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
	

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 5.0
			#include "UnityCG.cginc"


			struct v2f // struc for fragment shader
			{
				float4 pos : SV_POSITION;//The position of the vertex after being transformed into projection space
				float4 screenuv : TEXCOORD1;//UV coordinates
			};

			v2f vert (appdata_base v) //vertex shader
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);//object space to the camera’s clip space
				o.screenuv = ComputeScreenPos(o.pos);//Computes texture coordinate for doing a screenspace-mapped texture sample. Input is clip space position.
				return o;
			}
			
			sampler2D _MainTex;
			sampler2D _CameraDepthTexture;//depth texture
			
			fixed4 frag (v2f i) : SV_Target //fragment shader
			{
				

				float4 camDepth = tex2D(_CameraDepthTexture, i.screenuv);//get color values from the texture

			

				return camDepth;
			}
			ENDCG
		}
	}
}
