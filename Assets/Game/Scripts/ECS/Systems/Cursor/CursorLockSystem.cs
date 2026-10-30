using Game.Scripts.ECS.Components.Input;
using Game.Scripts.ECS.Components.Player;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Scripts.ECS.Systems.Cursor
{
    public class CursorLockSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsFilter _playerFilter;
        private EcsPool<InputBlocked> _blockedPool;
        private bool? _applied;

        public void Init(IEcsSystems systems)
        {
            EcsWorld world = systems.GetWorld();
            _playerFilter = world.Filter<PlayerTag>().End();
            _blockedPool = world.GetPool<InputBlocked>();
        }

        public void Run(IEcsSystems systems)
        {
            foreach (int entity in _playerFilter)
            {
                bool blocked = _blockedPool.Has(entity);
                if (_applied == blocked) return;

                _applied = blocked;
                UnityEngine.Cursor.lockState = blocked ? CursorLockMode.None : CursorLockMode.Locked;
                UnityEngine.Cursor.visible = blocked;
                return;
            }
        }
    }
}
