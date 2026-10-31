using Game.Scripts.ECS.Components.Camera;
using Game.Scripts.ECS.Components.DayObjects;
using Game.Scripts.ECS.Components.Input;
using Game.Scripts.ECS.Components.Interaction;
using Game.Scripts.ECS.Components.Movement;
using Game.Scripts.ECS.Components.Player;
using Game.Scripts.ECS.Extensions;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Scripts.ECS.Systems.DayObjects
{
    public class BedSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld _world;
        private EcsFilter _bedFilter;
        private EcsFilter _wakeRequestFilter;
        private EcsFilter _playerFilter;
        private EcsFilter _lyingFilter;
        private EcsPool<Bed> _poolBed;
        private EcsPool<LyingInBed> _poolLying;
        private EcsPool<InputBlocked> _poolBlocked;
        private EcsPool<InteractWhenBlocked> _poolInteractWhenBlocked;
        private EcsPool<CutsceneControlled> _poolCutscene;
        private EcsPool<PlayerRefs> _poolRefs;
        private EcsPool<LookRotation> _poolLook;
        private EcsPool<VerticalMovement> _poolVertical;
        private EcsPool<Focused> _poolFocused;

        public void Init(IEcsSystems systems)
        {
            _world = systems.GetWorld();
            _bedFilter = _world.Filter<Bed>().End();
            _wakeRequestFilter = _world.Filter<BedWake>().Inc<InteractionRequest>().End();
            _playerFilter = _world.Filter<PlayerTag>().Inc<PlayerRefs>().Inc<LookRotation>().End();
            _lyingFilter = _world.Filter<PlayerTag>().Inc<LyingInBed>().End();
            _poolBed = _world.GetPool<Bed>();
            _poolLying = _world.GetPool<LyingInBed>();
            _poolBlocked = _world.GetPool<InputBlocked>();
            _poolInteractWhenBlocked = _world.GetPool<InteractWhenBlocked>();
            _poolCutscene = _world.GetPool<CutsceneControlled>();
            _poolRefs = _world.GetPool<PlayerRefs>();
            _poolLook = _world.GetPool<LookRotation>();
            _poolVertical = _world.GetPool<VerticalMovement>();
            _poolFocused = _world.GetPool<Focused>();
        }

        public void Run(IEcsSystems systems)
        {
            StartLying();
            RequestGetUp();
            TickGetUp();
        }

        private void StartLying()
        {
            foreach (int bedEntity in _bedFilter)
            {
                ref var bed = ref _poolBed.Get(bedEntity);
                if (bed.PendingLie == false) continue;

                foreach (int player in _playerFilter)
                {
                    bed.PendingLie = false;

                    if (bed.Spawn != null)
                        Teleport(player, bed.Spawn);

                    ref var lying = ref _poolLying.Add(player);
                    lying.Look = bed.Look;
                    lying.GetUpDuration = bed.GetUpDuration;
                    lying.WakeStepId = bed.WakeStepId;
                    lying.Progress = 0f;
                    lying.GettingUp = false;

                    if (_poolBlocked.Has(player) == false) _poolBlocked.Add(player);
                    if (_poolInteractWhenBlocked.Has(player) == false) _poolInteractWhenBlocked.Add(player);
                    if (_poolCutscene.Has(player) == false) _poolCutscene.Add(player);

                    _world.RequestThought(bed.LieThoughtKey);
                    break;
                }
            }
        }

        private void RequestGetUp()
        {
            foreach (int wake in _wakeRequestFilter)
            {
                foreach (int player in _lyingFilter)
                {
                    ref var lying = ref _poolLying.Get(player);
                    if (lying.GettingUp) break;

                    lying.GettingUp = true;
                    if (_poolFocused.Has(wake)) _poolFocused.Del(wake);
                    break;
                }
            }
        }

        private void TickGetUp()
        {
            foreach (int player in _lyingFilter)
            {
                ref var lying = ref _poolLying.Get(player);
                if (lying.GettingUp == false) continue;

                lying.Progress += lying.GetUpDuration > 0f ? Time.deltaTime / lying.GetUpDuration : 1f;
                if (lying.Progress < 1f) continue;

                string stepId = lying.WakeStepId;
                _poolLying.Del(player);
                _poolBlocked.Del(player);
                _poolInteractWhenBlocked.Del(player);
                _poolCutscene.Del(player);
                _world.CompleteScenarioStep(stepId);
            }
        }

        private void Teleport(int player, Transform target)
        {
            ref var refs = ref _poolRefs.Get(player);
            float yaw = target.eulerAngles.y;

            refs.CharacterController.enabled = false;
            refs.CharacterController.transform.SetPositionAndRotation(target.position, Quaternion.Euler(0f, yaw, 0f));
            refs.CharacterController.enabled = true;

            ref var look = ref _poolLook.Get(player);
            look.Yaw = yaw;
            look.Pitch = 0f;
            refs.HeadTransform.localEulerAngles = Vector3.zero;

            if (_poolVertical.Has(player))
                _poolVertical.Get(player).Velocity = Vector3.zero;
        }
    }
}
