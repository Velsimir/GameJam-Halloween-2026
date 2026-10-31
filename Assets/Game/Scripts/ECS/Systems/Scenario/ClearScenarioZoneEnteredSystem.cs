using Game.Scripts.ECS.Components.Scenario;
using Leopotam.EcsLite;

namespace Game.Scripts.ECS.Systems.Scenario
{
    public class ClearScenarioZoneEnteredSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsFilter _filter;
        private EcsPool<ScenarioZoneEntered> _pool;

        public void Init(IEcsSystems systems)
        {
            EcsWorld world = systems.GetWorld();
            _filter = world.Filter<ScenarioZoneEntered>().End();
            _pool = world.GetPool<ScenarioZoneEntered>();
        }

        public void Run(IEcsSystems systems)
        {
            foreach (int entity in _filter)
                _pool.Del(entity);
        }
    }
}
