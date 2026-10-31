using Game.Scripts.Configs.Audio;
using UnityEngine;

namespace Game.Scripts.Services.Audio
{
    internal class AudioVoice
    {
        public readonly AudioSource Source;
        public int Version;
        public bool Active;
        public bool Loop;
        public float StartTime;
        public float BaseVolume;
        public float UserVolume = 1f;
        public Transform Follow;

        private float _fade;
        private float _fadeFrom;
        private float _fadeTarget;
        private float _fadeDuration;
        private float _fadeTimer;
        private bool _stopping;
        private float _defaultFadeOut;

        public AudioVoice(Transform parent)
        {
            var host = new GameObject("Voice");
            host.transform.SetParent(parent, false);
            Source = host.AddComponent<AudioSource>();
            Source.playOnAwake = false;
            host.SetActive(false);
        }

        public void Begin(SoundDefinition sound, AudioClip clip, float volume, float pitch, float fadeIn, float defaultFadeOut)
        {
            Version++;
            _defaultFadeOut = defaultFadeOut;
            Active = true;
            Loop = sound.Loop;
            StartTime = Time.unscaledTime;
            BaseVolume = sound.RollVolume() * volume;
            UserVolume = 1f;
            Follow = null;
            _stopping = false;

            Source.gameObject.SetActive(true);
            Source.clip = clip;
            Source.loop = sound.Loop;
            Source.pitch = sound.RollPitch() * pitch;
            Source.outputAudioMixerGroup = sound.Group;
            Source.panStereo = 0f;

            if (fadeIn > 0f)
            {
                _fade = 0f;
                _fadeFrom = 0f;
                _fadeTarget = 1f;
                _fadeDuration = fadeIn;
                _fadeTimer = 0f;
            }
            else
            {
                _fade = 1f;
                _fadeDuration = 0f;
            }

            ApplyVolume();
        }

        public void ConfigureSpatial(SoundDefinition sound, Vector3 position, Transform follow)
        {
            Source.spatialBlend = 1f;
            Source.rolloffMode = AudioRolloffMode.Logarithmic;
            Source.minDistance = sound.MinDistance;
            Source.maxDistance = sound.MaxDistance;
            Source.dopplerLevel = 0f;
            Source.spread = 0f;
            Source.transform.position = position;
            Follow = follow;
        }

        public void ConfigureFlat(float pan)
        {
            Source.spatialBlend = 0f;
            Source.panStereo = Mathf.Clamp(pan, -1f, 1f);
        }

        public void Play()
        {
            Source.Play();
        }

        public void RequestStop(float fadeOut)
        {
            if (_stopping) return;

            if (fadeOut < 0f) fadeOut = _defaultFadeOut;

            _stopping = true;
            if (fadeOut <= 0f)
            {
                Release();
                return;
            }

            _fadeFrom = _fade;
            _fadeTarget = 0f;
            _fadeDuration = fadeOut;
            _fadeTimer = 0f;
        }

        public void Tick(float deltaTime)
        {
            if (Active == false) return;

            if (Follow != null)
                Source.transform.position = Follow.position;

            if (_fadeDuration > 0f)
            {
                _fadeTimer += deltaTime;
                float t = Mathf.Clamp01(_fadeTimer / _fadeDuration);
                _fade = Mathf.Lerp(_fadeFrom, _fadeTarget, t);
                if (t >= 1f) _fadeDuration = 0f;
            }

            ApplyVolume();

            if (_stopping && _fadeDuration <= 0f)
            {
                Release();
                return;
            }

            if (Loop == false && Source.isPlaying == false)
                Release();
        }

        public void Release()
        {
            if (Active == false) return;

            Active = false;
            Source.Stop();
            Source.clip = null;
            Source.gameObject.SetActive(false);
        }

        private void ApplyVolume()
        {
            Source.volume = BaseVolume * UserVolume * _fade;
        }
    }
}
