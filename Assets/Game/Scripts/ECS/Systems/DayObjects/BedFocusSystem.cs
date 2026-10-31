using Game.Scripts.ECS.Components.DayObjects;
using Game.Scripts.ECS.Components.Interaction;
using Game.Scripts.ECS.Components.Player;
using Leopotam.EcsLite;

namespace Game.Scripts.ECS.Systems.DayObjects
{
    public class BedFocusSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsFilter _lyingFilter;
        private EcsFilter _focusedFilter;
        private EcsFilter _wakeFilter;
        private EcsPool<LyingInBed> _poolLying;
        private EcsPool<Focused> _poolFocused;
        private EcsPool<BedWake> _poolWake;

        public void Init(IEcsSystems systems)
        {
            EcsWorld world = systems.GetWorld();
            _lyingFilter = world.Filter<PlayerTag>().Inc<LyingInBed>().End();
            _focusedFilter = world.Filter<Focused>().End();
            _wakeFilter = world.Filter<BedWake>().Inc<Interactable>().End();
            _poolLying = world.GetPool<LyingInBed>();
            _poolFocused = world.GetPool<Focused>();
            _poolWake = world.GetPool<BedWake>();
        }

        public void Run(IEcsSystems systems)
        {
            foreach (int player in _lyingFilter)
            {
                if (_poolLying.Get(player).GettingUp) return;

                foreach (int entity in _focusedFilter)
                {
                    if (_poolWake.Has(entity) == false)
                        _poolFocused.Del(entity);
                }

                foreach (int wake in _wakeFilter)
                {
                    if (_poolFocused.Has(wake) == false)
                        _poolFocused.Add(wake);
                }

                return;
            }
        }
    }
}
