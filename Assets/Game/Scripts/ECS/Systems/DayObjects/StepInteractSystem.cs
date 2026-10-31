using Game.Scripts.ECS.Components.DayObjects;
using Game.Scripts.ECS.Components.Interaction;
using Game.Scripts.ECS.Extensions;
using Leopotam.EcsLite;

namespace Game.Scripts.ECS.Systems.DayObjects
{
    public class StepInteractSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld _world;
        private EcsFilter _filter;
        private EcsPool<StepInteract> _pool;

        public void Init(IEcsSystems systems)
        {
            _world = systems.GetWorld();
            _filter = _world.Filter<StepInteract>().Inc<InteractionRequest>().End();
            _pool = _world.GetPool<StepInteract>();
        }

        public void Run(IEcsSystems systems)
        {
            foreach (int entity in _filter)
            {
                ref var interact = ref _pool.Get(entity);
                _world.RequestThought(interact.ThoughtKey);
                _world.CompleteScenarioStep(interact.StepId);
            }
        }
    }
}
