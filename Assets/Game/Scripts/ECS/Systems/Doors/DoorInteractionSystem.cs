using Game.Scripts.ECS.Components.Doors;
using Game.Scripts.ECS.Components.Interaction;
using Game.Scripts.ECS.Extensions;
using Leopotam.EcsLite;

namespace Game.Scripts.ECS.Systems.Doors
{
    public class DoorInteractionSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld _world;
        private EcsFilter _filter;
        private EcsPool<Door> _poolDoor;
        private EcsPool<Locked> _poolLocked;
        private EcsPool<DoorRattle> _poolRattle;
        private EcsPool<Acting> _poolActing;
        private EcsPool<InteractionPrompt> _poolPrompt;

        public void Init(IEcsSystems systems)
        {
            _world = systems.GetWorld();

            _filter = _world.Filter<Door>().Inc<InteractionRequest>().End();
            _poolDoor = _world.GetPool<Door>();
            _poolLocked = _world.GetPool<Locked>();
            _poolRattle = _world.GetPool<DoorRattle>();
            _poolActing = _world.GetPool<Acting>();
            _poolPrompt = _world.GetPool<InteractionPrompt>();
        }

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in _filter)
            {
                ref var door = ref _poolDoor.Get(entity);

                if (_poolLocked.Has(entity))
                {
                    ref var locked = ref _poolLocked.Get(entity);

                    if (_world.CanUnlock(locked) == false)
                    {
                        _world.RequestThought(locked.LockedThoughtKey);
                        Rattle(entity);
                        continue;
                    }

                    _world.RequestThought(locked.UnlockedThoughtKey);
                    _poolLocked.Del(entity);
                }

                door.IsOpen = !door.IsOpen;

                if (_poolPrompt.Has(entity))
                    _poolPrompt.Get(entity).Key = door.IsOpen ? door.ClosePromptKey : door.OpenPromptKey;

                if (_poolActing.Has(entity) == false)
                    _poolActing.Add(entity);
            }
        }

        private void Rattle(int entity)
        {
            if (_poolRattle.Has(entity))
                _poolRattle.Get(entity).Time = 0f;
            else
                _poolRattle.Add(entity).Time = 0f;

            if (_poolActing.Has(entity) == false)
                _poolActing.Add(entity);
        }
    }
}
