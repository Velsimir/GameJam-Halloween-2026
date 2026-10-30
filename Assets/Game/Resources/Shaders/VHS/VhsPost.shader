Shader "Hidden/Retro/VHSPost"
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
            Name "Vhs"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float4 _VhsA;
            float4 _VhsB;
            float4 _VhsC;
            float4 _VhsD;
            float4 _VhsE;
            float4 _VhsF;
            float4 _VhsG;

            #define CHANNEL_SHIFT _VhsA.x
            #define CHROMA_BLEED _VhsA.y
            #define CHROMA_WIDTH _VhsA.z
            #define H_BLUR _VhsA.w
            #define SATURATION _VhsB.x
            #define WARM _VhsB.y
            #define CONTRAST _VhsB.z
            #define CURVATURE _VhsB.w
            #define SCAN_STRENGTH _VhsC.x
            #define SCAN_DENSITY _VhsC.y
            #define JITTER_STRENGTH _VhsC.z
            #define JITTER_FREQ _VhsC.w
            #define TRACK_STRENGTH _VhsD.x
            #define TRACK_SPEED _VhsD.y
            #define TRACK_HEIGHT _VhsD.z
            #define TRACK_NOISE _VhsD.w
            #define VHOLD_CHANCE _VhsE.x
            #define VHOLD_STRENGTH _VhsE.y
            #define GRAIN _VhsE.z
            #define SNOW _VhsE.w
            #define NOISE_SIZE _VhsF.x
            #define NOISE_SPEED _VhsF.y
            #define DROPOUT _VhsF.z
            #define VIGNETTE _VhsF.w
            #define TIME _VhsG.y

            float Hash11(float p)
            {
                p = frac(p * 0.1031);
                p *= p + 33.33;
                p *= p + p;
                return frac(p);
            }

            float Hash21(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * 0.1031);
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.x + p3.y) * p3.z);
            }

            float3 Fetch(float2 uv)
            {
                return SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv, 0).rgb;
            }

            float3 FetchBlur(float2 uv, float dx)
            {
                return (Fetch(uv - float2(dx, 0)) + Fetch(uv) * 2.0 + Fetch(uv + float2(dx, 0))) * 0.25;
            }

            float3 RgbToYuv(float3 c)
            {
                float y = dot(c, float3(0.299, 0.587, 0.114));
                return float3(y, (c.b - y) * 0.565, (c.r - y) * 0.713);
            }

            float3 YuvToRgb(float3 c)
            {
                float r = c.x + c.z / 0.713;
                float b = c.x + c.y / 0.565;
                float g = (c.x - 0.299 * r - 0.114 * b) / 0.587;
                return float3(r, g, b);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 res = _BlitTexture_TexelSize.zw;
                float2 uv = input.texcoord;
                float t = TIME;

                float2 cc = uv * 2.0 - 1.0;
                float r2 = dot(cc, cc);
                cc *= 1.0 + CURVATURE * 0.18 * r2;
                uv = cc * 0.5 + 0.5;
                float outside = (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0) ? 1.0 : 0.0;
                uv = saturate(uv);

                float holdSlot = floor(t * 2.0);
                float holdActive = step(Hash11(holdSlot * 7.13), VHOLD_CHANCE);
                float holdPhase = frac(t * 2.0);
                float holdSign = Hash11(holdSlot * 3.7) > 0.5 ? 1.0 : -1.0;
                float roll = holdActive * VHOLD_STRENGTH * holdSign * (1.0 - holdPhase) * (1.0 - holdPhase);
                uv.y = frac(uv.y + roll);

                float scanLine = floor(uv.y * SCAN_DENSITY);
                float jitterStep = floor(t * JITTER_FREQ);
                float jitter = (Hash21(float2(scanLine, jitterStep)) - 0.5) * 2.0;
                uv.x += jitter * JITTER_STRENGTH / res.x;

                float bandCenter = frac(t * TRACK_SPEED);
                float bandDist = frac(uv.y - bandCenter + 0.5) - 0.5;
                float halfHeight = max(TRACK_HEIGHT * 0.5, 0.0001);
                float band = saturate(1.0 - abs(bandDist) / halfHeight);
                band = band * band * (3.0 - 2.0 * band);
                float bandLine = floor(uv.y * res.y * 0.5);
                float bandRand = Hash21(float2(bandLine, floor(t * 30.0))) - 0.5;
                uv.x += band * TRACK_STRENGTH * 0.06 * (bandRand * 2.0 + sin(bandDist * 60.0 + t * 20.0));

                float shift = CHANNEL_SHIFT / res.x;
                float blurDx = H_BLUR / res.x;

                float3 col;
                col.r = FetchBlur(uv + float2(shift, 0), blurDx).r;
                col.g = FetchBlur(uv, blurDx).g;
                col.b = FetchBlur(uv - float2(shift, 0), blurDx).b;

                float3 yuv = RgbToYuv(col);
                float2 chromaAccum = yuv.yz;
                float bleedStep = CHROMA_WIDTH / 6.0 / res.x;
                for (int i = 1; i < 6; i++)
                {
                    float3 tap = RgbToYuv(Fetch(uv - float2(bleedStep * i, 0)));
                    chromaAccum += tap.yz;
                }
                chromaAccum /= 6.0;
                yuv.yz = lerp(yuv.yz, chromaAccum, CHROMA_BLEED);
                col = YuvToRgb(yuv);

                float luma = dot(col, float3(0.299, 0.587, 0.114));
                col = lerp(luma.xxx, col, SATURATION);
                col.r *= 1.0 + WARM * 0.15;
                col.b *= 1.0 - WARM * 0.15;
                col = (col - 0.5) * CONTRAST + 0.5;

                float scan = 0.5 + 0.5 * sin(uv.y * SCAN_DENSITY * 6.2831853);
                col *= lerp(1.0, scan, SCAN_STRENGTH);

                float noiseTime = floor(t * NOISE_SPEED * 30.0);
                float2 noiseCell = floor(uv * res / NOISE_SIZE);
                float grain = Hash21(noiseCell + noiseTime * 17.0) - 0.5;
                col += grain * GRAIN * 0.6;

                float snow = Hash21(noiseCell + noiseTime * 31.0 + 91.7);
                col += step(1.0 - SNOW * 0.06, snow) * 0.6;

                float bandNoise = Hash21(floor(uv * res * float2(0.5, 1.0)) + noiseTime * 13.0);
                col += band * TRACK_NOISE * (bandNoise - 0.35) * 0.8;

                float dropLine = floor(uv.y * res.y / NOISE_SIZE);
                float dropSlot = floor(t * NOISE_SPEED * 20.0);
                float dropHit = step(Hash21(float2(dropLine, dropSlot)), DROPOUT * 0.03);
                float dropStart = Hash21(float2(dropLine, dropSlot + 1.7));
                float dropLen = 0.05 + 0.2 * Hash21(float2(dropLine, dropSlot + 3.3));
                float dropInside = step(dropStart, uv.x) * step(uv.x, dropStart + dropLen);
                col = lerp(col, float3(1.0, 1.0, 1.0), dropHit * dropInside * 0.9);

                float vig = 1.0 - VIGNETTE * smoothstep(0.35, 1.25, length(cc));
                col *= vig;
                col *= 1.0 - outside;

                return half4(saturate(col), 1.0);
            }
            ENDHLSL
        }
    }
}
