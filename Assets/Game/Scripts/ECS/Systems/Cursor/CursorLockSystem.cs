using Game.Scripts.ECS.Components.DayObjects;
using Game.Scripts.ECS.Components.Diary;
using Game.Scripts.ECS.Components.Input;
using Game.Scripts.ECS.Components.Player;
using Game.Scripts.ECS.Components.Scenario;
using Game.Scripts.ECS.Components.Transition;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Scripts.ECS.Systems.Cursor
{
    public class CursorLockSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsFilter _playerFilter;
        private EcsFilter _diaryClosedFilter;
        private EcsPool<InputBlocked> _blockedPool;
        private EcsPool<Transitioning> _transitioningPool;
        private EcsPool<FadeAction> _fadeActionPool;
        private EcsPool<LyingInBed> _lyingPool;
        private EcsPool<DayIntro> _introPool;
        private EcsPool<DayChanging> _dayChangingPool;
        private bool? _applied;

        public void Init(IEcsSystems systems)
        {
            EcsWorld world = systems.GetWorld();
            _playerFilter = world.Filter<PlayerTag>().End();
            _diaryClosedFilter = world.Filter<DiaryClosed>().End();
            _blockedPool = world.GetPool<InputBlocked>();
            _transitioningPool = world.GetPool<Transitioning>();
            _fadeActionPool = world.GetPool<FadeAction>();
            _lyingPool = world.GetPool<LyingInBed>();
            _introPool = world.GetPool<DayIntro>();
            _dayChangingPool = world.GetPool<DayChanging>();
        }

        public void Run(IEcsSystems systems)
        {
            if (_diaryClosedFilter.GetEntitiesCount() > 0)
                _applied = null;

            foreach (int entity in _playerFilter)
            {
                bool scripted = _transitioningPool.Has(entity) || _fadeActionPool.Has(entity) || _lyingPool.Has(entity)
                                || _introPool.Has(entity) || _dayChangingPool.Has(entity);
                bool blocked = _blockedPool.Has(entity) && scripted == false;
                if (_applied == blocked) return;

                _applied = blocked;
                UnityEngine.Cursor.lockState = blocked ? CursorLockMode.None : CursorLockMode.Locked;
                UnityEngine.Cursor.visible = blocked;
                return;
            }
        }
    }
}
