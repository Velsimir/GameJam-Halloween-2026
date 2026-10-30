using Game.Scripts.ECS.Components.Fear;
using Game.Scripts.ECS.Components.Player;
using Leopotam.EcsLite;

namespace Game.Scripts.ECS.Systems.Fear
{
    public class FearZoneContactSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsFilter _zoneFilter;
        private EcsFilter _playerFilter;
        private EcsPool<FearZone> _zonePool;
        private EcsPool<FearZoneContact> _contactPool;

        public void Init(IEcsSystems systems)
        {
            EcsWorld world = systems.GetWorld();
            _zoneFilter = world.Filter<FearZone>().End();
            _playerFilter = world.Filter<PlayerTag>().Inc<FearZoneContact>().End();
            _zonePool = world.GetPool<FearZone>();
            _contactPool = world.GetPool<FearZoneContact>();
        }

        public void Run(IEcsSystems systems)
        {
            float target = 0f;
            foreach (int zoneEntity in _zoneFilter)
            {
                ref FearZone zone = ref _zonePool.Get(zoneEntity);
                if (zone.Inside && zone.Target > target) target = zone.Target;
            }

            foreach (int player in _playerFilter)
                _contactPool.Get(player).Target = target;
        }
    }
}
