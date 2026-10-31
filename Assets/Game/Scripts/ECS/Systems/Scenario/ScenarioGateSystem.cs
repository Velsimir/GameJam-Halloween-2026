using Game.Scripts.Configs.Scenario;
using Game.Scripts.ECS.Components.Interaction;
using Game.Scripts.ECS.Components.Scenario;
using Game.Scripts.ECS.Extensions;
using Leopotam.EcsLite;
using UnityEngine;
using Velsi.Typecraft.Dialogue;

namespace Game.Scripts.ECS.Systems.Scenario
{
    public class ScenarioGateSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly ScenarioConfig _config;
        private readonly DialoguePlayer _thoughtPlayer;

        private EcsWorld _world;
        private EcsFilter _filter;
        private EcsPool<InteractionRequest> _poolRequest;
        private float _lastNotNowTime = float.NegativeInfinity;

        public ScenarioGateSystem(ScenarioConfig config, DialoguePlayer thoughtPlayer)
        {
            _config = config;
            _thoughtPlayer = thoughtPlayer;
        }

        public void Init(IEcsSystems systems)
        {
            _world = systems.GetWorld();
            _filter = _world.Filter<ScenarioTarget>().Inc<InteractionRequest>().Inc<ScenarioLocked>().End();
            _poolRequest = _world.GetPool<InteractionRequest>();
        }

        public void Run(IEcsSystems systems)
        {
            bool blocked = false;
            foreach (int entity in _filter)
            {
                _poolRequest.Del(entity);
                blocked = true;
            }

            if (blocked == false || _config == null) return;
            if (_thoughtPlayer != null && _thoughtPlayer.IsPlaying) return;
            if (Time.time - _lastNotNowTime < _config.NotNowCooldown) return;

            _lastNotNowTime = Time.time;
            _world.RequestThought(_config.NotNowThoughtKey);
        }
    }
}
