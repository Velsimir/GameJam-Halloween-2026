using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Game.Scripts.Rendering.PSX
{
    [Serializable]
    [VolumeComponentMenu("Retro/PSX")]
    [SupportedOnRenderPipeline(typeof(UniversalRenderPipelineAsset))]
    public class PsxVolume : VolumeComponent, IPostProcessComponent
    {
        public ClampedFloatParameter Intensity = new ClampedFloatParameter(0f, 0f, 1f);

        public ClampedIntParameter VerticalResolution = new ClampedIntParameter(240, 32, 1080);

        public ClampedIntParameter ColorLevels = new ClampedIntParameter(32, 2, 256);

        public ClampedFloatParameter DitherStrength = new ClampedFloatParameter(0.6f, 0f, 1f);
        public ClampedIntParameter DitherScale = new ClampedIntParameter(1, 1, 8);

        public BoolParameter FogEnabled = new BoolParameter(true);
        public ColorParameter FogColor = new ColorParameter(new Color(0.04f, 0.05f, 0.06f, 1f), false, false, true);
        public MinFloatParameter FogStart = new MinFloatParameter(4f, 0f);
        public MinFloatParameter FogEnd = new MinFloatParameter(24f, 0.1f);

        public bool IsActive() => Intensity.value > 0f;

        public bool IsTileCompatible() => false;
    }
}
