using Game.Scripts.Configs;
using Game.Scripts.ECS.Components.Camera;
using Game.Scripts.ECS.Components.Fear;
using Game.Scripts.ECS.Components.Player;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Scripts.ECS.Systems.Camera
{
    public class FearPostProcessSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly FearConfig _config;
        private EcsFilter _cameraFilter;
        private EcsFilter _playerFilter;
        private EcsPool<CameraVolumeRefs> _volumePool;
        private EcsPool<FearFactor> _fearPool;

        public FearPostProcessSystem(FearConfig config)
        {
            _config = config;
        }

        public void Init(IEcsSystems systems)
        {
            EcsWorld world = systems.GetWorld();
            _cameraFilter = world.Filter<CameraTag>().Inc<CameraVolumeRefs>().End();
            _playerFilter = world.Filter<PlayerTag>().Inc<FearFactor>().End();
            _volumePool = world.GetPool<CameraVolumeRefs>();
            _fearPool = world.GetPool<FearFactor>();
        }

        public void Run(IEcsSystems systems)
        {
            if (_playerFilter.GetEntitiesCount() == 0) return;

            float fear = Mathf.Clamp01(_fearPool.Get(_playerFilter.GetRawEntities()[0]).Value);
            float blend = 1f - Mathf.Exp(-_config.PostProcessResponse * Time.deltaTime);

            foreach (int entity in _cameraFilter)
            {
                ref CameraVolumeRefs refs = ref _volumePool.Get(entity);
                refs.Smoothed = Mathf.Lerp(refs.Smoothed, fear, blend);

                refs.Vignette.intensity.value = refs.Smoothed * _config.VignetteMax;
                refs.FilmGrain.intensity.value = refs.Smoothed * _config.FilmGrainMax;
                refs.ChromaticAberration.intensity.value = refs.Smoothed * _config.ChromaticAberrationMax;
            }
        }
    }
}
