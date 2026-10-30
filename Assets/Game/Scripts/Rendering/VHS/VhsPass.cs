using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

namespace Game.Scripts.Rendering.VHS
{
    public class VhsPass : ScriptableRenderPass
    {
        private const float TimeWrap = 3600f;

        private static readonly int ParamsAId = Shader.PropertyToID("_VhsA");
        private static readonly int ParamsBId = Shader.PropertyToID("_VhsB");
        private static readonly int ParamsCId = Shader.PropertyToID("_VhsC");
        private static readonly int ParamsDId = Shader.PropertyToID("_VhsD");
        private static readonly int ParamsEId = Shader.PropertyToID("_VhsE");
        private static readonly int ParamsFId = Shader.PropertyToID("_VhsF");
        private static readonly int ParamsGId = Shader.PropertyToID("_VhsG");

        private readonly Material _material;

        public VhsPass(Material material)
        {
            _material = material;
            renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing + 1;
        }

        public void Setup(VhsVolume v)
        {
            float k = v.Intensity.value;

            _material.SetVector(ParamsAId, new Vector4(
                v.ChannelShift.value * k,
                v.ChromaBleed.value * k,
                v.ChromaBleedWidth.value,
                v.HorizontalBlur.value * k));

            _material.SetVector(ParamsBId, new Vector4(
                Mathf.Lerp(1f, v.Saturation.value, k),
                v.WarmShift.value * k,
                Mathf.Lerp(1f, v.Contrast.value, k),
                v.Curvature.value * k));

            _material.SetVector(ParamsCId, new Vector4(
                v.ScanlineStrength.value * k,
                v.ScanlineDensity.value,
                v.JitterStrength.value * k,
                v.JitterFrequency.value));

            _material.SetVector(ParamsDId, new Vector4(
                v.TrackingStrength.value * k,
                v.TrackingSpeed.value,
                v.TrackingHeight.value,
                v.TrackingNoise.value));

            _material.SetVector(ParamsEId, new Vector4(
                v.VerticalHoldChance.value * k,
                v.VerticalHoldStrength.value,
                v.GrainStrength.value * k,
                v.SnowStrength.value * k));

            _material.SetVector(ParamsFId, new Vector4(
                v.NoiseSize.value,
                v.NoiseSpeed.value,
                v.DropoutChance.value * k,
                v.Vignette.value * k));

            _material.SetVector(ParamsGId, new Vector4(0f, Time.time % TimeWrap, 0f, 0f));
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

            if (resourceData.isActiveTargetBackBuffer)
            {
                return;
            }

            TextureHandle source = resourceData.activeColorTexture;
            if (!source.IsValid())
            {
                return;
            }

            TextureDesc desc = source.GetDescriptor(renderGraph);
            desc.name = "_VhsTemp";
            desc.depthBufferBits = DepthBits.None;
            desc.msaaSamples = MSAASamples.None;
            desc.clearBuffer = false;

            TextureHandle temp = renderGraph.CreateTexture(desc);

            var parameters = new RenderGraphUtils.BlitMaterialParameters(source, temp, _material, 0);
            renderGraph.AddBlitPass(parameters, "VHS");
            renderGraph.AddBlitPass(temp, source, Vector2.one, Vector2.zero, passName: "VHS Copy");
        }
    }
}
