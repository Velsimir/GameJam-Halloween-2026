using System;
using UnityEngine;
using UnityEngine.Audio;

namespace Game.Scripts.Configs.Audio
{
    public enum AudioBus
    {
        Master,
        Music,
        Ambience,
        SFX,
        Voice,
        UI
    }

    [Serializable]
    public struct DayAlarm
    {
        [Min(1)] public int FromDay;
        public SoundDefinition Sound;
    }

    [CreateAssetMenu(fileName = "AudioConfig", menuName = "Configs/Audio/AudioConfig")]
    public class AudioConfig : ScriptableObject
    {
        [Header("Mixer")]
        [SerializeField] private AudioMixer _mixer;
        [SerializeField] private SoundLibrary _library;
        [SerializeField, Min(4)] private int _poolSize = 24;
        [SerializeField, Min(0f)] private float _loopFadeIn = 0.6f;
        [SerializeField, Min(0f)] private float _loopFadeOut = 0.6f;

        [Header("Bus volumes")]
        [SerializeField, Range(0f, 1f)] private float _masterVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float _musicVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float _ambienceVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float _sfxVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float _voiceVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float _uiVolume = 1f;

        [Header("Footsteps")]
        [SerializeField] private SoundDefinition _footstep;
        [SerializeField, Range(0f, 1f)] private float _footstepPan = 0.25f;

        [Header("Fear breath")]
        [SerializeField] private SoundDefinition _breath;
        [SerializeField, Range(0f, 1f)] private float _breathStartFear = 0.15f;
        [SerializeField, Range(0f, 1f)] private float _breathStopFear = 0.08f;
        [SerializeField, Range(0f, 1f)] private float _breathMinVolume = 0.2f;
        [SerializeField, Range(0f, 1f)] private float _breathMaxVolume = 1f;
        [SerializeField, Range(0.5f, 2f)] private float _breathMinPitch = 0.95f;
        [SerializeField, Range(0.5f, 2f)] private float _breathMaxPitch = 1.15f;
        [SerializeField, Min(0f)] private float _breathResponse = 3f;

        [Header("Alarm")]
        [SerializeField] private DayAlarm[] _alarms;

        public AudioMixer Mixer => _mixer;
        public SoundLibrary Library => _library;
        public int PoolSize => _poolSize;
        public float LoopFadeIn => _loopFadeIn;
        public float LoopFadeOut => _loopFadeOut;
        public SoundDefinition Footstep => _footstep;
        public float FootstepPan => _footstepPan;
        public SoundDefinition Breath => _breath;
        public float BreathStartFear => _breathStartFear;
        public float BreathStopFear => _breathStopFear;
        public float BreathMinVolume => _breathMinVolume;
        public float BreathMaxVolume => _breathMaxVolume;
        public float BreathMinPitch => _breathMinPitch;
        public float BreathMaxPitch => _breathMaxPitch;
        public float BreathResponse => _breathResponse;

        public SoundDefinition AlarmFor(int day)
        {
            SoundDefinition result = null;
            int best = int.MinValue;

            if (_alarms == null) return result;

            foreach (DayAlarm alarm in _alarms)
            {
                if (alarm.Sound != null && alarm.FromDay <= day && alarm.FromDay > best)
                {
                    best = alarm.FromDay;
                    result = alarm.Sound;
                }
            }

            return result;
        }

        public float DefaultVolume(AudioBus bus)
        {
            switch (bus)
            {
                case AudioBus.Music: return _musicVolume;
                case AudioBus.Ambience: return _ambienceVolume;
                case AudioBus.SFX: return _sfxVolume;
                case AudioBus.Voice: return _voiceVolume;
                case AudioBus.UI: return _uiVolume;
                default: return _masterVolume;
            }
        }

        public static string ParameterName(AudioBus bus)
        {
            return bus + "Volume";
        }
    }
}
