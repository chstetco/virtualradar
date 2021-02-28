Shader "RadarSimulation/VertexColorSurface" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_NormalMap("Normal", 2D) = "bump" {}
		_MetallicTex("MetallicTex", 2D) = "white" {}

		_Color2("Color2", Color) = (1,1,1,1)
		_MainTex2("Albedo2 (RGB)", 2D) = "white" {}
		_NormalMap2("Normal2", 2D) = "bump" {}
		_MetallicTex2("MetallicTex2", 2D) = "white" {}

		
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

		sampler2D _MainTex2;
		sampler2D _MetallicTex2;
		sampler2D _NormalMap2;

		

		struct Input {
			float2 uv_MainTex;
			float2 uv_MainTex2;
			float4 color : COLOR;
		};



		fixed4 _Color;
		fixed4 _Color2;



		void surf(Input IN, inout SurfaceOutputStandard o) {

			

			fixed4 c1 = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			fixed4 c2 = tex2D(_MainTex2, IN.uv_MainTex2) * _Color2;

			c1 = lerp(c1, c2, IN.color.r);

			o.Albedo = c1.rgb;

			fixed4 m1 = tex2D(_MetallicTex, IN.uv_MainTex);
			fixed4 m2 = tex2D(_MetallicTex2, IN.uv_MainTex2);

			m1 = lerp(m1,m2, IN.color.r);

			o.Metallic = m1.rgb;
			o.Smoothness = m1.a;

			fixed4 n1 = tex2D(_NormalMap, IN.uv_MainTex);
			fixed4 n2 = tex2D(_NormalMap2, IN.uv_MainTex2);

			n1 = lerp(n1,n2, IN.color.r);
			o.Normal = UnpackNormal(n1);

			//o.Alpha = c.a;
		}
		ENDCG
	}

}
