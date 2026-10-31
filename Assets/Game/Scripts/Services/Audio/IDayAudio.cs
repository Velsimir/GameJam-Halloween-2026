using UnityEngine;

namespace Game.Scripts.Services.Audio
{
    public interface IDayAudio
    {
        void Play(string context, AudioClip[] clips, float volume);
        void Stop();
    }
}
