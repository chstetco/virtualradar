// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/depthImageShader"
{
    Properties
    {
        _MainTex("Base (RGB)", 2D) = "white" {}
        _DepthLevel("Depth Level", Range(1, 3)) = 1
        _PermittivityMask("PermittivityMask", 2D) = "white" {}
    }
        SubShader
        {
            // No culling or depth
            //Cull Off ZWrite Off ZTest Always

            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag

                #include "UnityCG.cginc"

                uniform sampler2D _MainTex; 
                uniform sampler2D _CameraDepthTexture;
                uniform fixed _DepthLevel;
                uniform half4 _MainTex_TexelSize;

               // sampler2D _CameraGBufferTexture1;

                struct input
                {
                    float4 pos : POSITION;
                    half2 uv : TEXCOORD0;
                };

                struct output
                {
                    float4 pos : SV_POSITION;
                    half2 uv : TEXCOORD0;
                };

                output vert(input i)
                {
                    output o; 
                    o.pos = UnityObjectToClipPos(i.pos);
                    o.uv = MultiplyUV(UNITY_MATRIX_TEXTURE0, i.uv);

                    #if UNITY_UV_STARTS_AT_TOP
                    if (_MainTex_TexelSize.y < 0)
                        o.uv.y = 1 - o.uv.y;
                    #endif

                   // float permittivity = tex2D(_PermittivityMask, i.screenuv).r;

                    return o;
                }

                fixed4 frag(output o) : COLOR
                {
                    float depth = UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture, o.uv));
                    depth = pow(Linear01Depth(depth), _DepthLevel);
                    return depth;
                }
            ENDCG
        }
    }
}
