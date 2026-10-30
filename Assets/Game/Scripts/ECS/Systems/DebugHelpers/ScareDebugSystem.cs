#if UNITY_EDITOR
using Game.Scripts.ECS.Components.Fear;
using Leopotam.EcsLite;
using UnityEngine.InputSystem;

namespace Game.Scripts.ECS.Systems.DebugHelpers
{
    public class ScareDebugSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld _world;
        private EcsPool<ScareRequest> _pool;

        public void Init(IEcsSystems systems)
        {
            _world = systems.GetWorld();
            _pool = _world.GetPool<ScareRequest>();
        }

        public void Run(IEcsSystems systems)
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || keyboard.f2Key.wasPressedThisFrame == false) return;

            _pool.Add(_world.NewEntity()).Strength = 1f;
        }
    }
}
#endif
