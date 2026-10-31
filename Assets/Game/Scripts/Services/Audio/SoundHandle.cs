namespace Game.Scripts.Services.Audio
{
    public readonly struct SoundHandle
    {
        private readonly AudioVoice _voice;
        private readonly int _version;

        internal SoundHandle(AudioVoice voice)
        {
            _voice = voice;
            _version = voice != null ? voice.Version : 0;
        }

        public bool IsValid => _voice != null && _voice.Version == _version && _voice.Active;

        public void Stop(float fadeOut = -1f)
        {
            if (IsValid) _voice.RequestStop(fadeOut);
        }

        public void SetVolume(float volume)
        {
            if (IsValid) _voice.UserVolume = volume;
        }

        public void SetPitch(float pitch)
        {
            if (IsValid) _voice.Source.pitch = pitch;
        }
    }
}
