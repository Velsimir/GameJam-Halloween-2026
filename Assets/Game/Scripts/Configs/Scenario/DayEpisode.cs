using System;
using UnityEngine;

namespace Game.Scripts.Configs.Scenario
{
    [Serializable]
    public class DayEpisode
    {
        [SerializeField] private AudioClip[] _clips;
        [SerializeField, Min(0f)] private float _seconds;
        [SerializeField] private string _timeLabel;
        [SerializeField] private ItemDefinition _giveItem;
        [SerializeField] private string _debugName;

        public AudioClip[] Clips => _clips;
        public float Seconds => _seconds;
        public string TimeLabel => _timeLabel;
        public ItemDefinition GiveItem => _giveItem;
        public string DebugName => _debugName;
        public bool Enabled => _seconds > 0f;
    }
}
