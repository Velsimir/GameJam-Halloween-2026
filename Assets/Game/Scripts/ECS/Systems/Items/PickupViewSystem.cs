using Game.Scripts.ECS.Components.Items;
using Leopotam.EcsLite;

namespace Game.Scripts.ECS.Systems.Items
{
    public class PickupViewSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld _world;
        private EcsFilter _filter;
        private EcsPool<Pickup> _poolPickup;

        public void Init(IEcsSystems systems)
        {
            _world = systems.GetWorld();
            _filter = _world.Filter<Pickup>().Inc<PickedUp>().End();
            _poolPickup = _world.GetPool<Pickup>();
        }

        public void Run(IEcsSystems systems)
        {
            foreach (int entity in _filter)
            {
                ref var pickup = ref _poolPickup.Get(entity);
                if (pickup.View != null)
                    pickup.View.SetActive(false);

                _world.DelEntity(entity);
            }
        }
    }
}
