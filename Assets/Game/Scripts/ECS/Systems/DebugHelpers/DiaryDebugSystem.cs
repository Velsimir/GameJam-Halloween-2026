#if UNITY_EDITOR
using Game.Scripts.ECS.Components.Diary;
using Leopotam.EcsLite;
using UnityEngine.InputSystem;

namespace Game.Scripts.ECS.Systems.DebugHelpers
{
    public class DiaryDebugSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld _world;
        private EcsFilter _requestFilter;

        public void Init(IEcsSystems systems)
        {
            _world = systems.GetWorld();
            _requestFilter = _world.Filter<DiaryOpenRequest>().End();
        }

        public void Run(IEcsSystems systems)
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || keyboard.f8Key.wasPressedThisFrame == false) return;
            if (_requestFilter.GetEntitiesCount() > 0) return;

            _world.GetPool<DiaryOpenRequest>().Add(_world.NewEntity()).Writable = false;
        }
    }
}
#endif
