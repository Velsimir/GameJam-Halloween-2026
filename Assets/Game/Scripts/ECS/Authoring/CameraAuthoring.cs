using Game.Scripts.Configs;
using Game.Scripts.ECS.Components.Camera;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Zenject;

namespace Game.Scripts.ECS.Authoring
{
    public class CameraAuthoring : MonoBehaviour
    {
        [SerializeField] private CinemachineMixingCamera _mixingCamera;
        [SerializeField] private Volume _volume;

        [Inject] IEcsWorldProvider _ecsWorldProvider;
        [Inject] GameConfigs _gameConfigs;

        private void Start()
        {
            int camera = _ecsWorldProvider.World.NewEntity();
            _ecsWorldProvider.World.GetPool<CameraTag>().Add(camera);

            ref var cinemachineRefs = ref _ecsWorldProvider.World.GetPool<CinemachineRefs>().Add(camera);
            cinemachineRefs.CameraMixing = _mixingCamera;
            cinemachineRefs.Cameras = _mixingCamera.GetComponentsInChildren<CinemachineCamera>(true);
            cinemachineRefs.BaseFov = new float[cinemachineRefs.Cameras.Length];
            for (int i = 0; i < cinemachineRefs.Cameras.Length; i++)
                cinemachineRefs.BaseFov[i] = cinemachineRefs.Cameras[i].Lens.FieldOfView;

            if (_volume == null) return;

            VolumeProfile profile = _volume.profile;
            ref var volumeRefs = ref _ecsWorldProvider.World.GetPool<CameraVolumeRefs>().Add(camera);
            volumeRefs.Vignette = GetOrAdd<Vignette>(profile);
            volumeRefs.FilmGrain = GetOrAdd<FilmGrain>(profile);
            volumeRefs.ChromaticAberration = GetOrAdd<ChromaticAberration>(profile);

            volumeRefs.Vignette.intensity.Override(0f);
            volumeRefs.Vignette.smoothness.Override(_gameConfigs.FearConfig.VignetteSmoothness);
            volumeRefs.FilmGrain.type.Override(FilmGrainLookup.Thin1);
            volumeRefs.FilmGrain.intensity.Override(0f);
            volumeRefs.ChromaticAberration.intensity.Override(0f);
        }

        private static T GetOrAdd<T>(VolumeProfile profile) where T : VolumeComponent
        {
            return profile.TryGet(out T component) ? component : profile.Add<T>(true);
        }
    }
}
