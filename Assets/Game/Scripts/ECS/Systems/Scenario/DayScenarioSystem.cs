using System.Collections.Generic;
using Game.Scripts.Configs;
using Game.Scripts.Configs.Scenario;
using Game.Scripts.Configs.Scenario.Actions;
using Game.Scripts.ECS.Components.Doors;
using Game.Scripts.ECS.Components.Input;
using Game.Scripts.ECS.Components.Interaction;
using Game.Scripts.ECS.Components.Items;
using Game.Scripts.ECS.Components.Player;
using Game.Scripts.ECS.Components.Scenario;
using Game.Scripts.ECS.Components.Transition;
using Game.Scripts.ECS.Extensions;
using Game.Scripts.Services.Scenario;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Scripts.ECS.Systems.Scenario
{
    public class DayScenarioSystem : IEcsInitSystem, IEcsRunSystem, IEcsDestroySystem
    {
        private readonly DayScenario _scenario;
        private readonly ScenarioContext _context = new ScenarioContext();

        private EcsWorld _world;
        private EcsFilter _progressFilter;
        private EcsFilter _targetFilter;
        private EcsFilter _lockedTargetFilter;
        private EcsFilter _activeTargetFilter;
        private EcsFilter _interactedFilter;
        private EcsFilter _zoneEnteredFilter;
        private EcsFilter _externalFilter;
        private EcsFilter _blockedPlayerFilter;
        private EcsFilter _transitioningPlayerFilter;
        private EcsFilter _inventoryFilter;
        private EcsPool<ScenarioProgress> _poolProgress;
        private EcsPool<ScenarioTarget> _poolTarget;
        private EcsPool<ScenarioActive> _poolActive;
        private EcsPool<ScenarioLocked> _poolLocked;
        private EcsPool<ScenarioExternalComplete> _poolExternal;
        private EcsPool<DayFinished> _poolDayFinished;
        private EcsPool<Locked> _poolDoorLocked;
        private EcsPool<TransitionDoor> _poolTransitionDoor;
        private EcsPool<Inventory> _poolInventory;

        public DayScenarioSystem(DayScenario scenario)
        {
            _scenario = scenario;
        }

        public void Init(IEcsSystems systems)
        {
            _world = systems.GetWorld();
            _progressFilter = _world.Filter<ScenarioProgress>().End();
            _targetFilter = _world.Filter<ScenarioTarget>().End();
            _lockedTargetFilter = _world.Filter<ScenarioTarget>().Inc<ScenarioLocked>().End();
            _activeTargetFilter = _world.Filter<ScenarioTarget>().Inc<ScenarioActive>().End();
            _interactedFilter = _world.Filter<ScenarioTarget>().Inc<ScenarioActive>().Inc<InteractionRequest>().End();
            _zoneEnteredFilter = _world.Filter<ScenarioTarget>().Inc<ScenarioActive>().Inc<ScenarioZoneEntered>().End();
            _externalFilter = _world.Filter<ScenarioExternalComplete>().End();
            _blockedPlayerFilter = _world.Filter<PlayerTag>().Inc<InputBlocked>().End();
            _transitioningPlayerFilter = _world.Filter<PlayerTag>().Inc<Transitioning>().End();
            _inventoryFilter = _world.Filter<PlayerTag>().Inc<Inventory>().End();
            _poolProgress = _world.GetPool<ScenarioProgress>();
            _poolTarget = _world.GetPool<ScenarioTarget>();
            _poolActive = _world.GetPool<ScenarioActive>();
            _poolLocked = _world.GetPool<ScenarioLocked>();
            _poolExternal = _world.GetPool<ScenarioExternalComplete>();
            _poolDayFinished = _world.GetPool<DayFinished>();
            _poolDoorLocked = _world.GetPool<Locked>();
            _poolTransitionDoor = _world.GetPool<TransitionDoor>();
            _poolInventory = _world.GetPool<Inventory>();

            if (_scenario == null) return;

            foreach (string error in _scenario.ValidationErrors())
                Debug.LogError($"[Scenario] {error}", _scenario);

            if (_scenario.Steps.Count > 0)
                _poolProgress.Add(_world.NewEntity());
        }

        public void Run(IEcsSystems systems)
        {
            if (TryGetProgress(out int progressEntity) == false)
            {
                UnlockAllTargets();
                return;
            }

            ref ScenarioProgress progress = ref _poolProgress.Get(progressEntity);
            if (progress.Finished)
            {
                UnlockAllTargets();
                return;
            }

            if (progress.Started == false)
            {
                progress.Started = true;
                StartStep(ref progress);
            }

            DayStep step = _scenario.Steps[progress.StepIndex];
            ActivateTargets(step.TargetId);
            ReleaseUnusedTargets(progress.StepIndex);

            if (IsCompleted(step))
            {
                CompleteStep(ref progress, step);
                return;
            }

            TickReminder(ref progress, step);
        }

        public void Destroy(IEcsSystems systems)
        {
            _context.Dispose();
        }

        private bool TryGetProgress(out int progressEntity)
        {
            foreach (int entity in _progressFilter)
            {
                progressEntity = entity;
                return true;
            }

            progressEntity = -1;
            return false;
        }

        private void StartStep(ref ScenarioProgress progress)
        {
            DayStep step = _scenario.Steps[progress.StepIndex];
            progress.IdleTime = 0f;

            Execute(step.StartActions);

            if (string.IsNullOrEmpty(step.ObjectiveKey))
                _world.RequestThought(step.StartThought);
            else
                _world.ShowObjective(step.ObjectiveKey, step.StartThought);

            ActivateTargets(step.TargetId);
        }

        private void CompleteStep(ref ScenarioProgress progress, DayStep step)
        {
            _world.CompleteObjective(step.ObjectiveKey);
            Execute(step.CompleteActions);

            progress.StepIndex++;
            DeactivateTargets(progress.StepIndex);

            if (progress.StepIndex < _scenario.Steps.Count)
            {
                StartStep(ref progress);
                return;
            }

            progress.Finished = true;
            _poolDayFinished.Add(_world.NewEntity()).Day = _scenario.Day;
        }

        private bool IsCompleted(DayStep step)
        {
            foreach (int entity in _externalFilter)
            {
                if (_poolExternal.Get(entity).StepId == (step.Id ?? string.Empty))
                    return true;
            }

            switch (step.Condition)
            {
                case ScenarioCondition.Interact:
                    foreach (int entity in _interactedFilter)
                    {
                        if (IsTarget(entity, step.TargetId) && WillInteractionSucceed(entity))
                            return true;
                    }

                    return false;
                case ScenarioCondition.EnterZone:
                    foreach (int entity in _zoneEnteredFilter)
                    {
                        if (IsTarget(entity, step.TargetId))
                            return true;
                    }

                    return false;
                case ScenarioCondition.HasItem:
                    return PlayerHasItem(step.Item);
                default:
                    return false;
            }
        }

        private bool WillInteractionSucceed(int entity)
        {
            if (_poolDoorLocked.Has(entity) && _world.CanUnlock(_poolDoorLocked.Get(entity)) == false)
                return false;

            if (_poolTransitionDoor.Has(entity))
            {
                if (_poolTransitionDoor.Get(entity).Target == null) return false;
                if (_transitioningPlayerFilter.GetEntitiesCount() > 0) return false;
            }

            return true;
        }

        private bool PlayerHasItem(ItemDefinition item)
        {
            if (item == null) return false;

            foreach (int player in _inventoryFilter)
            {
                if (_poolInventory.Get(player).Items.Contains(item))
                    return true;
            }

            return false;
        }

        private void TickReminder(ref ScenarioProgress progress, DayStep step)
        {
            if (step.IdleSeconds <= 0f) return;
            if (_blockedPlayerFilter.GetEntitiesCount() > 0) return;

            progress.IdleTime += Time.deltaTime;
            if (progress.IdleTime < step.IdleSeconds) return;

            progress.IdleTime = 0f;
            _world.RequestThought(step.ReminderThought);
        }

        private void ActivateTargets(string targetId)
        {
            if (string.IsNullOrEmpty(targetId)) return;

            foreach (int entity in _targetFilter)
            {
                if (IsTarget(entity, targetId) == false) continue;

                if (_poolLocked.Has(entity)) _poolLocked.Del(entity);
                if (_poolActive.Has(entity) == false) _poolActive.Add(entity);
            }
        }

        private void DeactivateTargets(int nextStepIndex)
        {
            foreach (int entity in _activeTargetFilter)
            {
                ref ScenarioTarget target = ref _poolTarget.Get(entity);
                bool relock = target.Gated && IsUsedFrom(target.Id, nextStepIndex);

                _poolActive.Del(entity);
                if (relock && _poolLocked.Has(entity) == false)
                    _poolLocked.Add(entity);
            }
        }

        private void ReleaseUnusedTargets(int fromStepIndex)
        {
            foreach (int entity in _lockedTargetFilter)
            {
                if (IsUsedFrom(_poolTarget.Get(entity).Id, fromStepIndex) == false)
                    _poolLocked.Del(entity);
            }
        }

        private void UnlockAllTargets()
        {
            foreach (int entity in _lockedTargetFilter)
                _poolLocked.Del(entity);
        }

        private bool IsUsedFrom(string targetId, int fromStepIndex)
        {
            if (string.IsNullOrEmpty(targetId) || _scenario == null) return false;

            IReadOnlyList<DayStep> steps = _scenario.Steps;
            for (int i = fromStepIndex; i < steps.Count; i++)
            {
                if (steps[i] != null && steps[i].TargetId == targetId)
                    return true;
            }

            return false;
        }

        private bool IsTarget(int entity, string targetId)
        {
            return string.IsNullOrEmpty(targetId) == false && _poolTarget.Get(entity).Id == targetId;
        }

        private void Execute(IReadOnlyList<ScenarioAction> actions)
        {
            if (actions == null) return;

            foreach (ScenarioAction action in actions)
                action?.Execute(_world, _context);
        }
    }
}
