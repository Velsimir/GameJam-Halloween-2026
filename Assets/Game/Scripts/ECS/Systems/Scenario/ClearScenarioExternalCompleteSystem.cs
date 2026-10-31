using Game.Scripts.ECS.Components.Scenario;
using Leopotam.EcsLite;

namespace Game.Scripts.ECS.Systems.Scenario
{
    public class ClearScenarioExternalCompleteSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld _world;
        private EcsFilter _filter;

        public void Init(IEcsSystems systems)
        {
            _world = systems.GetWorld();
            _filter = _world.Filter<ScenarioExternalComplete>().End();
        }

        public void Run(IEcsSystems systems)
        {
            foreach (int entity in _filter)
                _world.DelEntity(entity);
        }
    }
}
