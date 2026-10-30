#if UNITY_EDITOR
using Game.Scripts.ECS.Components.Input;
using Game.Scripts.ECS.Components.Player;
using Leopotam.EcsLite;
using UnityEngine.InputSystem;

namespace Game.Scripts.ECS.Systems.DebugHelpers
{
    public class InputBlockToggleDebugSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsFilter _playerFilter;
        private EcsPool<InputBlocked> _blockedPool;

        public void Init(IEcsSystems systems)
        {
            EcsWorld world = systems.GetWorld();
            _playerFilter = world.Filter<PlayerTag>().End();
            _blockedPool = world.GetPool<InputBlocked>();
        }

        public void Run(IEcsSystems systems)
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || keyboard.f1Key.wasPressedThisFrame == false) return;

            foreach (int entity in _playerFilter)
            {
                if (_blockedPool.Has(entity)) _blockedPool.Del(entity);
                else _blockedPool.Add(entity);
            }
        }
    }
}
#endif
