Shader "Hidden/ScreenSpaceRadarShader"
{
	// Properties section (see in editor)
	Properties{
	[HideInInspector] _MainTex("Base (RGB)", 2D) = "white" {}
	_RadiationPatternMask("RadiationPatternMask", 2D) = "white" {}
	_RadiationPatternMask("RoughnessMap", 2D) = "white" {}
	_Blend("Blend", Range(0, 1)) = 0
	_MaxDistance("MaxDistance", Range(0, 1)) = 0
	_MaxVelocity("_MaxVelocity", Range(0, 100)) = 1
	_LerpSpecular("LerpSpecular", Range(0, 1)) = 0
	_LerpNormal("LerpNormal", Range(0, 1)) = 0	
	_ViewDir("_ViewDir", Vector) = (0,0,0,0)		
	_LowerChirpFrequency("_LowerChirpFrequency", float) = 1
	_ChirpRate("_ChirpRate", float) = 1
    _fov("_fov", float) = 100
	_BandwidthOfTheChirp("_BandwidthOfTheChirp", float) = 1

	[HideInInspector]
	[Enum(Unity_and_TCP,1,Unity_to_ROS,2)] _Enum("Enum", Int) = 1
	}
		SubShader
	{
		Tags { "RenderType" = "Opaque" }

		Pass
		{
			CGPROGRAM
			#pragma vertex vert					// vertex shader 
			#pragma fragment frag				// fragment shader
			#pragma target 5.0					// need target 5.0 for compute buffer
			#include "UnityCG.cginc"

			struct v2f							// struct for pixel shader
			{
				float4 pos : SV_POSITION;		// The position of the vertex after being transformed into projection space
				float4 screenuv : TEXCOORD1;	// UV coordinates
				
			};

			v2f vert(appdata_base v)			// vertex shader part (use appdata_base struct)
			{
				v2f o;												// init
				o.pos = UnityObjectToClipPos(v.vertex);				// Transforms a point from object space to the camer’s clip space
				o.screenuv = ComputeScreenPos(o.pos);				// Computes texture coordinate for doing a screenspace-mapped texture sample		
				return o;
			}

			sampler2D _CameraDepthTexture;					// depth texture from deferred rendering path
			sampler2D _BTex;								// last frame depth texture
			uniform sampler2D _MainTex;						// camera view texture
			uniform sampler2D _RadiationPatternMask;		// Radiation Pattern Weighting Mask
			uniform sampler2D _RoughnessMap; 
			uniform float _Blend;							// blend between camera view and image effect
			uniform float _MaxDistance;						// max. range possible given radar setup
			uniform float _MaxVelocity;						// max. velocity given radar setup
			uniform half _LerpSpecular;						// blend specular map control
			uniform half _LerpNormal;						// blend normal map control
			
			sampler2D _CameraGBufferTexture1;				// from deferred rendering path - Smoothness on A channel, Metallic on R channel
			sampler2D _CameraGBufferTexture2;				// from deferred rendering path (Normal RGB)

			float4 _ViewDir;								// sensor view direction
			int _width;										// screenwidth
			int _height;								    // screenheight
			int _chirpsNumber;								// number of chirps
			int _samplesNumber;								// number of samples
			float _ChirpRate;								// chirp rate
			int _nrAntennas;
			float _fov;										// field-of-view of radar sensor
			float _centerFrequency;							// chirp center frequency
			float _LowerChirpFrequency;						// chirp start frequency
			float _BandwidthOfTheChirp;						// chirp bandwidth
			
			RWStructuredBuffer<float2> _gpuBuffer1 : register(u1);	// GPU memory buffer 1 (used for communication between GPU and CPU) --> antenna 1 and 2
			RWStructuredBuffer<float2> _gpuBuffer2 : register(u2);	// GPU memory buffer 2 (used for communication between GPU and CPU) --> antenna 3 and 4
			int _Enum;												// Unity_and_TCP=1, Unity_to_ROS=2

			// from command buffer
			sampler2D _GlasDepthMap;								// transparent depth map
			sampler2D _GlasNormalMap;								// transparent normal map
			sampler2D _GlasMetalMapFront;							// transparent metal/smoothness front map
			sampler2D _GlasMetalMapBack;							// transparent metal/smoothness back map
			sampler2D _VMaskMap;									// transparent velocity map

			
			fixed4 frag(v2f i) : SV_Target // fragment shader
			{
				float4 c = tex2D(_MainTex, i.screenuv);				// get color values from the camera view texture
				
				float radiationpatternweighting = tex2D(_RadiationPatternMask, i.screenuv).r; // get weighting value from texture
				float roughness = tex2D(_RoughnessMap, i.screenuv).r; // get weighting value from texture
				
				// transparent objects velocity
				float velocitymaptrans;
				float2 velocitymaptranssign = tex2D(_VMaskMap, i.screenuv).rg*10.0f; // get velocity values from the texture (r channel...velocity value, g channel...direction, *10.0f...convert 0.0f-1.0f value back velocity)

				if (velocitymaptranssign.y >= 0.5f) { // velocitymaptranssign.y is same as velocitymaptranssign.g (direction)
					velocitymaptrans = velocitymaptranssign.r;
				}
				else {
					velocitymaptrans = -velocitymaptranssign.r;
				}

				// depth of solid and transparent objects
				float2 uv = i.screenuv.xy / i.screenuv.w;										// calculate uv for depth (from each pixel)
				float depth = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv));		// give high precision value from depth texture (0 to 1) from current frame (solid objects depth map)														
				float depthprev = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_BTex, uv));				// give high precision value from depth texture (0 to 1) from last frame (solid objects depth map)	
				float depthtrans = tex2D(_GlasDepthMap, i.screenuv).r;							// linear depth transparent objects depth map				

				// normal vector of solide and transparent objects
				float4 gbuffer2 = tex2D(_CameraGBufferTexture2, i.screenuv);					// Normal RGB (solid)
				float3 gbuffer2trans = tex2D(_GlasNormalMap, i.screenuv);						// Normal RGB (transparent)
				
				float3 normalvector = float3((gbuffer2.r * 2) - 1, (gbuffer2.g * 2) - 1, (gbuffer2.b * 2) - 1);// convert normal color texture(blue channel) to normal vector (solid)
				float3 normalvectortrans = float3((gbuffer2trans.r * 2) - 1, (gbuffer2trans.g * 2) - 1, (gbuffer2trans.b * 2) - 1);// convert normal color texture(blue channel) to normal vector (transparent)

				// metallic and smoothness
				float4 gbuffer1 = tex2D(_CameraGBufferTexture1, i.screenuv); // Metallic R(red channel), Smoothness A(alpha channel)	(solid)
				float4 gbuffer1transfront = tex2D(_GlasMetalMapFront, i.screenuv); // transparent
				float4 gbuffer1transback = tex2D(_GlasMetalMapBack, i.screenuv); // transparent
				float4 gbuffer1trans = gbuffer1transfront - gbuffer1transback;
				
				float pi = 3.14159;
				float spacing; 

				int x = int(i.screenuv.x * _width); // compute buffer width
				int y = int(i.screenuv.y * _height); // compute buffer height
							
				float azimuth_angle; 
				float elevation_angle; 
				float f;
				float phase_shift_rx2;
				float phase_shift_rx3;
				float phase_shift_rx4;
				float c0 = 299792458.0f; // speed of light
				float lambda; 

				_centerFrequency = _LowerChirpFrequency + (_BandwidthOfTheChirp * 0.5f);
				lambda = c0 / _centerFrequency;
				spacing = lambda / 2.0f;

				f = (_width * 0.5f) / tan((_fov * pi / 180.0f) * 0.5f);
					
				if (depth < 1)
				{

					azimuth_angle = atan((x - 0.5f * _width) / f);
					elevation_angle = atan((y - 0.5f * _height) / f);
					phase_shift_rx2 = (2 * pi * spacing * sin(azimuth_angle)) / lambda;
					phase_shift_rx3 = (4 * pi * spacing * sin(azimuth_angle)) / lambda;
					phase_shift_rx4 = (6 * pi * spacing * sin(azimuth_angle)) / lambda;
				}
				else
				{
					phase_shift_rx2 = 0.0f;
					phase_shift_rx3 = 0.0f;
					phase_shift_rx4 = 0.0f;
				}
	
				float distance = depth * _MaxDistance;// / cos(azimuth_angle); // calculate distance (solid)
				float distancetrans = depthtrans * _MaxDistance; // calculate distance (transparent)
				
				if (distance >= _MaxDistance)
				{
					distance = 0.0f;
				}
				if (distancetrans >= _MaxDistance)
				{
					distancetrans = 0.0f;
				}
				
				float delta_t = unity_DeltaTime.x; // frame to frame time
				float velocity = (((depth - depthprev)  * _MaxDistance)) / delta_t; // velocity calculation (solid)	
				float velocitytrans=velocitymaptrans; // velocity transparent

				/*if (abs(velocity)<=abs(_MaxVelocity)) 
				{
					velocity = -velocity;
				}
				else 
				{
					velocity = 0.0f;
				}
				*/

				if (abs(velocitytrans) <= abs(_MaxVelocity)) 
				{
					velocitytrans = velocitytrans;
				}
				else 
				{
					velocitytrans = 0.0f;
				}
				
				// Radar Simulation
				float diffuse;
				float diffuse_trans;
				float specular;
				float specular_trans;
				float3 sensor_direction = _ViewDir;

				specular_trans = max(0.0, dot(reflect(sensor_direction, normalvectortrans), sensor_direction));
				specular = max(0.0, dot(reflect(-sensor_direction, normalvector), sensor_direction)); 
				diffuse = dot(sensor_direction, normalvector);
				diffuse_trans = dot(sensor_direction, normalvectortrans);

				float Rij = gbuffer1.a * diffuse;// +(1 - gbuffer1.a) * specular;// *(gbuffer1.r * gbuffer1.g * gbuffer1.b * gbuffer1.a); // reflection coefficient (solid)	
				float Rijtrans = roughness * diffuse_trans;

				float distancefin = distance;
				float distancefintrans = distancetrans;
				float fs = 2000000.0f; // sample frequency
				
				for (uint chirps = 0; chirps < _chirpsNumber; chirps++) // chirp loop
				{
					for (uint i = 0; i < _samplesNumber; i++) // samples loop
					{
						float t = (float)(i + 1) / fs; // time vector	

						// Solid Object
						float tau = (2/c0) * (distancefin + (velocity * (_samplesNumber / fs)*chirps)); // time delay 

						float f1 = 2*pi*_LowerChirpFrequency * tau;
						float f2 = 2 * pi*_ChirpRate*tau*t;
						float f3 = -pi * _ChirpRate*tau*tau;
							
						float fn = f1 + f2 + f3;

						// Transparent Object
						float tautrans = (2 / c0) * (distancefintrans + (velocitytrans * (_samplesNumber / fs)*chirps));//time delay //				

						float f1trans = 2 * pi*_LowerChirpFrequency * tautrans;
						float f2trans = 2 * pi*_ChirpRate*tautrans*t;
						float f3trans = -pi * _ChirpRate*tautrans*tautrans;

						float fntrans = f1trans + f2trans + f3trans;
							
						// receiver contributions (solid and transparent)
						float solidrx1 = 0.0f;
						float solidrx2 = 0.0f;
						float solidrx3 = 0.0f;
						float solidrx4 = 0.0f;
						float transrx1 = 0.0f;
						float transrx2 = 0.0f;
						float transrx3 = 0.0f;
						float transrx4 = 0.0f;

						if (distancefin != 0) 
						{
							if (_nrAntennas == 1)
							{
								solidrx1 = radiationpatternweighting * (1 / distancefin) * Rij * cos(fn);
							}
							else if (_nrAntennas == 2)
							{
								solidrx1 = radiationpatternweighting * (1 / distancefin) * Rij * cos(fn);
								solidrx2 = radiationpatternweighting * (1 / distancefin) * Rij * cos(fn + phase_shift_rx2);
							}
							else if (_nrAntennas == 3)
							{
								solidrx1 = radiationpatternweighting * (1 / distancefin) * Rij * cos(fn);
								solidrx2 = radiationpatternweighting * (1 / distancefin) * Rij * cos(fn + phase_shift_rx2);
								solidrx3 = radiationpatternweighting * (1 / distancefin) * Rij * cos(fn + phase_shift_rx3);
							}
							else
							{
								solidrx1 = radiationpatternweighting * (1 / distancefin) * Rij * cos(fn);
								solidrx2 = radiationpatternweighting * (1 / distancefin) * Rij * cos(fn + phase_shift_rx2);
								solidrx3 = radiationpatternweighting * (1 / distancefin) * Rij * cos(fn + phase_shift_rx3);
								solidrx4 = radiationpatternweighting * (1 / distancefin) * Rij * cos(fn + phase_shift_rx4);
							}
						}
										
						if (distancefintrans != 0)
						{
							if (_nrAntennas == 1)
							{
								transrx1 = radiationpatternweighting * (1 / distancefintrans) * Rijtrans * cos(fntrans);
							}
							else if (_nrAntennas == 2)
							{
								transrx1 = radiationpatternweighting * (1 / distancefintrans) * Rijtrans * cos(fntrans);
								transrx2 = radiationpatternweighting * (1 / distancefintrans) * Rijtrans * cos(fntrans + phase_shift_rx2);
							}
							else if (_nrAntennas == 3)
							{
								transrx1 = radiationpatternweighting * (1 / distancefintrans) * Rijtrans * cos(fntrans);
								transrx2 = radiationpatternweighting * (1 / distancefintrans) * Rijtrans * cos(fntrans + phase_shift_rx2);
								transrx3 = radiationpatternweighting * (1 / distancefintrans) * Rijtrans * cos(fntrans + phase_shift_rx3);
							}
							else
							{
								transrx1 = radiationpatternweighting * (1 / distancefintrans) * Rijtrans * cos(fntrans);
								transrx2 = radiationpatternweighting * (1 / distancefintrans) * Rijtrans * cos(fntrans + phase_shift_rx2);
								transrx3 = radiationpatternweighting * (1 / distancefintrans) * Rijtrans * cos(fntrans + phase_shift_rx3);
								transrx4 = radiationpatternweighting * (1 / distancefintrans) * Rijtrans * cos(fntrans + phase_shift_rx4);
							}
						}
	
						if (_nrAntennas == 1)
						{
							_gpuBuffer1[(chirps * _width * _height * _samplesNumber) + (i * _width * _height) + (y * _width) + x].x = solidrx1 + transrx1;
							_gpuBuffer1[(chirps * _width * _height * _samplesNumber) + (i * _width * _height) + (y * _width) + x].y = 0.0f;
							_gpuBuffer2[(chirps * _width * _height * _samplesNumber) + (i * _width * _height) + (y * _width) + x].x = 0.0f;
							_gpuBuffer2[(chirps * _width * _height * _samplesNumber) + (i * _width * _height) + (y * _width) + x].y = 0.0f;
						}
						else if (_nrAntennas == 2)
						{
							_gpuBuffer1[(chirps * _width * _height * _samplesNumber) + (i * _width * _height) + (y * _width) + x].x = solidrx1 + transrx1;
							_gpuBuffer1[(chirps * _width * _height * _samplesNumber) + (i * _width * _height) + (y * _width) + x].y = solidrx2 + transrx2;
							_gpuBuffer2[(chirps * _width * _height * _samplesNumber) + (i * _width * _height) + (y * _width) + x].x = 0.0f;
							_gpuBuffer2[(chirps * _width * _height * _samplesNumber) + (i * _width * _height) + (y * _width) + x].y = 0.0f;
						}
						else if (_nrAntennas == 3)
						{
							_gpuBuffer1[(chirps * _width * _height * _samplesNumber) + (i * _width * _height) + (y * _width) + x].x = solidrx1 + transrx1;
							_gpuBuffer1[(chirps * _width * _height * _samplesNumber) + (i * _width * _height) + (y * _width) + x].y = solidrx2 + transrx2;
							_gpuBuffer2[(chirps * _width * _height * _samplesNumber) + (i * _width * _height) + (y * _width) + x].x = solidrx3 + transrx3;
							_gpuBuffer2[(chirps * _width * _height * _samplesNumber) + (i * _width * _height) + (y * _width) + x].y = 0.0f;
						}
						else
						{
							_gpuBuffer1[(chirps * _width * _height * _samplesNumber) + (i * _width * _height) + (y * _width) + x].x = solidrx1 + transrx1;
							_gpuBuffer1[(chirps * _width * _height * _samplesNumber) + (i * _width * _height) + (y * _width) + x].y = solidrx2 + transrx2;
							_gpuBuffer2[(chirps * _width * _height * _samplesNumber) + (i * _width * _height) + (y * _width) + x].x = solidrx3 + transrx3;
							_gpuBuffer2[(chirps * _width * _height * _samplesNumber) + (i * _width * _height) + (y * _width) + x].y = solidrx4 + transrx4;
						}
						

					}
						distancefin = distance + (velocity*chirps * (_samplesNumber / fs));
						distancefintrans = distancetrans + (velocitytrans*chirps * (_samplesNumber / fs));
				}
				

				// image effect
				float4 imageeffect;
				imageeffect = lerp(float4(depth, depth, depth, 1), gbuffer1, _LerpSpecular); // lerp between depth and metallic				
				imageeffect = lerp(imageeffect, gbuffer2, _LerpNormal); // lerp with normal texture			
				return lerp(c, imageeffect, 1.0f); // blend cameraview and effects
			}
			ENDCG
		}
	}
}

