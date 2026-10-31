using UnityEngine;

namespace Game.Scripts.Services.Audio
{
    public class DayAudioService : IDayAudio
    {
        private readonly AudioSource _source;

        public DayAudioService()
        {
            var host = new GameObject("DayAudio");
            Object.DontDestroyOnLoad(host);
            _source = host.AddComponent<AudioSource>();
            _source.playOnAwake = false;
            _source.spatialBlend = 0f;
        }

        public void Play(string context, AudioClip[] clips, float volume)
        {
            int count = clips == null ? 0 : clips.Length;
            Debug.Log($"[DayAudio] play '{context}': {count} clip(s), volume {volume:0.00}");

            if (clips == null) return;

            foreach (AudioClip clip in clips)
            {
                if (clip == null) continue;

                Debug.Log($"[DayAudio] clip '{clip.name}'");
                _source.PlayOneShot(clip, volume);
            }
        }

        public void Stop()
        {
            _source.Stop();
        }
    }
}
