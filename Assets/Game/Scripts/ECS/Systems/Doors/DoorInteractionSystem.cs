using Game.Scripts.ECS.Components.Doors;
using Game.Scripts.ECS.Components.Interaction;
using Game.Scripts.ECS.Components.Scenario;
using Game.Scripts.ECS.Extensions;
using Game.Scripts.Services.Audio;
using Leopotam.EcsLite;

namespace Game.Scripts.ECS.Systems.Doors
{
    public class DoorInteractionSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly IAudioService _audio;

        private EcsWorld _world;
        private EcsFilter _filter;
        private EcsFilter _lockableFilter;
        private EcsPool<LockableDoor> _poolLockable;
        private EcsPool<ScenarioActive> _poolScenarioActive;
        private EcsPool<Door> _poolDoor;
        private EcsPool<Locked> _poolLocked;
        private EcsPool<DoorRattle> _poolRattle;
        private EcsPool<Acting> _poolActing;
        private EcsPool<InteractionPrompt> _poolPrompt;

        public DoorInteractionSystem(IAudioService audio)
        {
            _audio = audio;
        }

        public void Init(IEcsSystems systems)
        {
            _world = systems.GetWorld();

            _filter = _world.Filter<Door>().Inc<InteractionRequest>().End();
            _poolDoor = _world.GetPool<Door>();
            _poolLocked = _world.GetPool<Locked>();
            _poolRattle = _world.GetPool<DoorRattle>();
            _poolActing = _world.GetPool<Acting>();
            _poolPrompt = _world.GetPool<InteractionPrompt>();
            _lockableFilter = _world.Filter<Door>().Inc<LockableDoor>().End();
            _poolLockable = _world.GetPool<LockableDoor>();
            _poolScenarioActive = _world.GetPool<ScenarioActive>();
        }

        public void Run(IEcsSystems systems)
        {
            RefreshLockPrompts();

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

                if (TryLock(entity, ref door))
                    continue;

                door.IsOpen = !door.IsOpen;
                _audio.PlayOn(door.IsOpen ? door.OpenSound : door.CloseSound, door.Pivot);

                if (_poolPrompt.Has(entity))
                    _poolPrompt.Get(entity).Key = door.IsOpen ? door.ClosePromptKey : door.OpenPromptKey;

                if (_poolActing.Has(entity) == false)
                    _poolActing.Add(entity);
            }
        }

        private void RefreshLockPrompts()
        {
            foreach (var entity in _lockableFilter)
            {
                if (_poolLocked.Has(entity) || _poolPrompt.Has(entity) == false)
                    continue;

                ref var door = ref _poolDoor.Get(entity);

                if (door.IsOpen == false && _poolScenarioActive.Has(entity))
                    _poolPrompt.Get(entity).Key = _poolLockable.Get(entity).LockPromptKey;
            }
        }

        private bool TryLock(int entity, ref Door door)
        {
            if (door.IsOpen || _poolLockable.Has(entity) == false || _poolScenarioActive.Has(entity) == false)
                return false;

            ref var lockable = ref _poolLockable.Get(entity);

            if (_world.HasItem(lockable.Item) == false)
            {
                _world.RequestThought(lockable.NoItemThoughtKey);
                return true;
            }

            ref var locked = ref _poolLocked.Add(entity);
            locked.Item = lockable.Item;
            locked.LockedThoughtKey = lockable.LockedThoughtKey;
            locked.UnlockedThoughtKey = lockable.UnlockedThoughtKey;

            if (_poolPrompt.Has(entity))
                _poolPrompt.Get(entity).Key = door.LockedPromptKey;

            _world.RequestThought(lockable.LockDoneThoughtKey);
            _world.CompleteScenarioStep(lockable.StepId);
            return true;
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
