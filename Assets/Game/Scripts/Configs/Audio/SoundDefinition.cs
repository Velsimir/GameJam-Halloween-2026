using System;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

namespace Game.Scripts.Configs.Audio
{
    [CreateAssetMenu(fileName = "Sound", menuName = "Configs/Audio/Sound")]
    public class SoundDefinition : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private AudioClip[] _clips;
        [SerializeField] private AudioMixerGroup _group;
        [SerializeField, Range(0f, 1f)] private float _volume = 1f;
        [SerializeField, Range(0f, 1f)] private float _volumeSpread;
        [SerializeField, Range(0.1f, 3f)] private float _pitch = 1f;
        [SerializeField, Range(0f, 1f)] private float _pitchSpread;
        [SerializeField] private bool _loop;
        [SerializeField] private bool _spatial;
        [SerializeField, Min(0.01f)] private float _minDistance = 1f;
        [SerializeField, Min(0.02f)] private float _maxDistance = 15f;

        [NonSerialized] private int _lastIndex = -1;

        public string Id => string.IsNullOrEmpty(_id) ? name : _id;
        public AudioMixerGroup Group => _group;
        public float Volume => _volume;
        public bool Loop => _loop;
        public bool Spatial => _spatial;
        public float MinDistance => _minDistance;
        public float MaxDistance => Mathf.Max(_maxDistance, _minDistance + 0.01f);
        public bool HasClips => _clips != null && _clips.Length > 0;

        public float MaxClipLength
        {
            get
            {
                float length = 0f;
                if (_clips == null) return length;

                foreach (AudioClip clip in _clips)
                {
                    if (clip != null && clip.length > length) length = clip.length;
                }

                return length;
            }
        }

        public AudioClip PickClip()
        {
            if (HasClips == false) return null;
            if (_clips.Length == 1) return _clips[0];

            int index = Random.Range(0, _clips.Length);
            if (index == _lastIndex)
                index = (index + 1 + Random.Range(0, _clips.Length - 1)) % _clips.Length;

            _lastIndex = index;
            return _clips[index];
        }

        public float RollVolume()
        {
            return Mathf.Clamp01(_volume * (1f + Random.Range(-_volumeSpread, _volumeSpread)));
        }

        public float RollPitch()
        {
            return Mathf.Max(0.05f, _pitch + Random.Range(-_pitchSpread, _pitchSpread));
        }
    }
}
