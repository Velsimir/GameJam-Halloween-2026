using System.Collections.Generic;
using Game.Scripts.Configs.Audio;
using Game.Scripts.Services.Audio;
using UnityEngine;

namespace Game.Scripts.Services.Scenario
{
    public class ScenarioContext
    {
        private readonly IAudioService _audio;
        private readonly List<SoundHandle> _loops = new List<SoundHandle>();

        public ScenarioContext(IAudioService audio)
        {
            _audio = audio;
        }

        public void PlaySound(SoundDefinition sound)
        {
            if (sound == null) return;

            SoundHandle handle = _audio.Play(sound);
            if (sound.Loop) _loops.Add(handle);
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
            foreach (SoundHandle handle in _loops)
                handle.Stop();

            _loops.Clear();
        }
    }
}
