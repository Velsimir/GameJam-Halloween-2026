using Leopotam.EcsLite;

namespace Game.Scripts.ECS
{
    public class EcsWorldProvider : IEcsWorldProvider
    {
        private EcsWorld _world;

        public bool IsAlive => _world != null && _world.IsAlive();

        public EcsWorld World
        {
            get
            {
                if (IsAlive == false)
                    _world = new EcsWorld();

                return _world;
            }
        }

        public void Reset()
        {
            if (_world != null && _world.IsAlive())
                _world.Destroy();

            _world = null;
        }
    }
}
