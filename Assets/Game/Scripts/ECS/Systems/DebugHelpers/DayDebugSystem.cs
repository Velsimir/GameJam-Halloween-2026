#if UNITY_EDITOR
using Game.Scripts.ECS.Components.Scenario;
using Game.Scripts.Services.DayProgress;
using Leopotam.EcsLite;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Scripts.ECS.Systems.DebugHelpers
{
    public class DayDebugSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly IDayProgressService _dayProgress;

        private EcsWorld _world;
        private EcsFilter _finishedFilter;

        public DayDebugSystem(IDayProgressService dayProgress)
        {
            _dayProgress = dayProgress;
        }

        public void Init(IEcsSystems systems)
        {
            _world = systems.GetWorld();
            _finishedFilter = _world.Filter<DayFinished>().End();
        }

        public void Run(IEcsSystems systems)
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || keyboard.f7Key.wasPressedThisFrame == false) return;
            if (_finishedFilter.GetEntitiesCount() > 0) return;

            _world.GetPool<DayFinished>().Add(_world.NewEntity()).Day = _dayProgress.CurrentDay;
            Debug.Log($"[DayFlow] F7: finish day {_dayProgress.CurrentDay}");
        }
    }
}
#endif
