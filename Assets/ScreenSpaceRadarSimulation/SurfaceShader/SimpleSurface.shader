Shader "RadarSimulation/SimpleSurface" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_NormalMap("Normal", 2D) = "bump" {}
		//[Enum(NoRefection,0,Plastic,0.25,Wood,0.5,Metal,1)] _Glossiness ("Surface", Range(0,1)) = 0.0
		_MetallicTex("MetallicTex", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard addshadow

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _MetallicTex;
		sampler2D _NormalMap;
		struct Input {
			float2 uv_MainTex;
		};

		half _Glossiness;
		
		fixed4 _Color;

		

		void surf (Input IN, inout SurfaceOutputStandard o) {
			
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			
			fixed4 m = tex2D(_MetallicTex, IN.uv_MainTex);

			o.Metallic = m.rgb;
			o.Smoothness = m.a;

			o.Normal = UnpackNormal(tex2D(_NormalMap, IN.uv_MainTex));

			//o.Alpha = c.a;
		}
		ENDCG
	}
	
}
