using Game.Scripts.Configs;
using Game.Scripts.Configs.Audio;
using Game.Scripts.ECS.Components.Fear;
using Game.Scripts.ECS.Components.Player;
using Game.Scripts.Services.Audio;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Scripts.ECS.Systems.Audio
{
    public class FearBreathSystem : IEcsInitSystem, IEcsRunSystem, IEcsDestroySystem
    {
        private readonly AudioConfig _config;
        private readonly FearConfig _fearConfig;
        private readonly IAudioService _audio;

        private EcsFilter _filter;
        private EcsPool<FearFactor> _poolFear;
        private SoundHandle _handle;
        private float _level;

        public FearBreathSystem(AudioConfig config, FearConfig fearConfig, IAudioService audio)
        {
            _config = config;
            _fearConfig = fearConfig;
            _audio = audio;
        }

        public void Init(IEcsSystems systems)
        {
            EcsWorld world = systems.GetWorld();
            _filter = world.Filter<PlayerTag>().Inc<FearFactor>().End();
            _poolFear = world.GetPool<FearFactor>();
        }

        public void Run(IEcsSystems systems)
        {
            foreach (int entity in _filter)
            {
                float fear = Mathf.Clamp01(_poolFear.Get(entity).Value / Mathf.Max(0.0001f, _fearConfig.MaxValue));
                Tick(fear);
                return;
            }
        }

        public void Destroy(IEcsSystems systems)
        {
            _handle.Stop();
        }

        private void Tick(float fear)
        {
            if (_handle.IsValid == false)
            {
                if (fear < _config.BreathStartFear) return;

                _level = fear;
                _handle = _audio.Play(_config.Breath, pitch: Pitch(_level));
                _handle.SetVolume(Volume(_level));
                return;
            }

            if (fear < _config.BreathStopFear)
            {
                _handle.Stop();
                return;
            }

            _level = Mathf.MoveTowards(_level, fear, _config.BreathResponse * Time.deltaTime);
            _handle.SetVolume(Volume(_level));
            _handle.SetPitch(Pitch(_level));
        }

        private float Volume(float fear)
        {
            return Mathf.Lerp(_config.BreathMinVolume, _config.BreathMaxVolume, fear);
        }

        private float Pitch(float fear)
        {
            return Mathf.Lerp(_config.BreathMinPitch, _config.BreathMaxPitch, fear);
        }
    }
}
