using Game.Scripts.ECS.Components.Audio;
using Game.Scripts.ECS.Components.Player;
using Game.Scripts.ECS.Components.Walk;
using Leopotam.EcsLite;

namespace Game.Scripts.ECS.Systems.Audio
{
    public class FootstepDetectSystem : IEcsInitSystem, IEcsRunSystem
    {
        private const float HalfCycle = 0.5f;

        private EcsWorld _world;
        private EcsFilter _filter;
        private EcsPool<WalkCycle> _poolWalk;
        private EcsPool<FootstepState> _poolState;
        private EcsPool<FootstepEvent> _poolEvent;

        public void Init(IEcsSystems systems)
        {
            _world = systems.GetWorld();
            _filter = _world.Filter<PlayerTag>().Inc<WalkCycle>().End();
            _poolWalk = _world.GetPool<WalkCycle>();
            _poolState = _world.GetPool<FootstepState>();
            _poolEvent = _world.GetPool<FootstepEvent>();
        }

        public void Run(IEcsSystems systems)
        {
            foreach (int entity in _filter)
            {
                float phase = _poolWalk.Get(entity).Phase;

                if (_poolState.Has(entity) == false)
                {
                    _poolState.Add(entity).PrevPhase = phase;
                    continue;
                }

                ref var state = ref _poolState.Get(entity);
                bool wrapped = phase < state.PrevPhase;
                bool halfCrossed = state.PrevPhase < HalfCycle && phase >= HalfCycle;
                state.PrevPhase = phase;

                if (wrapped == false && halfCrossed == false) continue;

                state.Left = !state.Left;
                _poolEvent.Add(_world.NewEntity()).Left = state.Left;
            }
        }
    }
}
