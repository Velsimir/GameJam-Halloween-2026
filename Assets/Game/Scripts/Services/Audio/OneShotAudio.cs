using UnityEngine;

namespace Game.Scripts.Services.Audio
{
    public static class OneShotAudio
    {
        private static AudioSource _source;

        public static void Play(AudioClip clip, float volume)
        {
            if (clip == null) return;

            if (_source == null)
            {
                var host = new GameObject("OneShotAudio");
                _source = host.AddComponent<AudioSource>();
                _source.playOnAwake = false;
                _source.spatialBlend = 0f;
            }

            _source.PlayOneShot(clip, volume);
        }
    }
}
