using Game.Scripts.Configs;
using Game.Scripts.ECS.Components.Fear;
using Game.Scripts.ECS.Components.Player;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Scripts.ECS.Systems.Fear
{
    public class FearFactorSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly FearConfig _config;
        private EcsFilter _filter;
        private EcsPool<FearFactor> _fearPool;
        private EcsPool<FearZoneContact> _contactPool;

        public FearFactorSystem(FearConfig config)
        {
            _config = config;
        }

        public void Init(IEcsSystems systems)
        {
            EcsWorld world = systems.GetWorld();
            _filter = world.Filter<PlayerTag>().Inc<FearFactor>().Inc<FearZoneContact>().End();
            _fearPool = world.GetPool<FearFactor>();
            _contactPool = world.GetPool<FearZoneContact>();
        }

        public void Run(IEcsSystems systems)
        {
            foreach (int entity in _filter)
            {
                ref FearFactor fear = ref _fearPool.Get(entity);
                float target = Mathf.Clamp01(_contactPool.Get(entity).Target) * _config.MaxValue;
                float speed = fear.Value < target ? _config.GrowthSpeed : _config.DecaySpeed;
                fear.Value = Mathf.MoveTowards(fear.Value, target, speed * Time.deltaTime);
            }
        }
    }
}
