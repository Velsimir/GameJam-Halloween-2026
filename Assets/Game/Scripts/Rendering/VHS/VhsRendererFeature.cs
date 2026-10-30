using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Game.Scripts.Rendering.VHS
{
    public class VhsRendererFeature : ScriptableRendererFeature
    {
        private const string ShaderPath = "Shaders/VHS/VhsPost";

        private Material _material;
        private VhsPass _pass;

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
            _pass = new VhsPass(_material);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (_pass == null || renderingData.cameraData.cameraType != CameraType.Game)
            {
                return;
            }

            VhsVolume volume = VolumeManager.instance.stack.GetComponent<VhsVolume>();
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
