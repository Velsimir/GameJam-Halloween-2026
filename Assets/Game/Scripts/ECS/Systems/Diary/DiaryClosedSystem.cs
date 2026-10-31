using Game.Scripts.Configs.Scenario;
using Game.Scripts.ECS.Components.Diary;
using Game.Scripts.ECS.Extensions;
using Leopotam.EcsLite;
using Velsi.Typecraft.Dialogue;

namespace Game.Scripts.ECS.Systems.Diary
{
    public class DiaryClosedSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly DayScenario _scenario;

        private EcsWorld _world;
        private EcsFilter _filter;
        private EcsPool<DiaryClosed> _pool;

        public DiaryClosedSystem(DayScenario scenario)
        {
            _scenario = scenario;
        }

        public void Init(IEcsSystems systems)
        {
            _world = systems.GetWorld();
            _filter = _world.Filter<DiaryClosed>().End();
            _pool = _world.GetPool<DiaryClosed>();
        }

        public void Run(IEcsSystems systems)
        {
            foreach (int entity in _filter)
            {
                DiaryClosed closed = _pool.Get(entity);
                if (closed.Writable == false || string.IsNullOrEmpty(closed.CompleteStepId)) continue;

                if (closed.AllRead)
                    _world.CompleteScenarioStep(closed.CompleteStepId);
                else
                    _world.RequestThought(FindReminder(closed.CompleteStepId));
            }
        }

        private DialogueAsset FindReminder(string stepId)
        {
            if (_scenario == null) return null;

            foreach (DayStep step in _scenario.Steps)
            {
                if (step != null && step.Id == stepId)
                    return step.ReminderThought;
            }

            return null;
        }
    }
}
