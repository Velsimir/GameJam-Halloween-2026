using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Game.Scripts.Rendering.PSX
{
    public class PsxRendererFeature : ScriptableRendererFeature
    {
        private const string ShaderPath = "Shaders/PSX/PsxPost";

        private Material _material;
        private PsxPass _pass;

        public override void Create()
        {
            CoreUtils.Destroy(_material);
            _material = null;
            _pass = null;

            Shader shader = Resources.Load<Shader>(ShaderPath);
            if (shader == null)
            {
                return;
            }

            _material = CoreUtils.CreateEngineMaterial(shader);
            _pass = new PsxPass(_material);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (_pass == null || renderingData.cameraData.cameraType != CameraType.Game)
            {
                return;
            }

            PsxVolume volume = VolumeManager.instance.stack.GetComponent<PsxVolume>();
            if (volume == null || !volume.IsActive())
            {
                return;
            }

            _pass.Setup(volume);
            renderer.EnqueuePass(_pass);
        }

        protected override void Dispose(bool disposing)
        {
            CoreUtils.Destroy(_material);
            _material = null;
            _pass = null;
        }
    }
}
