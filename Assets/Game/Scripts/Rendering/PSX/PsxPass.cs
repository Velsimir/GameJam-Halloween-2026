using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

namespace Game.Scripts.Rendering.PSX
{
    public class PsxPass : ScriptableRenderPass
    {
        private static readonly int ParamsId = Shader.PropertyToID("_PsxParams");
        private static readonly int FogParamsId = Shader.PropertyToID("_PsxFogParams");
        private static readonly int FogColorId = Shader.PropertyToID("_PsxFogColor");
        private static readonly int DepthTextureId = Shader.PropertyToID("_PsxDepthTexture");

        private readonly Material _material;

        private int _verticalResolution;
        private int _levels;
        private float _ditherStrength;
        private int _ditherScale;
        private float _intensity;
        private bool _fogEnabled;
        private Color _fogColor;
        private float _fogStart;
        private float _fogEnd;

        public PsxPass(Material material)
        {
            _material = material;
            renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        }

        public void Setup(PsxVolume volume)
        {
            _intensity = volume.Intensity.value;
            _verticalResolution = volume.VerticalResolution.value;
            _levels = volume.ColorLevels.value;
            _ditherStrength = volume.DitherStrength.value;
            _ditherScale = volume.DitherScale.value;
            _fogEnabled = volume.FogEnabled.value;
            _fogColor = volume.FogColor.value;
            _fogStart = volume.FogStart.value;
            _fogEnd = Mathf.Max(volume.FogEnd.value, volume.FogStart.value + 0.01f);
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

            if (resourceData.isActiveTargetBackBuffer)
            {
                return;
            }

            TextureHandle source = resourceData.activeColorTexture;
            TextureHandle depth = resourceData.activeDepthTexture;
            if (!source.IsValid())
            {
                return;
            }

            bool hasDepth = depth.IsValid();

            TextureDesc desc = source.GetDescriptor(renderGraph);
            int height = Mathf.Clamp(_verticalResolution, 1, desc.height);
            int width = Mathf.Max(1, Mathf.RoundToInt(height * (float)desc.width / desc.height));
            desc.name = "_PsxLowRes";
            desc.width = width;
            desc.height = height;
            desc.depthBufferBits = DepthBits.None;
            desc.msaaSamples = MSAASamples.None;
            desc.filterMode = FilterMode.Point;
            desc.clearBuffer = false;

            TextureHandle lowRes = renderGraph.CreateTexture(desc);

            _material.SetVector(ParamsId, new Vector4(_levels, _ditherStrength, _ditherScale, _intensity));
            _material.SetVector(FogParamsId, new Vector4(_fogStart, _fogEnd, _fogEnabled && hasDepth ? 1f : 0f, 0f));
            _material.SetColor(FogColorId, _fogColor);

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("PSX Downsample", out PassData passData))
            {
                passData.Source = source;
                passData.Depth = depth;
                passData.HasDepth = hasDepth;
                passData.Material = _material;

                builder.UseTexture(source);
                if (hasDepth)
                {
                    builder.UseTexture(depth);
                }

                builder.SetRenderAttachment(lowRes, 0);
                builder.AllowGlobalStateModification(true);
                builder.SetRenderFunc(static (PassData data, RasterGraphContext context) =>
                {
                    if (data.HasDepth)
                    {
                        context.cmd.SetGlobalTexture(DepthTextureId, data.Depth, RenderTextureSubElement.Depth);
                    }

                    Blitter.BlitTexture(context.cmd, data.Source, new Vector4(1f, 1f, 0f, 0f), data.Material, 0);
                });
            }

            renderGraph.AddBlitPass(lowRes, source, Vector2.one, Vector2.zero,
                filterMode: RenderGraphUtils.BlitFilterMode.ClampNearest, passName: "PSX Upscale");
        }

        private class PassData
        {
            public TextureHandle Source;
            public TextureHandle Depth;
            public bool HasDepth;
            public Material Material;
        }
    }
}
