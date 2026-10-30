using Game.Scripts.Configs;
using Game.Scripts.ECS.Components.Camera;
using Game.Scripts.ECS.Components.Fear;
using Game.Scripts.ECS.Components.Player;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Scripts.ECS.Systems.Camera
{
    public class CameraScareShakeSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly FearConfig _config;
        private EcsFilter _filter;
        private EcsPool<ScareShake> _shakePool;
        private EcsPool<CameraOffset> _offsetPool;

        public CameraScareShakeSystem(FearConfig config)
        {
            _config = config;
        }

        public void Init(IEcsSystems systems)
        {
            EcsWorld world = systems.GetWorld();
            _filter = world.Filter<PlayerTag>().Inc<ScareShake>().Inc<CameraOffset>().End();
            _shakePool = world.GetPool<ScareShake>();
            _offsetPool = world.GetPool<CameraOffset>();
        }

        public void Run(IEcsSystems systems)
        {
            foreach (int entity in _filter)
            {
                ref ScareShake shake = ref _shakePool.Get(entity);
                shake.Elapsed += Time.deltaTime;

                if (shake.Elapsed >= _config.ScareDuration)
                {
                    _shakePool.Del(entity);
                    continue;
                }

                float remaining = 1f - shake.Elapsed / _config.ScareDuration;
                float envelope = remaining * remaining * shake.Strength;
                float phase = shake.Elapsed * _config.ScareFrequency;

                ref CameraOffset offset = ref _offsetPool.Get(entity);
                offset.Position += new Vector3(Mathf.Sin(phase), Mathf.Sin(phase * 1.3f + 1f), 0f) * (_config.ScarePositionAmplitude * envelope);
                offset.Rotation += new Vector3(Mathf.Sin(phase * 1.1f), Mathf.Sin(phase * 0.9f + 2f), Mathf.Sin(phase * 1.7f) * 0.5f) * (_config.ScareRotationAmplitude * envelope);
                offset.FovDelta += _config.ScareFovAmplitude * envelope;
            }
        }
    }
}
