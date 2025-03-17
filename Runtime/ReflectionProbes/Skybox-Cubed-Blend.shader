// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)
Shader "Skybox/CubemapBlend"
{
    Properties
    {
        _Tint("Tint Color", Color) = (1.0, 1.0, 1.0, 1.0)
        [Gamma] _Exposure("Exposure", Range(0, 8)) = 1.0
        _Rotation("Rotation", Range(0, 360)) = 0
        [NoScaleOffset] _TexA("CubemapA (HDR)", Cube) = "grey" {}
        [NoScaleOffset] _TexB("CubemapB (HDR)", Cube) = "grey" {}
        _Blend("Blend", Range(0, 1)) = 0.0
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

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Macros.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURECUBE(_TexA);
            TEXTURECUBE(_TexB);
            SAMPLER(sampler_TexA);
            half4 _TexA_HDR;
            half4 _TexB_HDR;
            
            CBUFFER_START(UnityPerMaterial)
            half4 _Tint;
            half _Exposure;
            float _Rotation;
            float _Blend;
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
                half4 tex_a = SAMPLE_TEXTURECUBE_LOD(_TexA, sampler_TexA, IN.texcoord, 0);
                half4 tex_b = SAMPLE_TEXTURECUBE_LOD(_TexB, sampler_TexA, IN.texcoord, 0);

                half3 c = tex_a.rgb;
                half3 c_b = tex_b.rgb;

                c = lerp(c, c_b, _Blend);
                c = c * _Tint.rgb;
                c *= _Exposure;

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

            samplerCUBE _TexA;
            samplerCUBE _TexB;
            half4 _TexA_HDR;
            half4 _TexB_HDR;
            half4 _Tint;
            half _Exposure;
            float _Rotation;
            float _Blend;

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
                half4 tex_a = texCUBE(_TexA, i.texcoord);
                half4 tex_b = texCUBE(_TexB, i.texcoord);

                half3 c = DecodeHDR(tex_a, _TexA_HDR);
                half3 c_b = DecodeHDR(tex_b, _TexB_HDR);

                c = lerp(c, c_b, _Blend);
                c = c * _Tint.rgb * unity_ColorSpaceDouble.rgb;
                c *= _Exposure;

                return half4(c, 1.0);
            }
            ENDCG
        }
    }
    Fallback Off
}

/*
Shader "Unlit/Skybox-Cubed-Blend"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
*/