using System.Collections.Generic;
using UnityEngine;

namespace Game.Scripts.Configs.Audio
{
    [CreateAssetMenu(fileName = "SoundLibrary", menuName = "Configs/Audio/SoundLibrary")]
    public class SoundLibrary : ScriptableObject
    {
        [SerializeField] private SoundDefinition[] _sounds;

        private Dictionary<string, SoundDefinition> _byId;

        public IReadOnlyList<SoundDefinition> Sounds => _sounds;

        public bool TryGet(string id, out SoundDefinition sound)
        {
            if (_byId == null) Build();
            return _byId.TryGetValue(id ?? string.Empty, out sound);
        }

        public SoundDefinition Find(string id)
        {
            if (TryGet(id, out SoundDefinition sound)) return sound;

            Debug.LogWarning($"[Audio] sound '{id}' not found in library");
            return null;
        }

        private void OnValidate()
        {
            _byId = null;
        }

        private void Build()
        {
            _byId = new Dictionary<string, SoundDefinition>();
            if (_sounds == null) return;

            foreach (SoundDefinition sound in _sounds)
            {
                if (sound == null) continue;

                if (_byId.ContainsKey(sound.Id))
                    Debug.LogWarning($"[Audio] duplicate sound id '{sound.Id}' in library", this);
                else
                    _byId.Add(sound.Id, sound);
            }
        }
    }
}
