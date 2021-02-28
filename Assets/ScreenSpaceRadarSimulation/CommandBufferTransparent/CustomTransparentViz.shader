Shader "Hidden/CustomTransparentViz"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Lerp("_Lerp", Range(0,1)) = 0
		_Lerp2("_Lerp2", Range(0,1)) = 0
		_Lerp2("_Lerp3", Range(0,1)) = 0
	}
		SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			sampler2D _MainTex;
			sampler2D _GlasDepthMap;
			sampler2D _GlasNormalMap;
			sampler2D _GlasMetalMapFront;
			sampler2D _GlasMetalMapBack;
			sampler2D _VMaskMap;
			
			float _Lerp;
			float _Lerp2;
			float _Lerp3;
			fixed4 frag(v2f i) : SV_Target
			{
				//fixed4 col = tex2D(_MainTex, i.uv);
			fixed4 depth = tex2D(_GlasDepthMap, i.uv);
			fixed4 normal = tex2D(_GlasNormalMap, i.uv);
			fixed4 metalfront = tex2D(_GlasMetalMapFront, i.uv);
			fixed4 metalback = tex2D(_GlasMetalMapBack, i.uv);
			fixed4 metal = metalfront;
			
			 metal = metalfront - metalback;
			
			fixed4 vmask = tex2D(_VMaskMap, i.uv);
			return lerp(lerp(lerp(depth, normal, _Lerp), metal.r*metalfront.a, _Lerp2), vmask, _Lerp3);
		}
		ENDCG
	}
	}
}
