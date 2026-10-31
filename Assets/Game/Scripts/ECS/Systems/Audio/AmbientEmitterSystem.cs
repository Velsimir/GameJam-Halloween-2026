using Game.Scripts.Configs;
using Game.Scripts.Configs.Audio;
using Game.Scripts.ECS.Components.Audio;
using Game.Scripts.ECS.Components.Fear;
using Game.Scripts.ECS.Components.Player;
using Game.Scripts.Services.Audio;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Scripts.ECS.Systems.Audio
{
    public class AmbientEmitterSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly FearConfig _fearConfig;
        private readonly IAudioService _audio;
        private readonly int _day;

        private EcsFilter _emitterFilter;
        private EcsFilter _playerFilter;
        private EcsPool<AmbientEmitter> _poolEmitter;
        private EcsPool<PlayerRefs> _poolRefs;
        private EcsPool<FearFactor> _poolFear;

        public AmbientEmitterSystem(FearConfig fearConfig, IAudioService audio, int day)
        {
            _fearConfig = fearConfig;
            _audio = audio;
            _day = day;
        }

        public void Init(IEcsSystems systems)
        {
            EcsWorld world = systems.GetWorld();
            _emitterFilter = world.Filter<AmbientEmitter>().End();
            _playerFilter = world.Filter<PlayerTag>().Inc<PlayerRefs>().End();
            _poolEmitter = world.GetPool<AmbientEmitter>();
            _poolRefs = world.GetPool<PlayerRefs>();
            _poolFear = world.GetPool<FearFactor>();
        }

        public void Run(IEcsSystems systems)
        {
            foreach (int player in _playerFilter)
            {
                Transform head = _poolRefs.Get(player).HeadTransform;
                float fear = _poolFear.Has(player)
                    ? Mathf.Clamp01(_poolFear.Get(player).Value / Mathf.Max(0.0001f, _fearConfig.MaxValue))
                    : 0f;

                foreach (int entity in _emitterFilter)
                    Tick(ref _poolEmitter.Get(entity), head, fear);

                return;
            }
        }

        private void Tick(ref AmbientEmitter emitter, Transform head, float fear)
        {
            AmbientEmitterProfile profile = emitter.Profile;
            if (emitter.Inside == false || _day < profile.MinDay)
            {
                emitter.Timer = -1f;
                return;
            }

            if (emitter.Timer < 0f)
                emitter.Timer = RollInterval(profile, fear);

            emitter.Timer -= Time.deltaTime;
            if (emitter.Timer > 0f) return;

            emitter.Timer = RollInterval(profile, fear);
            Emit(profile, emitter.Point, head);
        }

        private void Emit(AmbientEmitterProfile profile, Transform point, Transform head)
        {
            if (profile.Mode == AmbientEmitterMode.AtPoint)
            {
                _audio.PlayAt(profile.Sound, point.position);
                return;
            }

            Vector3 forward = Vector3.ProjectOnPlane(head.forward, Vector3.up);
            if (forward.sqrMagnitude < 0.0001f) forward = Vector3.forward;

            float angle = Random.Range(profile.AngleMin, profile.AngleMax) * (Random.value < 0.5f ? -1f : 1f);
            Vector3 direction = Quaternion.Euler(0f, angle, 0f) * forward.normalized;
            Vector3 position = head.position + direction * Random.Range(profile.DistanceMin, profile.DistanceMax);
            _audio.PlayAt(profile.Sound, position);
        }

        private static float RollInterval(AmbientEmitterProfile profile, float fear)
        {
            float interval = Random.Range(profile.IntervalMin, profile.IntervalMax);
            return interval * Mathf.Lerp(1f, profile.FearIntervalScale, fear);
        }
    }
}
