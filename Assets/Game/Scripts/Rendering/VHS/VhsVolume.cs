using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Game.Scripts.Rendering.VHS
{
    [Serializable]
    [VolumeComponentMenu("Retro/VHS")]
    [SupportedOnRenderPipeline(typeof(UniversalRenderPipelineAsset))]
    public class VhsVolume : VolumeComponent, IPostProcessComponent
    {
        public ClampedFloatParameter Intensity = new ClampedFloatParameter(0f, 0f, 1f);

        public ClampedFloatParameter ChannelShift = new ClampedFloatParameter(1f, 0f, 8f);
        public ClampedFloatParameter ChromaBleed = new ClampedFloatParameter(0.5f, 0f, 1f);
        public ClampedFloatParameter ChromaBleedWidth = new ClampedFloatParameter(8f, 0f, 40f);
        public ClampedFloatParameter Saturation = new ClampedFloatParameter(0.9f, 0f, 2f);
        public ClampedFloatParameter WarmShift = new ClampedFloatParameter(0.1f, -1f, 1f);
        public ClampedFloatParameter Contrast = new ClampedFloatParameter(1.05f, 0.5f, 1.5f);

        public ClampedFloatParameter ScanlineStrength = new ClampedFloatParameter(0.15f, 0f, 1f);
        public ClampedFloatParameter ScanlineDensity = new ClampedFloatParameter(360f, 60f, 1080f);
        public ClampedFloatParameter JitterStrength = new ClampedFloatParameter(0.3f, 0f, 6f);
        public ClampedFloatParameter JitterFrequency = new ClampedFloatParameter(12f, 0f, 60f);

        public ClampedFloatParameter TrackingStrength = new ClampedFloatParameter(0.2f, 0f, 1f);
        public ClampedFloatParameter TrackingSpeed = new ClampedFloatParameter(0.05f, 0f, 1f);
        public ClampedFloatParameter TrackingHeight = new ClampedFloatParameter(0.08f, 0.01f, 0.5f);
        public ClampedFloatParameter TrackingNoise = new ClampedFloatParameter(0.2f, 0f, 1f);
        public ClampedFloatParameter VerticalHoldChance = new ClampedFloatParameter(0.02f, 0f, 1f);
        public ClampedFloatParameter VerticalHoldStrength = new ClampedFloatParameter(0.05f, 0f, 1f);

        public ClampedFloatParameter GrainStrength = new ClampedFloatParameter(0.12f, 0f, 1f);
        public ClampedFloatParameter SnowStrength = new ClampedFloatParameter(0.1f, 0f, 1f);
        public ClampedFloatParameter NoiseSize = new ClampedFloatParameter(1.5f, 1f, 8f);
        public ClampedFloatParameter NoiseSpeed = new ClampedFloatParameter(1f, 0f, 4f);
        public ClampedFloatParameter DropoutChance = new ClampedFloatParameter(0.1f, 0f, 1f);

        public ClampedFloatParameter Curvature = new ClampedFloatParameter(0.1f, 0f, 1f);
        public ClampedFloatParameter Vignette = new ClampedFloatParameter(0.25f, 0f, 1f);
        public ClampedFloatParameter HorizontalBlur = new ClampedFloatParameter(0.5f, 0f, 4f);

        public bool IsActive() => Intensity.value > 0f;

        public bool IsTileCompatible() => false;
    }
}
