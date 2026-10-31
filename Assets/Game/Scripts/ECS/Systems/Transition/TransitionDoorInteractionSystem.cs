using Game.Scripts.ECS.Components.Doors;
using Game.Scripts.ECS.Components.Input;
using Game.Scripts.ECS.Components.Interaction;
using Game.Scripts.ECS.Components.Player;
using Game.Scripts.ECS.Components.Transition;
using Game.Scripts.ECS.Extensions;
using Leopotam.EcsLite;

namespace Game.Scripts.ECS.Systems.Transition
{
    public class TransitionDoorInteractionSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld _world;
        private EcsFilter _doorFilter;
        private EcsFilter _playerFilter;
        private EcsPool<TransitionDoor> _poolDoor;
        private EcsPool<Locked> _poolLocked;
        private EcsPool<InteractionPrompt> _poolPrompt;
        private EcsPool<Transitioning> _poolTransitioning;
        private EcsPool<InputBlocked> _poolBlocked;

        public void Init(IEcsSystems systems)
        {
            _world = systems.GetWorld();

            _doorFilter = _world.Filter<TransitionDoor>().Inc<InteractionRequest>().End();
            _playerFilter = _world.Filter<PlayerTag>().End();
            _poolDoor = _world.GetPool<TransitionDoor>();
            _poolLocked = _world.GetPool<Locked>();
            _poolPrompt = _world.GetPool<InteractionPrompt>();
            _poolTransitioning = _world.GetPool<Transitioning>();
            _poolBlocked = _world.GetPool<InputBlocked>();
        }

        public void Run(IEcsSystems systems)
        {
            foreach (int doorEntity in _doorFilter)
            {
                foreach (int player in _playerFilter)
                {
                    if (_poolTransitioning.Has(player))
                        break;

                    ref var door = ref _poolDoor.Get(doorEntity);

                    if (door.Target == null)
                        break;

                    if (_poolLocked.Has(doorEntity))
                    {
                        ref var locked = ref _poolLocked.Get(doorEntity);

                        if (_world.CanUnlock(locked) == false)
                        {
                            _world.RequestThought(locked.LockedThoughtKey);
                            break;
                        }

                        _world.RequestThought(locked.UnlockedThoughtKey);
                        _poolLocked.Del(doorEntity);

                        if (_poolPrompt.Has(doorEntity))
                            _poolPrompt.Get(doorEntity).Key = door.GoPromptKey;
                    }

                    ref var transitioning = ref _poolTransitioning.Add(player);
                    transitioning.Phase = TransitionPhase.FadeOut;
                    transitioning.Timer = 0f;
                    transitioning.Target = door.Target;
                    transitioning.EnterThoughtKey = door.EnterThoughtKey;

                    if (_poolBlocked.Has(player) == false)
                        _poolBlocked.Add(player);

                    break;
                }
            }
        }
    }
}
