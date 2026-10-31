using Game.Scripts.Configs;
using Game.Scripts.Configs.Scenario;
using Game.Scripts.Services.Audio;
using Game.Scripts.ECS.Components.Input;
using Game.Scripts.ECS.Components.Movement;
using Game.Scripts.ECS.Components.Player;
using Game.Scripts.ECS.Components.Scenario;
using Game.Scripts.ECS.Components.Transition;
using Game.Scripts.ECS.Extensions;
using Game.Scripts.Views;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Scripts.ECS.Systems.Transition
{
    public class TransitionSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly ScreenFadeConfig _config;
        private readonly DayFlowConfig _flowConfig;
        private readonly ScreenFadeView _view;
        private readonly IDayAudio _audio;

        private EcsWorld _world;
        private EcsFilter _filter;
        private EcsPool<Transitioning> _poolTransitioning;
        private EcsPool<PlayerRefs> _poolRefs;
        private EcsPool<LookRotation> _poolLook;
        private EcsPool<VerticalMovement> _poolVertical;
        private EcsPool<InputBlocked> _poolBlocked;

        public TransitionSystem(ScreenFadeConfig config, DayFlowConfig flowConfig, ScreenFadeView view, IDayAudio audio)
        {
            _config = config;
            _flowConfig = flowConfig;
            _view = view;
            _audio = audio;
        }

        public void Init(IEcsSystems systems)
        {
            _world = systems.GetWorld();

            _filter = _world.Filter<PlayerTag>().Inc<Transitioning>().Inc<PlayerRefs>().End();
            _poolTransitioning = _world.GetPool<Transitioning>();
            _poolRefs = _world.GetPool<PlayerRefs>();
            _poolLook = _world.GetPool<LookRotation>();
            _poolVertical = _world.GetPool<VerticalMovement>();
            _poolBlocked = _world.GetPool<InputBlocked>();
        }

        public void Run(IEcsSystems systems)
        {
            foreach (int entity in _filter)
            {
                ref var transitioning = ref _poolTransitioning.Get(entity);
                transitioning.Timer += Time.deltaTime;

                switch (transitioning.Phase)
                {
                    case TransitionPhase.FadeOut:
                        _view.Alpha = Progress(transitioning.Timer, _config.FadeOutTime);

                        if (transitioning.Timer >= _config.FadeOutTime)
                        {
                            _view.Alpha = 1f;
                            transitioning.Timer = 0f;

                            if (transitioning.Episode != null)
                            {
                                BeginEpisode(ref transitioning);
                                transitioning.Phase = TransitionPhase.Episode;
                                break;
                            }

                            Teleport(entity, ref transitioning);
                            transitioning.Phase = TransitionPhase.Hold;
                        }
                        break;

                    case TransitionPhase.Episode:
                        if (transitioning.Timer >= transitioning.Episode.Seconds)
                        {
                            EndEpisode(ref transitioning);
                            Teleport(entity, ref transitioning);
                            transitioning.Phase = TransitionPhase.Hold;
                            transitioning.Timer = 0f;
                        }
                        break;

                    case TransitionPhase.Hold:
                        if (transitioning.Timer >= _config.HoldTime)
                        {
                            transitioning.Phase = TransitionPhase.FadeIn;
                            transitioning.Timer = 0f;
                        }
                        break;

                    case TransitionPhase.FadeIn:
                        _view.Alpha = 1f - Progress(transitioning.Timer, _config.FadeInTime);

                        if (transitioning.Timer >= _config.FadeInTime)
                        {
                            _view.Alpha = 0f;
                            string stepId = transitioning.StepId;
                            _poolBlocked.Del(entity);
                            _poolTransitioning.Del(entity);

                            if (string.IsNullOrEmpty(stepId) == false)
                                _world.GetPool<PendingStepComplete>().Add(_world.NewEntity()).StepId = stepId;
                        }
                        break;
                }
            }
        }

        private void BeginEpisode(ref Transitioning transitioning)
        {
            DayEpisode episode = transitioning.Episode;
            _view.SetCaption(episode.TimeLabel);
            _audio.Play(episode.DebugName ?? transitioning.StepId, episode.Clips, _flowConfig.EpisodeVolume);
        }

        private void EndEpisode(ref Transitioning transitioning)
        {
            _audio.Stop();
            _view.SetCaption(null);
            _world.TryAddItem(transitioning.Episode.GiveItem);
        }

        private void Teleport(int entity, ref Transitioning transitioning)
        {
            Transform target = transitioning.Target;
            ref var refs = ref _poolRefs.Get(entity);
            CharacterController controller = refs.CharacterController;

            float yaw = target.eulerAngles.y;

            controller.enabled = false;
            controller.transform.SetPositionAndRotation(target.position, Quaternion.Euler(0f, yaw, 0f));
            controller.enabled = true;

            if (_poolLook.Has(entity))
            {
                ref var look = ref _poolLook.Get(entity);
                look.Yaw = yaw;
                look.Pitch = 0f;
            }

            refs.HeadTransform.localEulerAngles = Vector3.zero;

            if (_poolVertical.Has(entity))
                _poolVertical.Get(entity).Velocity = Vector3.zero;

            _world.RequestThought(transitioning.EnterThoughtKey);
        }

        private static float Progress(float timer, float duration)
        {
            return duration > 0f ? Mathf.Clamp01(timer / duration) : 1f;
        }
    }
}
