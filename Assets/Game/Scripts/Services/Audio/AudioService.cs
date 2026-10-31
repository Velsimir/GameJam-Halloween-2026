using System;
using System.Collections.Generic;
using Game.Scripts.Configs;
using Game.Scripts.Configs.Audio;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace Game.Scripts.Services.Audio
{
    public class AudioService : IAudioService, ITickable, IDisposable
    {
        private readonly AudioConfig _config;
        private readonly List<AudioVoice> _voices = new List<AudioVoice>();
        private readonly Dictionary<AudioBus, float> _busVolumes = new Dictionary<AudioBus, float>();
        private readonly Transform _root;

        public AudioService(GameConfigs gameConfigs)
        {
            _config = gameConfigs.AudioConfig;

            var host = new GameObject("AudioService");
            Object.DontDestroyOnLoad(host);
            _root = host.transform;

            foreach (AudioBus bus in Enum.GetValues(typeof(AudioBus)))
                SetBusVolume(bus, _config != null ? _config.DefaultVolume(bus) : 1f);
        }

        public SoundHandle Play(SoundDefinition sound, float volume = 1f, float pitch = 1f, float pan = 0f, float fadeIn = -1f)
        {
            AudioVoice voice = Acquire(sound, volume, pitch, fadeIn);
            if (voice == null) return default;

            voice.ConfigureFlat(pan);
            voice.Play();
            return new SoundHandle(voice);
        }

        public SoundHandle PlayAt(SoundDefinition sound, Vector3 position, float volume = 1f, float pitch = 1f, float fadeIn = -1f)
        {
            AudioVoice voice = Acquire(sound, volume, pitch, fadeIn);
            if (voice == null) return default;

            voice.ConfigureSpatial(sound, position, null);
            voice.Play();
            return new SoundHandle(voice);
        }

        public SoundHandle PlayOn(SoundDefinition sound, Transform target, float volume = 1f, float pitch = 1f, float fadeIn = -1f)
        {
            if (target == null) return Play(sound, volume, pitch, 0f, fadeIn);

            AudioVoice voice = Acquire(sound, volume, pitch, fadeIn);
            if (voice == null) return default;

            voice.ConfigureSpatial(sound, target.position, target);
            voice.Play();
            return new SoundHandle(voice);
        }

        public SoundHandle Play(string id, float volume = 1f, float pitch = 1f, float pan = 0f, float fadeIn = -1f)
        {
            if (_config == null || _config.Library == null) return default;

            return Play(_config.Library.Find(id), volume, pitch, pan, fadeIn);
        }

        public void SetBusVolume(AudioBus bus, float linear)
        {
            linear = Mathf.Clamp01(linear);
            _busVolumes[bus] = linear;

            if (_config == null || _config.Mixer == null) return;

            float db = linear <= 0.0001f ? -80f : Mathf.Log10(linear) * 20f;
            _config.Mixer.SetFloat(AudioConfig.ParameterName(bus), db);
        }

        public float GetBusVolume(AudioBus bus)
        {
            return _busVolumes.TryGetValue(bus, out float volume) ? volume : 1f;
        }

        public void StopAll(float fadeOut = 0f)
        {
            foreach (AudioVoice voice in _voices)
            {
                if (voice.Active) voice.RequestStop(fadeOut);
            }
        }

        public void Tick()
        {
            float delta = Time.unscaledDeltaTime;
            foreach (AudioVoice voice in _voices)
                voice.Tick(delta);
        }

        public void Dispose()
        {
            if (_root != null) Object.Destroy(_root.gameObject);
            _voices.Clear();
        }

        private AudioVoice Acquire(SoundDefinition sound, float volume, float pitch, float fadeIn)
        {
            if (sound == null) return null;

            AudioClip clip = sound.PickClip();
            if (clip == null) return null;

            AudioVoice voice = FindFree() ?? CreateVoice() ?? Steal();
            if (voice == null) return null;

            if (fadeIn < 0f)
                fadeIn = sound.Loop && _config != null ? _config.LoopFadeIn : 0f;

            float fadeOut = sound.Loop && _config != null ? _config.LoopFadeOut : 0f;
            voice.Begin(sound, clip, volume, pitch, fadeIn, fadeOut);
            return voice;
        }

        private AudioVoice FindFree()
        {
            foreach (AudioVoice voice in _voices)
            {
                if (voice.Active == false) return voice;
            }

            return null;
        }

        private AudioVoice CreateVoice()
        {
            int limit = _config != null ? _config.PoolSize : 24;
            if (_voices.Count >= limit) return null;

            var voice = new AudioVoice(_root);
            _voices.Add(voice);
            return voice;
        }

        private AudioVoice Steal()
        {
            AudioVoice oldest = null;
            foreach (AudioVoice voice in _voices)
            {
                if (voice.Loop) continue;
                if (oldest == null || voice.StartTime < oldest.StartTime) oldest = voice;
            }

            oldest?.Release();
            return oldest;
        }
    }
}
