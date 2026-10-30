using Game.Scripts.ECS.Components.Doors;
using Game.Scripts.ECS.Components.Interaction;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Scripts.ECS.Systems.Doors
{
    public class DoorAnimationSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsFilter _filter;
        private EcsPool<Door> _pool;
        private EcsPool<Acting> _poolActing;
        private EcsPool<DoorRattle> _poolRattle;

        public void Init(IEcsSystems systems)
        {
            EcsWorld world = systems.GetWorld();
            _filter = world.Filter<Door>().Inc<Acting>().End();
            _pool = world.GetPool<Door>();
            _poolActing = world.GetPool<Acting>();
            _poolRattle = world.GetPool<DoorRattle>();
        }

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in _filter)
            {
                ref var door = ref _pool.Get(entity);
                float targetAngle = door.IsOpen
                    ? door.OpenAngle
                    : door.CloseAngle;

                door.CurrentAngle = Mathf.MoveTowardsAngle(door.CurrentAngle, targetAngle, door.Speed * Time.deltaTime);

                float rattleOffset = 0f;
                if (_poolRattle.Has(entity))
                {
                    ref var rattle = ref _poolRattle.Get(entity);
                    rattle.Time += Time.deltaTime;
                    float progress = door.RattleDuration > 0f ? rattle.Time / door.RattleDuration : 1f;

                    if (progress >= 1f)
                        _poolRattle.Del(entity);
                    else
                        rattleOffset = Mathf.Sin(progress * door.RattleCycles * Mathf.PI * 2f) * door.RattleAngle * (1f - progress);
                }

                door.Pivot.localRotation = door.InitialRotation * Quaternion.Euler(0, door.CurrentAngle + rattleOffset, 0);

                if (targetAngle == door.CurrentAngle && _poolRattle.Has(entity) == false)
                    _poolActing.Del(entity);
            }
        }
    }
}
