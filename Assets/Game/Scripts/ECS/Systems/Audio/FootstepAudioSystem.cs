using Game.Scripts.Configs.Audio;
using Game.Scripts.ECS.Components.Walk;
using Game.Scripts.Services.Audio;
using Leopotam.EcsLite;

namespace Game.Scripts.ECS.Systems.Audio
{
    public class FootstepAudioSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly AudioConfig _config;
        private readonly IAudioService _audio;

        private EcsFilter _filter;
        private EcsPool<FootstepEvent> _poolEvent;

        public FootstepAudioSystem(AudioConfig config, IAudioService audio)
        {
            _config = config;
            _audio = audio;
        }

        public void Init(IEcsSystems systems)
        {
            EcsWorld world = systems.GetWorld();
            _filter = world.Filter<FootstepEvent>().End();
            _poolEvent = world.GetPool<FootstepEvent>();
        }

        public void Run(IEcsSystems systems)
        {
            foreach (int entity in _filter)
            {
                float pan = _poolEvent.Get(entity).Left ? -_config.FootstepPan : _config.FootstepPan;
                _audio.Play(_config.Footstep, pan: pan);
            }
        }
    }
}
