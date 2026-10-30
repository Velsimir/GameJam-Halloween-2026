Shader "Hidden/Retro/PSXPost"
{
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off
        ZTest Always
        Cull Off
        Blend Off

        Pass
        {
            Name "PsxDownsample"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            TEXTURE2D_X_FLOAT(_PsxDepthTexture);

            float4 _PsxParams;
            float4 _PsxFogParams;
            half4 _PsxFogColor;

            static const float BayerMatrix[16] =
            {
                0.0, 8.0, 2.0, 10.0,
                12.0, 4.0, 14.0, 6.0,
                3.0, 11.0, 1.0, 9.0,
                15.0, 7.0, 13.0, 5.0
            };

            float Bayer4(float2 pixel)
            {
                uint2 p = (uint2)floor(pixel) & 3u;
                return BayerMatrix[p.y * 4u + p.x] / 16.0 - 0.5;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.texcoord;
                half3 color = saturate(SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_PointClamp, uv, 0).rgb);

                float intensity = _PsxParams.w;

                if (_PsxFogParams.z > 0.5)
                {
                    float rawDepth = SAMPLE_TEXTURE2D_X_LOD(_PsxDepthTexture, sampler_PointClamp, uv, 0).r;
                    float eyeDepth = LinearEyeDepth(rawDepth, _ZBufferParams);
                    #if UNITY_REVERSED_Z
                        bool isSky = rawDepth <= 0.00001;
                    #else
                        bool isSky = rawDepth >= 0.99999;
                    #endif
                    float fog = saturate((eyeDepth - _PsxFogParams.x) / max(_PsxFogParams.y - _PsxFogParams.x, 0.0001));
                    fog = isSky ? 1.0 : fog;
                    color = lerp(color, _PsxFogColor.rgb, fog * intensity);
                }

                float levels = max(_PsxParams.x, 2.0);
                float dither = Bayer4(input.positionCS.xy / max(_PsxParams.z, 1.0)) * _PsxParams.y * intensity;
                color = floor(color * (levels - 1.0) + 0.5 + dither) / (levels - 1.0);

                return half4(saturate(color), 1.0);
            }
            ENDHLSL
        }
    }
}
