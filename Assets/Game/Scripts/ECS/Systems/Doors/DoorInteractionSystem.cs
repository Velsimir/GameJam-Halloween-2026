using Game.Scripts.ECS.Components.Doors;
using Game.Scripts.ECS.Components.Interaction;
using Leopotam.EcsLite;

namespace Game.Scripts.ECS.Systems.Doors
{
    public class DoorInteractionSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsFilter _filter;
        private EcsPool<Door> _poolDoor;
        private EcsPool<Acting> _poolActing;
        private EcsPool<InteractionPrompt> _poolPrompt;

        public void Init(IEcsSystems systems)
        {
            EcsWorld world = systems.GetWorld();

            _filter = world.Filter<Door>().Inc<InteractionRequest>().End();
            _poolDoor = world.GetPool<Door>();
            _poolActing = world.GetPool<Acting>();
            _poolPrompt = world.GetPool<InteractionPrompt>();
        }

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in _filter)
            {
                ref var door = ref _poolDoor.Get(entity);
                door.IsOpen = !door.IsOpen;

                if (_poolPrompt.Has(entity))
                    _poolPrompt.Get(entity).Key = door.IsOpen ? door.ClosePromptKey : door.OpenPromptKey;
                
                if (_poolActing.Has(entity) == false)
                    _poolActing.Add(entity);
            }
        }
    }
}