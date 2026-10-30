using Game.Scripts.ECS.Components.Interaction;
using Game.Scripts.ECS.Components.Items;
using Game.Scripts.ECS.Extensions;
using Leopotam.EcsLite;

namespace Game.Scripts.ECS.Systems.Items
{
    public class PickupSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld _world;
        private EcsFilter _filter;
        private EcsPool<Pickup> _poolPickup;
        private EcsPool<PickedUp> _poolPickedUp;
        private EcsPool<Interactable> _poolInteractable;
        private EcsPool<Focused> _poolFocused;
        private EcsPool<InteractionPrompt> _poolPrompt;

        public void Init(IEcsSystems systems)
        {
            _world = systems.GetWorld();
            _filter = _world.Filter<Pickup>().Inc<InteractionRequest>().Inc<Interactable>().Exc<PickedUp>().End();
            _poolPickup = _world.GetPool<Pickup>();
            _poolPickedUp = _world.GetPool<PickedUp>();
            _poolInteractable = _world.GetPool<Interactable>();
            _poolFocused = _world.GetPool<Focused>();
            _poolPrompt = _world.GetPool<InteractionPrompt>();
        }

        public void Run(IEcsSystems systems)
        {
            foreach (int entity in _filter)
            {
                ref var pickup = ref _poolPickup.Get(entity);

                if (_world.TryAddItem(pickup.Item) == false)
                    continue;

                _world.RequestThought(pickup.Item.PickupThoughtKey);

                _poolPickedUp.Add(entity);
                _poolInteractable.Del(entity);
                if (_poolFocused.Has(entity)) _poolFocused.Del(entity);
                if (_poolPrompt.Has(entity)) _poolPrompt.Del(entity);
            }
        }
    }
}
