Shader "Custom/CustomTransparentDepthNormal"
{
    
	Properties
	{
		//_Absorption("_Absorption", Range(0,1)) = 0
		_RoughnessTex("RoughnessTex", 2D) = "white" {}
	}
		SubShader
	{
		 Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }//transparent 		
	    
		Pass//depth pass
		{

		ZTest Always //depth testing
		Blend DstAlpha SrcAlpha //Multiplicative transparent blend (alpha)

			CGPROGRAM
			#pragma vertex vert//vertex shader 
			#pragma fragment frag//fragment shader
			#include "UnityCG.cginc"

		// Properties
		sampler2D _HoldDepthTexture;
		struct vertexInput						//vertex shader struct input
		{
			float4 vertex : POSITION;			// mesh vertex
			float3 normal : NORMAL;				// mesh normal
		};

		struct vertexOutput						// struct for pixel shader
		{
			float4 pos : SV_POSITION;			// The position of the vertex after being transformed into projection space
			float linearDepth : TEXCOORD1;		// linear depth
			float4 screenPos : TEXCOORD2;		// UV coordinates


		};

		vertexOutput vert(vertexInput input)	// vertex shader
		{
			vertexOutput output;
			output.pos = UnityObjectToClipPos(input.vertex);	// Transforms a point from object space to the camer’s clip space					
			output.screenPos = ComputeScreenPos(output.pos);	// Computes texture coordinate for doing a screenspace-mapped texture sample		
			output.linearDepth = -(UnityObjectToViewPos(input.vertex).z * _ProjectionParams.w);	// calculate linear depth
			
			return output;
		}

		float4 frag(vertexOutput input) : COLOR //fragment shader
		{
			float4 c = float4(0, 0, 0, 1);
			
			float2 uv = input.screenPos.xy / input.screenPos.w; // normalized screen-space pos
			float camDepth = SAMPLE_DEPTH_TEXTURE(_HoldDepthTexture, uv);	// get depth texture data
			camDepth = Linear01Depth(camDepth); // converts z buffer value to depth value from 0..1


			float diff = saturate(input.linearDepth - camDepth);
			if (diff < 0.001) {	// check if transparent objects are infront or behind solid objects
				
				c = input.linearDepth;
			}

			c.a = c.a +c.r + c.g;

			return c;
			
		}

		ENDCG
	}

		Pass //normal pass
		{

		//Blend SrcAlpha OneMinusSrcAlpha
	    //Blend One One

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

		struct vertexInput
		{
			float4 vertex : POSITION;
			float3 normal : NORMAL;
		};

		struct vertexOutput
		{
			float4 pos : SV_POSITION;
			half3 worldNormal : TEXCOORD1;	// world space normal
			float4 screenPos : TEXCOORD2;


		};

		vertexOutput vert(vertexInput input)
		{
			vertexOutput output;
			output.pos = UnityObjectToClipPos(input.vertex);
			output.screenPos = ComputeScreenPos(output.pos);
			output.worldNormal = UnityObjectToWorldNormal(input.normal);// Transforms normal from object to world space
			return output;
		}

		float4 frag(vertexOutput input) : COLOR
		{
			float4 c = float4(0, 0, 0, 1);
			// normal is a 3D vector with xyz components; in -1..1
			// range. To display it as color, bring the range into 0..1
			// and put into red, green, blue components
			c.rgb = (input.worldNormal + 1) * 0.5;
			return c;

		}

		ENDCG
	}

			Pass // metal and smoothness pass Front
		{

		//ZTest Always //depth testing
		//Blend DstAlpha SrcAlpha //Multiplicative transparent blend (alpha)
		Cull Front


			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"


		float _ConductiveAdjustment;

		struct vertexInput
		{
			float4 vertex : POSITION;		
			float3 normal : NORMAL;
		};

		struct vertexOutput
		{
			float4 pos : SV_POSITION;
			float linearDepth : TEXCOORD1; // linear depth
			float4 screenPos : TEXCOORD2;


		};

		vertexOutput vert(vertexInput input)
		{
			vertexOutput output;
			output.pos = UnityObjectToClipPos(input.vertex); // Transforms a point from object space to the camer’s clip space					
			output.screenPos = ComputeScreenPos(output.pos); // Computes texture coordinate for doing a screenspace-mapped texture sample		
			output.linearDepth = -(UnityObjectToViewPos(input.vertex).z * _ProjectionParams.w); // calculate linear depth
			
			return output;

		}

		float4 frag(vertexOutput input) : COLOR
		{
			float4 c = float4(0, 0, 0, 1);

			
			//c = (1.0f-(1.0f/input.linearDepth)) *_ConductiveAdjustment;
			c.rgb = input.linearDepth;
			c.a = _ConductiveAdjustment;

			return c;

		}

		ENDCG
		}

				Pass //velocity pass
		{

			//Blend SrcAlpha OneMinusSrcAlpha
			//Blend One One

				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"

				// Properties
				//sampler2D_float _CameraDepthTexture;
				sampler2D _HoldDepthTexture;

				float _Velocity;
				struct vertexInput
				{
					float4 vertex : POSITION;
					float3 normal : NORMAL;
				};

				struct vertexOutput
				{
					float4 pos : SV_POSITION;
					float linearDepth : TEXCOORD1;
					float4 screenPos : TEXCOORD2;


				};

				vertexOutput vert(vertexInput input)
				{
					vertexOutput output;
					output.pos = UnityObjectToClipPos(input.vertex);
					output.screenPos = ComputeScreenPos(output.pos);
					output.linearDepth = -(UnityObjectToViewPos(input.vertex).z * _ProjectionParams.w);

					return output;
				}

				float4 frag(vertexOutput input) : COLOR
				{
					float4 c = float4(0,0,0, 1);

					
					float2 uv = input.screenPos.xy / input.screenPos.w; // normalized screen-space pos
					float camDepth = SAMPLE_DEPTH_TEXTURE(_HoldDepthTexture , uv);
					camDepth = Linear01Depth(camDepth); // converts z buffer value to depth value from 0..1


					float diff = saturate(input.linearDepth - camDepth);
					if (diff < 0.001) {
						//c = float4(0, 1, 0, 1);
						
						if (_Velocity>=0.0f) {
							c = float4(abs(_Velocity)/10.0f, 1.0f, 0, 1);
							//first component of vector c is: value of velocity converted to color
							//second component of vector c is: used for the direction of the velocity
						}
						else {
							c = float4(abs(_Velocity)/10.0f, 0.0f, 0, 1);
						}
							
						
						
						
					}

					return c;
					
				}

				ENDCG
		}


					Pass //metal and smoothness pass Back
				{

					//ZTest Always //depth testing
					Blend DstAlpha SrcAlpha //Multiplicative transparent blend (alpha)
					//Cull Back


						CGPROGRAM
						#pragma vertex vert
						#pragma fragment frag
						#include "UnityCG.cginc"


					float _ConductiveAdjustment;//1 is full metal
					//float _TransparentObjectBlend;//blend between transparent objects

					struct vertexInput
					{
						float4 vertex : POSITION;
						float3 normal : NORMAL;
					};

					struct vertexOutput
					{
						float4 pos : SV_POSITION;
						float linearDepth : TEXCOORD1;//linear depth
						float4 screenPos : TEXCOORD2;


					};

					vertexOutput vert(vertexInput input)
					{
						vertexOutput output;
						output.pos = UnityObjectToClipPos(input.vertex);//Transforms a point from object space to the camer’s clip space					
						output.screenPos = ComputeScreenPos(output.pos);//Computes texture coordinate for doing a screenspace-mapped texture sample		
						output.linearDepth = -(UnityObjectToViewPos(input.vertex).z * _ProjectionParams.w);//calculate linear depth
						//UnityObjectToViewPos...Transforms a point from object space to view space

						return output;

					}

					float4 frag(vertexOutput input) : COLOR
					{
						float4 c = float4(0, 0, 0, 1);


						//c = (1.0f-(1.0f/input.linearDepth)) *_ConductiveAdjustment;
						c.rgb = input.linearDepth;
						//fixed4 rough = tex2D(_RoughnessTex);
						c.a = _ConductiveAdjustment;

						return c;

					}

					ENDCG
				}
	}
}