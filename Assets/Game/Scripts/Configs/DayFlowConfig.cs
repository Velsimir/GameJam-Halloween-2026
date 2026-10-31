using Game.Scripts.Configs.Audio;
using UnityEngine;

namespace Game.Scripts.Configs
{
    [CreateAssetMenu(fileName = "DayFlowConfig", menuName = "Configs/DayFlowConfig")]
    public class DayFlowConfig : ScriptableObject
    {
        [Header("Intro")]
        [SerializeField] private string _titleKey = "ui.day.title";
        [SerializeField, Min(0f)] private float _introHold = 2f;
        [SerializeField, Min(0f)] private float _introFadeOut = 1.2f;
        [SerializeField] private SoundDefinition _introSound;

        [Header("Day change")]
        [SerializeField, Min(0f)] private float _dayFadeOut = 1.2f;
        [SerializeField, Min(0f)] private float _endFadeIn = 1f;
        [SerializeField] private SoundDefinition _endMusic;

        [Header("Episode")]
        [SerializeField, Min(0f)] private float _episodeSoundFadeOut = 0.4f;

        public string TitleKey => _titleKey;
        public float IntroHold => _introHold;
        public float IntroFadeOut => _introFadeOut;
        public SoundDefinition IntroSound => _introSound;
        public float DayFadeOut => _dayFadeOut;
        public float EndFadeIn => _endFadeIn;
        public SoundDefinition EndMusic => _endMusic;
        public float EpisodeSoundFadeOut => _episodeSoundFadeOut;
    }
}
