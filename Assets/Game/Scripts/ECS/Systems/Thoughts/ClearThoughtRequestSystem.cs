using Game.Scripts.ECS.Components.Thoughts;
using Leopotam.EcsLite;

namespace Game.Scripts.ECS.Systems.Thoughts
{
    public class ClearThoughtRequestSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld _world;
        private EcsFilter _filter;

        public void Init(IEcsSystems systems)
        {
            _world = systems.GetWorld();
            _filter = _world.Filter<ThoughtRequest>().End();
        }

        public void Run(IEcsSystems systems)
        {
            foreach (int entity in _filter)
                _world.DelEntity(entity);
        }
    }
}
