using System;
using Game.Scripts.Configs.Audio;
using UnityEngine;

namespace Game.Scripts.Configs.Scenario
{
    [Serializable]
    public class DayEpisode
    {
        [SerializeField] private SoundDefinition _sound;
        [SerializeField, Min(0f)] private float _seconds;
        [SerializeField] private string _timeLabel;
        [SerializeField] private ItemDefinition _giveItem;
        [SerializeField] private string _debugName;

        public SoundDefinition Sound => _sound;
        public float Seconds => _seconds;
        public float Duration => Mathf.Max(_seconds, _sound != null ? _sound.MaxClipLength : 0f);
        public string TimeLabel => _timeLabel;
        public ItemDefinition GiveItem => _giveItem;
        public string DebugName => _debugName;
        public bool Enabled => Duration > 0f;
    }
}
