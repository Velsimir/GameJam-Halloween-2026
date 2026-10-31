using Game.Scripts.Configs;
using Game.Scripts.ECS.Components.Input;
using Game.Scripts.ECS.Components.DayObjects;
using Game.Scripts.ECS.Components.Player;
using Game.Scripts.ECS.Components.Scenario;
using Game.Scripts.Services.Audio;
using Game.Scripts.Views;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Scripts.ECS.Systems.Scenario
{
    public class DayIntroSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly DayFlowConfig _config;
        private readonly DayIntroView _view;
        private readonly IAudioService _audio;
        private readonly int _day;
        private readonly bool _enabled;

        private EcsFilter _playerFilter;
        private EcsPool<DayIntro> _poolIntro;
        private EcsPool<InputBlocked> _poolBlocked;
        private EcsPool<LyingInBed> _poolLying;
        private bool _started;
        private bool _finished;

        public DayIntroSystem(DayFlowConfig config, DayIntroView view, IAudioService audio, int day, bool enabled)
        {
            _config = config;
            _view = view;
            _audio = audio;
            _day = day;
            _enabled = enabled;
        }

        public void Init(IEcsSystems systems)
        {
            EcsWorld world = systems.GetWorld();
            _playerFilter = world.Filter<PlayerTag>().End();
            _poolIntro = world.GetPool<DayIntro>();
            _poolBlocked = world.GetPool<InputBlocked>();
            _poolLying = world.GetPool<LyingInBed>();

            if (_enabled == false)
            {
                _view.Hide();
                _finished = true;
                return;
            }

            _view.Show(_config.TitleKey, _day);
        }

        public void Run(IEcsSystems systems)
        {
            if (_finished) return;

            foreach (int player in _playerFilter)
            {
                if (_started == false)
                {
                    _started = true;
                    ref var intro = ref _poolIntro.Add(player);
                    intro.OwnsBlock = _poolBlocked.Has(player) == false;
                    if (intro.OwnsBlock) _poolBlocked.Add(player);
                    _audio.Play(_config.IntroSound);
                }

                ref var state = ref _poolIntro.Get(player);
                state.Timer += Time.deltaTime;

                float fadeStart = _config.IntroHold;
                if (state.Timer < fadeStart)
                {
                    _view.Alpha = 1f;
                    return;
                }

                float fade = _config.IntroFadeOut;
                float progress = fade > 0f ? Mathf.Clamp01((state.Timer - fadeStart) / fade) : 1f;
                _view.Alpha = 1f - progress;

                if (progress < 1f) return;

                if (state.OwnsBlock && _poolLying.Has(player) == false)
                    _poolBlocked.Del(player);

                _poolIntro.Del(player);
                _finished = true;
                return;
            }
        }
    }
}
