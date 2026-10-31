using UnityEngine;

namespace Game.Scripts.Services.Scenario
{
    public class ScenarioContext
    {
        private AudioSource _audio;

        public void PlaySound(AudioClip clip, float volume)
        {
            if (clip == null) return;

            if (_audio == null)
            {
                var host = new GameObject("ScenarioAudio");
                _audio = host.AddComponent<AudioSource>();
                _audio.playOnAwake = false;
                _audio.spatialBlend = 0f;
            }

            _audio.PlayOneShot(clip, volume);
        }

        public void SetActive(string targetId, bool active)
        {
            if (ScenarioTargetRegistry.TryGet(targetId, out GameObject target))
                target.SetActive(active);
            else
                Debug.LogWarning($"[Scenario] SetActive: target '{targetId}' not registered");
        }

        public void Dispose()
        {
            if (_audio != null) Object.Destroy(_audio.gameObject);
            _audio = null;
        }
    }
}
