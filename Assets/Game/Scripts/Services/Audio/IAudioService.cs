using Game.Scripts.Configs.Audio;
using UnityEngine;

namespace Game.Scripts.Services.Audio
{
    public interface IAudioService
    {
        SoundHandle Play(SoundDefinition sound, float volume = 1f, float pitch = 1f, float pan = 0f, float fadeIn = -1f);
        SoundHandle PlayAt(SoundDefinition sound, Vector3 position, float volume = 1f, float pitch = 1f, float fadeIn = -1f);
        SoundHandle PlayOn(SoundDefinition sound, Transform target, float volume = 1f, float pitch = 1f, float fadeIn = -1f);
        SoundHandle Play(string id, float volume = 1f, float pitch = 1f, float pan = 0f, float fadeIn = -1f);
        void SetBusVolume(AudioBus bus, float linear);
        float GetBusVolume(AudioBus bus);
        void StopAll(float fadeOut = 0f);
    }
}
