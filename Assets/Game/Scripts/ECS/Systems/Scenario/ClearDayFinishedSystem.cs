using Game.Scripts.ECS.Components.Scenario;
using Leopotam.EcsLite;

namespace Game.Scripts.ECS.Systems.Scenario
{
    public class ClearDayFinishedSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld _world;
        private EcsFilter _filter;

        public void Init(IEcsSystems systems)
        {
            _world = systems.GetWorld();
            _filter = _world.Filter<DayFinished>().End();
        }

        public void Run(IEcsSystems systems)
        {
            foreach (int entity in _filter)
                _world.DelEntity(entity);
        }
    }
}
