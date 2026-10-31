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
        [SerializeField] private AudioClip _introClip;
        [SerializeField, Range(0f, 1f)] private float _introVolume = 0.8f;

        [Header("Day change")]
        [SerializeField, Min(0f)] private float _dayFadeOut = 1.2f;
        [SerializeField, Min(0f)] private float _endFadeIn = 1f;

        [Header("Episode")]
        [SerializeField, Range(0f, 1f)] private float _episodeVolume = 0.8f;

        public string TitleKey => _titleKey;
        public float IntroHold => _introHold;
        public float IntroFadeOut => _introFadeOut;
        public AudioClip IntroClip => _introClip;
        public float IntroVolume => _introVolume;
        public float DayFadeOut => _dayFadeOut;
        public float EndFadeIn => _endFadeIn;
        public float EpisodeVolume => _episodeVolume;
    }
}
