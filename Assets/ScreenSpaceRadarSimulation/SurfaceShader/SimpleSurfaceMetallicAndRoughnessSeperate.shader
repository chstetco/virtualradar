Shader "RadarSimulation/SimpleSurfaceMetallicAndRoughnessSeperate" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_NormalMap("Normal", 2D) = "bump" {}
		
		_MetallicTex("MetallicTex", 2D) = "black" {}
		_RoughnessTex("RoughnessTex", 2D) = "white" {}
		 _ConductiveAdjustment("ConductiveAdjustment", Range(0,1)) = 0.1
			
	}
		SubShader{
			Tags { "RenderType" = "Opaque" }
			LOD 200

			CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard addshadow

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _MetallicTex;
		sampler2D _NormalMap;
		sampler2D _RoughnessTex;
		struct Input {
			float2 uv_MainTex;
		};

		half _Glossiness;
		half _ConductiveAdjustment;
		fixed4 _Color;



		void surf(Input IN, inout SurfaceOutputStandard o) {

			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;

			fixed4 metal = tex2D(_MetallicTex, IN.uv_MainTex);
			fixed4 rough = tex2D(_RoughnessTex, IN.uv_MainTex);
			o.Metallic = _ConductiveAdjustment * metal.r;

			o.Smoothness = _ConductiveAdjustment * abs(1 - rough.r);
			//o.Smoothness = abs(1 - rough.r);
			o.Normal = UnpackNormal(tex2D(_NormalMap, IN.uv_MainTex));

			//o.Alpha = c.a;
		}
		ENDCG
	}

}

