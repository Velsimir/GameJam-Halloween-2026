using Game.Scripts.Configs;
using Game.Scripts.ECS.Components.DayObjects;
using Game.Scripts.ECS.Components.Input;
using Game.Scripts.ECS.Components.Interaction;
using Game.Scripts.ECS.Components.Player;
using Game.Scripts.ECS.Components.Transition;
using Game.Scripts.ECS.Extensions;
using Game.Scripts.Services.Audio;
using Game.Scripts.Views;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Scripts.ECS.Systems.DayObjects
{
    public class FadeActionSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly ScreenFadeConfig _config;
        private readonly ScreenFadeView _view;
        private readonly IAudioService _audio;

        private EcsWorld _world;
        private EcsFilter _triggerFilter;
        private EcsFilter _playerFilter;
        private EcsFilter _activeFilter;
        private EcsPool<FadeActionTrigger> _poolTrigger;
        private EcsPool<FadeAction> _poolAction;
        private EcsPool<InputBlocked> _poolBlocked;
        private EcsPool<Transitioning> _poolTransitioning;
        private EcsPool<LyingInBed> _poolLying;

        public FadeActionSystem(ScreenFadeConfig config, ScreenFadeView view, IAudioService audio)
        {
            _config = config;
            _view = view;
            _audio = audio;
        }

        public void Init(IEcsSystems systems)
        {
            _world = systems.GetWorld();
            _triggerFilter = _world.Filter<FadeActionTrigger>().Inc<InteractionRequest>().End();
            _playerFilter = _world.Filter<PlayerTag>().End();
            _activeFilter = _world.Filter<PlayerTag>().Inc<FadeAction>().End();
            _poolTrigger = _world.GetPool<FadeActionTrigger>();
            _poolAction = _world.GetPool<FadeAction>();
            _poolBlocked = _world.GetPool<InputBlocked>();
            _poolTransitioning = _world.GetPool<Transitioning>();
            _poolLying = _world.GetPool<LyingInBed>();
        }

        public void Run(IEcsSystems systems)
        {
            StartActions();
            TickActions();
        }

        private void StartActions()
        {
            foreach (int triggerEntity in _triggerFilter)
            {
                foreach (int player in _playerFilter)
                {
                    if (_poolAction.Has(player) || _poolTransitioning.Has(player) || _poolLying.Has(player))
                        break;

                    ref var trigger = ref _poolTrigger.Get(triggerEntity);

                    if (trigger.RequiredItem != null && _world.HasItem(trigger.RequiredItem) == false)
                    {
                        _world.RequestThought(trigger.MissingItemThoughtKey);
                        break;
                    }

                    ref var action = ref _poolAction.Add(player);
                    action.Phase = TransitionPhase.FadeOut;
                    action.Timer = 0f;
                    action.FadeOut = trigger.FadeOut >= 0f ? trigger.FadeOut : _config.FadeOutTime;
                    action.Hold = trigger.Hold >= 0f ? trigger.Hold : _config.HoldTime;
                    action.FadeIn = trigger.FadeIn >= 0f ? trigger.FadeIn : _config.FadeInTime;
                    action.Sound = trigger.Sound;
                    action.StepId = trigger.StepId;
                    action.ConsumeItem = trigger.ConsumeItem ? trigger.RequiredItem : null;
                    action.ShowOnPeak = trigger.ShowOnPeak;
                    action.HideOnPeak = trigger.HideOnPeak;

                    if (_poolBlocked.Has(player) == false)
                        _poolBlocked.Add(player);

                    break;
                }
            }
        }

        private void TickActions()
        {
            foreach (int player in _activeFilter)
            {
                ref var action = ref _poolAction.Get(player);
                action.Timer += Time.deltaTime;

                switch (action.Phase)
                {
                    case TransitionPhase.FadeOut:
                        _view.Alpha = Progress(action.Timer, action.FadeOut);

                        if (action.Timer >= action.FadeOut)
                        {
                            _view.Alpha = 1f;
                            Peak(ref action);
                            action.Phase = TransitionPhase.Hold;
                            action.Timer = 0f;
                        }
                        break;

                    case TransitionPhase.Hold:
                        if (action.Timer >= action.Hold)
                        {
                            action.Phase = TransitionPhase.FadeIn;
                            action.Timer = 0f;
                        }
                        break;

                    case TransitionPhase.FadeIn:
                        _view.Alpha = 1f - Progress(action.Timer, action.FadeIn);

                        if (action.Timer >= action.FadeIn)
                        {
                            _view.Alpha = 0f;
                            string stepId = action.StepId;
                            _poolBlocked.Del(player);
                            _poolAction.Del(player);
                            _world.CompleteScenarioStep(stepId);
                        }
                        break;
                }
            }
        }

        private void Peak(ref FadeAction action)
        {
            _audio.Play(action.Sound);

            if (action.ConsumeItem != null)
                _world.TryRemoveItem(action.ConsumeItem);

            SetActive(action.ShowOnPeak, true);
            SetActive(action.HideOnPeak, false);
        }

        private static void SetActive(GameObject[] objects, bool active)
        {
            if (objects == null) return;

            foreach (GameObject target in objects)
            {
                if (target != null) target.SetActive(active);
            }
        }

        private static float Progress(float timer, float duration)
        {
            return duration > 0f ? Mathf.Clamp01(timer / duration) : 1f;
        }
    }
}
