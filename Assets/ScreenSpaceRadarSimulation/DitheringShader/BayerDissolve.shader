// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

Shader "RadarSimulation/BayerDissolve" {
	Properties{

		_Color("Color", Color) = (1,1,1,1)

		_MainTex("Albedo (RGB)", 2D) = "white" {}

		_BayerTex("Filter", 2D) = "gray" {}

		_Glossiness("Smoothness", Range(0,1)) = 0.5

		_Metallic("Metallic", Range(0,1)) = 0.0

		_XScrollSpeed("X Scroll Speed", Float) = 0
		_YScrollSpeed("Y Scroll Speed", Float) = 0
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

			sampler2D _BayerTex;

			float4 _BayerTex_TexelSize;

			float _XScrollSpeed;
			float _YScrollSpeed;

			struct Input {

				float2 uv_MainTex;

				float4 screenPos;

			};



			half _Glossiness;

			half _Metallic;

			fixed4 _Color;



			// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.

			// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.

			// #pragma instancing_options assumeuniformscaling

			UNITY_INSTANCING_BUFFER_START(Props)

				// put more per-instance properties here

			UNITY_INSTANCING_BUFFER_END(Props)



			void surf(Input IN, inout SurfaceOutputStandard o) {


				

				// Albedo comes from a texture tinted by color

				fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;

				o.Albedo = c.rgb;

				// Metallic and smoothness come from slider variables

				o.Metallic = _Metallic;

				o.Smoothness = _Glossiness;

				o.Alpha = c.a;



				//Get the screen coordinates in pixels


				fixed xScrollValue = _XScrollSpeed * _Time.x;
				fixed yScrollValue = _YScrollSpeed * _Time.x;

				float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
				screenUV += fixed2(xScrollValue, yScrollValue);
				
				screenUV *= float2(_ScreenParams.x * _BayerTex_TexelSize.x,_ScreenParams.y * _BayerTex_TexelSize.y);

				//Compare alpha to threshold

				clip(c.a - tex2D(_BayerTex, screenUV).r);

			}

			ENDCG

		}

			FallBack "Diffuse"

}
