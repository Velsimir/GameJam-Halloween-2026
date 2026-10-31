using Game.Scripts.Configs.Audio;
using Game.Scripts.Services.Audio;
using Leopotam.EcsLite;

namespace Game.Scripts.ECS.Systems.Audio
{
    public class BackgroundAudioSystem : IEcsInitSystem, IEcsDestroySystem
    {
        private readonly AudioConfig _config;
        private readonly IAudioService _audio;

        private SoundHandle _handle;

        public BackgroundAudioSystem(AudioConfig config, IAudioService audio)
        {
            _config = config;
            _audio = audio;
        }

        public void Init(IEcsSystems systems)
        {
            _handle = _audio.Play(_config.Background);
        }

        public void Destroy(IEcsSystems systems)
        {
            _handle.Stop();
        }
    }
}
