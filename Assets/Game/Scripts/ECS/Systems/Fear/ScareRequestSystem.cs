using Game.Scripts.Configs;
using Game.Scripts.ECS.Components.Fear;
using Game.Scripts.ECS.Components.Player;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Scripts.ECS.Systems.Fear
{
    public class ScareRequestSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly FearConfig _config;
        private EcsFilter _requestFilter;
        private EcsFilter _playerFilter;
        private EcsPool<ScareRequest> _requestPool;
        private EcsPool<FearFactor> _fearPool;
        private EcsPool<ScareShake> _shakePool;

        public ScareRequestSystem(FearConfig config)
        {
            _config = config;
        }

        public void Init(IEcsSystems systems)
        {
            EcsWorld world = systems.GetWorld();
            _requestFilter = world.Filter<ScareRequest>().End();
            _playerFilter = world.Filter<PlayerTag>().Inc<FearFactor>().End();
            _requestPool = world.GetPool<ScareRequest>();
            _fearPool = world.GetPool<FearFactor>();
            _shakePool = world.GetPool<ScareShake>();
        }

        public void Run(IEcsSystems systems)
        {
            foreach (int requestEntity in _requestFilter)
            {
                float strength = _requestPool.Get(requestEntity).Strength;

                foreach (int player in _playerFilter)
                {
                    ref FearFactor fear = ref _fearPool.Get(player);
                    fear.Value = Mathf.Min(fear.Value + strength * _config.ScareFearBoost, _config.MaxValue);

                    if (!_shakePool.Has(player)) _shakePool.Add(player);
                    ref ScareShake shake = ref _shakePool.Get(player);
                    shake.Elapsed = 0f;
                    shake.Strength = strength;
                }
            }
        }
    }
}
