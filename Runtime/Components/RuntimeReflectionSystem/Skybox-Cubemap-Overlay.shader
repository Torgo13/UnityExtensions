// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)
Shader "Skybox/CubemapOverlay"
{
    Properties
    {
        [MainColor] _Tint("Tint Color", Color) = (1.0, 1.0, 1.0, 1.0)
        [Gamma] _Exposure("Exposure", Range(0, 8)) = 1.0
        _Rotation("Rotation", Range(0, 360)) = 0
        [MainTexture] [NoScaleOffset] _Tex("Cubemap (HDR)", Cube) = "grey" {}
        [NoScaleOffset] _TexB("CubemapB (HDR)", Cube) = "grey" {}
        _Blend("Blend", Range(0, 1)) = 0.0
        _MipLevel("Mip Level", Range(1, 16)) = 1.0
    }

    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" "RenderPipeline"="UniversalPipeline" }
        Cull Off ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #pragma multi_compile _ OVERLAY

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Macros.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURECUBE(_Tex);
            TEXTURECUBE(_TexB);
            SAMPLER(sampler_Tex);
            
            CBUFFER_START(UnityPerMaterial)
            half3 _Tint;
            half _Exposure;
            float _MipLevel;
            half _Rotation;
            half _Blend;
            CBUFFER_END

            float3 RotateAroundYInDegrees(float3 vertex, float degrees)
            {
                float alpha = degrees * PI / 180.0;
                float sin_a, cos_a;
                sincos(alpha, sin_a, cos_a);
                float2x2 m = float2x2(cos_a, -sin_a, sin_a, cos_a);
                return float3(mul(m, vertex.xz), vertex.y).xzy;
            }

            struct Attributes
            {
                float4 vertex : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 vertex : SV_POSITION;
                float3 texcoord : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes IN)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                float3 rotated = RotateAroundYInDegrees(IN.vertex.xyz, _Rotation);
                o.vertex = TransformObjectToHClip(rotated);
                o.texcoord = IN.vertex.xyz;
                return o;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half3 c = SAMPLE_TEXTURECUBE_LOD(_Tex, sampler_Tex, IN.texcoord, _MipLevel).rgb;
                half3 c_b = SAMPLE_TEXTURECUBE(_TexB, sampler_Tex, IN.texcoord).rgb;

#if OVERLAY
#else
                c = lerp(c, c_b, _Blend);
#endif // OVERLAY

                c *= _Tint;
                c *= _Exposure;

#if OVERLAY
                c += c_b * _Blend;
#endif // OVERLAY

                return half4(c, 1.0);
            }
            ENDHLSL
        }
    }

    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"

            samplerCUBE _Tex;
            samplerCUBE _TexB;
            half4 _Tex_HDR;
            half4 _TexB_HDR;
            half3 _Tint;
            half _Exposure;
            float _MipLevel;
            half _Rotation;
            half _Blend;

            float3 RotateAroundYInDegrees(float3 vertex, float degrees)
            {
                float alpha = degrees * UNITY_PI / 180.0;
                float sin_a, cos_a;
                sincos(alpha, sin_a, cos_a);
                float2x2 m = float2x2(cos_a, -sin_a, sin_a, cos_a);
                return float3(mul(m, vertex.xz), vertex.y).xzy;
            }

            struct appdata_t
            {
                float4 vertex : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 texcoord : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert(appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                float3 rotated = RotateAroundYInDegrees(v.vertex.xyz, _Rotation);
                o.vertex = UnityObjectToClipPos(rotated);
                o.texcoord = v.vertex.xyz;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                half4 tex = texCUBElod(_Tex, float4(i.texcoord, _MipLevel));
                half4 tex_b = texCUBE(_TexB, i.texcoord);

                half3 c = DecodeHDR(tex, _Tex_HDR);
                half3 c_b = DecodeHDR(tex_b, _TexB_HDR);
                
#if OVERLAY
#else
                c = lerp(c, c_b, _Blend);
#endif // OVERLAY

                c = c * _Tint * unity_ColorSpaceDouble.rgb;
                c *= _Exposure;

#if OVERLAY
                c += c_b * _Blend;
#endif // OVERLAY

                return half4(c, 1.0);
            }
            ENDCG
        }
    }
    Fallback Off
}
