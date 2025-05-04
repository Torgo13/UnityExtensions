Shader "Cubemap"
{
    Properties
    {
        [MainColor] _Tint("Tint Color", Color) = (1.0, 1.0, 1.0, 1.0)
        [Gamma] _Exposure("Exposure", Range(0, 8)) = 1.0
        _Rotation("Rotation", Range(0, 360)) = 0
        _MipLevel("Mip Level", Range(1, 16)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        Cull Off //ZWrite Off
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Macros.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            CBUFFER_START(UnityPerMaterial)
            half4 _Tint;
            half _Exposure;
            half _MipLevel;
            float _Rotation;
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
                half3 c = SAMPLE_TEXTURECUBE_LOD(unity_SpecCube0, samplerunity_SpecCube0, IN.texcoord, _MipLevel).rgb;
                c *= _Tint.rgb;
                c *= _Exposure;

                return half4(c, 1.0);
            }
            ENDHLSL
        }
    }
}
