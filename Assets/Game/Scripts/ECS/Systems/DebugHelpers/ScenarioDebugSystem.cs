#if UNITY_EDITOR
using Game.Scripts.Configs.Scenario;
using Game.Scripts.ECS.Components.Scenario;
using Leopotam.EcsLite;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Scripts.ECS.Systems.DebugHelpers
{
    public class ScenarioDebugSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly DayScenario _scenario;

        private EcsWorld _world;
        private EcsFilter _progressFilter;
        private EcsPool<ScenarioProgress> _poolProgress;
        private EcsPool<ScenarioExternalComplete> _poolExternal;

        public ScenarioDebugSystem(DayScenario scenario)
        {
            _scenario = scenario;
        }

        public void Init(IEcsSystems systems)
        {
            _world = systems.GetWorld();
            _progressFilter = _world.Filter<ScenarioProgress>().End();
            _poolProgress = _world.GetPool<ScenarioProgress>();
            _poolExternal = _world.GetPool<ScenarioExternalComplete>();
        }

        public void Run(IEcsSystems systems)
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return;

            bool skip = keyboard.f3Key.wasPressedThisFrame;
            bool log = keyboard.f6Key.wasPressedThisFrame;
            if (skip == false && log == false) return;

            if (_scenario == null)
            {
                Debug.Log("[Scenario] no scenario for current day");
                return;
            }

            foreach (int entity in _progressFilter)
            {
                ref ScenarioProgress progress = ref _poolProgress.Get(entity);
                if (progress.Finished || progress.StepIndex >= _scenario.Steps.Count)
                {
                    Debug.Log($"[Scenario] day {_scenario.Day} finished");
                    return;
                }

                DayStep step = _scenario.Steps[progress.StepIndex];

                if (log)
                    Debug.Log($"[Scenario] day {_scenario.Day} step {progress.StepIndex + 1}/{_scenario.Steps.Count} id='{step.Id}' target='{step.TargetId}' condition={step.Condition} idle={progress.IdleTime:0.0}s");

                if (skip)
                {
                    _poolExternal.Add(_world.NewEntity()).StepId = step.Id ?? string.Empty;
                    Debug.Log($"[Scenario] F3: force complete step '{step.Id}'");
                }

                return;
            }
        }
    }
}
#endif
