using Game.Scripts.ECS.Components.Scenario;
using Game.Scripts.ECS.Extensions;
using Leopotam.EcsLite;

namespace Game.Scripts.ECS.Systems.Scenario
{
    public class PendingStepCompleteSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld _world;
        private EcsFilter _filter;
        private EcsPool<PendingStepComplete> _pool;

        public void Init(IEcsSystems systems)
        {
            _world = systems.GetWorld();
            _filter = _world.Filter<PendingStepComplete>().End();
            _pool = _world.GetPool<PendingStepComplete>();
        }

        public void Run(IEcsSystems systems)
        {
            foreach (int entity in _filter)
            {
                _world.CompleteScenarioStep(_pool.Get(entity).StepId);
                _world.DelEntity(entity);
            }
        }
    }
}
