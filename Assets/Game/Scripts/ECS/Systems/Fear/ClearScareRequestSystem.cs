using Game.Scripts.ECS.Components.Fear;
using Leopotam.EcsLite;

namespace Game.Scripts.ECS.Systems.Fear
{
    public class ClearScareRequestSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld _world;
        private EcsFilter _filter;

        public void Init(IEcsSystems systems)
        {
            _world = systems.GetWorld();
            _filter = _world.Filter<ScareRequest>().End();
        }

        public void Run(IEcsSystems systems)
        {
            foreach (int entity in _filter)
                _world.DelEntity(entity);
        }
    }
}
