Shader "Retro/PSX Lit"
{
    Properties
    {
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        [MainColor] _BaseColor("Color", Color) = (1, 1, 1, 1)
        [HDR] _EmissionColor("Emission Color", Color) = (0, 0, 0, 1)
        _EmissionMap("Emission Map", 2D) = "white" {}
        _SnapResolution("Vertex Snap Grid (vertical px, 0 = off)", Float) = 240
        _AffineStrength("Affine Texture Mapping", Range(0, 1)) = 1
        [Toggle(_PSX_POINT_FILTER)] _PointFilter("Point Filtering", Float) = 1
        [Toggle(_PSX_VERTEX_LIGHTING)] _VertexLighting("Vertex Lighting", Float) = 1
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            half4 _BaseColor;
            half4 _EmissionColor;
            float _SnapResolution;
            float _AffineStrength;
        CBUFFER_END
        ENDHLSL

        Pass
        {
            Name "ForwardOnly"
            Tags { "LightMode" = "UniversalForwardOnly" }

            Cull Back
            ZWrite On

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex vert
            #pragma fragment frag

            #pragma shader_feature_local_fragment _PSX_POINT_FILTER
            #pragma shader_feature_local _PSX_VERTEX_LIGHTING
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap);
            TEXTURE2D(_EmissionMap);
            SamplerState sampler_psx_point_repeat;
            SamplerState sampler_psx_linear_repeat;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uvPerspective : TEXCOORD0;
                noperspective float2 uvAffine : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
                half3 normalWS : TEXCOORD3;
                half3 vertexLight : TEXCOORD4;
            };

            half3 AccumulateLight(float3 positionWS, half3 normalWS, bool useShadows)
            {
                half3 result = SampleSH(normalWS);

                Light mainLight;
                if (useShadows)
                {
                    mainLight = GetMainLight(TransformWorldToShadowCoord(positionWS));
                }
                else
                {
                    mainLight = GetMainLight();
                }
                result += mainLight.color * (saturate(dot(normalWS, mainLight.direction)) * mainLight.distanceAttenuation * mainLight.shadowAttenuation);

                #if defined(_ADDITIONAL_LIGHTS) && !defined(USE_FORWARD_PLUS)
                    uint count = GetAdditionalLightsCount();
                    for (uint i = 0u; i < count; ++i)
                    {
                        Light light = GetAdditionalLight(i, positionWS);
                        result += light.color * (saturate(dot(normalWS, light.direction)) * light.distanceAttenuation);
                    }
                #endif

                return result;
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs positions = GetVertexPositionInputs(input.positionOS.xyz);
                float4 clip = positions.positionCS;

                if (_SnapResolution > 0.5 && clip.w > 0.0001)
                {
                    float2 grid = float2(_SnapResolution * _ScreenParams.x / _ScreenParams.y, _SnapResolution) * 0.5;
                    clip.xy = round(clip.xy / clip.w * grid) / grid * clip.w;
                }

                float2 uv = TRANSFORM_TEX(input.uv, _BaseMap);
                half3 normalWS = TransformObjectToWorldNormal(input.normalOS);

                output.positionCS = clip;
                output.uvPerspective = uv;
                output.uvAffine = uv;
                output.positionWS = positions.positionWS;
                output.normalWS = normalWS;

                #if defined(_PSX_VERTEX_LIGHTING)
                    output.vertexLight = AccumulateLight(positions.positionWS, normalWS, false);
                #else
                    output.vertexLight = 0;
                #endif

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 uv = lerp(input.uvPerspective, input.uvAffine, _AffineStrength);

                #if defined(_PSX_POINT_FILTER)
                    half4 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_psx_point_repeat, uv);
                    half3 emission = SAMPLE_TEXTURE2D(_EmissionMap, sampler_psx_point_repeat, uv).rgb;
                #else
                    half4 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_psx_linear_repeat, uv);
                    half3 emission = SAMPLE_TEXTURE2D(_EmissionMap, sampler_psx_linear_repeat, uv).rgb;
                #endif

                albedo *= _BaseColor;

                #if defined(_PSX_VERTEX_LIGHTING)
                    half3 light = input.vertexLight;
                #else
                    half3 light = AccumulateLight(input.positionWS, normalize(input.normalWS), true);
                #endif

                half3 color = albedo.rgb * light + emission * _EmissionColor.rgb;
                return half4(color, 1.0);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex vertShadow
            #pragma fragment fragShadow
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            float3 _LightDirection;
            float3 _LightPosition;

            struct ShadowAttributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct ShadowVaryings
            {
                float4 positionCS : SV_POSITION;
            };

            ShadowVaryings vertShadow(ShadowAttributes input)
            {
                ShadowVaryings output;
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

                #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                    float3 lightDirectionWS = normalize(_LightPosition - positionWS);
                #else
                    float3 lightDirectionWS = _LightDirection;
                #endif

                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));
                #if UNITY_REVERSED_Z
                    positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #else
                    positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #endif
                output.positionCS = positionCS;
                return output;
            }

            half4 fragShadow(ShadowVaryings input) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            ZWrite On
            ColorMask R
            Cull Back

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex vertDepth
            #pragma fragment fragDepth

            struct DepthAttributes
            {
                float4 positionOS : POSITION;
            };

            struct DepthVaryings
            {
                float4 positionCS : SV_POSITION;
            };

            DepthVaryings vertDepth(DepthAttributes input)
            {
                DepthVaryings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }

            half fragDepth(DepthVaryings input) : SV_Target
            {
                return input.positionCS.z;
            }
            ENDHLSL
        }
    }

    FallBack Off
}
