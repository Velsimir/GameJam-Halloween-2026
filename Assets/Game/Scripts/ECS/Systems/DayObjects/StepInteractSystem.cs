using Game.Scripts.Configs.Scenario;
using Game.Scripts.ECS.Components.DayObjects;
using Game.Scripts.ECS.Components.Input;
using Game.Scripts.ECS.Components.Interaction;
using Game.Scripts.ECS.Components.Player;
using Game.Scripts.ECS.Components.Transition;
using Game.Scripts.ECS.Extensions;
using Leopotam.EcsLite;

namespace Game.Scripts.ECS.Systems.DayObjects
{
    public class StepInteractSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly DayScenario _scenario;

        private EcsWorld _world;
        private EcsFilter _filter;
        private EcsFilter _playerFilter;
        private EcsPool<StepInteract> _pool;
        private EcsPool<Transitioning> _poolTransitioning;
        private EcsPool<InputBlocked> _poolBlocked;

        public StepInteractSystem(DayScenario scenario)
        {
            _scenario = scenario;
        }

        public void Init(IEcsSystems systems)
        {
            _world = systems.GetWorld();
            _filter = _world.Filter<StepInteract>().Inc<InteractionRequest>().End();
            _playerFilter = _world.Filter<PlayerTag>().End();
            _pool = _world.GetPool<StepInteract>();
            _poolTransitioning = _world.GetPool<Transitioning>();
            _poolBlocked = _world.GetPool<InputBlocked>();
        }

        public void Run(IEcsSystems systems)
        {
            foreach (int entity in _filter)
            {
                ref var interact = ref _pool.Get(entity);
                _world.RequestThought(interact.ThoughtKey);

                DayEpisode episode = FindEpisode(interact.StepId);
                if (episode != null && interact.EpisodeSpawn != null)
                {
                    StartEpisode(episode, interact.EpisodeSpawn, interact.StepId);
                    continue;
                }

                _world.CompleteScenarioStep(interact.StepId);
            }
        }

        private void StartEpisode(DayEpisode episode, UnityEngine.Transform spawn, string stepId)
        {
            foreach (int player in _playerFilter)
            {
                if (_poolTransitioning.Has(player)) return;

                ref var transitioning = ref _poolTransitioning.Add(player);
                transitioning.Phase = TransitionPhase.FadeOut;
                transitioning.Timer = 0f;
                transitioning.Target = spawn;
                transitioning.Episode = episode;
                transitioning.StepId = stepId;

                if (_poolBlocked.Has(player) == false)
                    _poolBlocked.Add(player);

                return;
            }
        }

        private DayEpisode FindEpisode(string stepId)
        {
            if (_scenario == null || string.IsNullOrEmpty(stepId)) return null;

            foreach (DayStep step in _scenario.Steps)
            {
                if (step != null && step.Id == stepId)
                    return step.Episode;
            }

            return null;
        }
    }
}
