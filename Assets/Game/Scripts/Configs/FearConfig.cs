using UnityEngine;

namespace Game.Scripts.Configs
{
    [CreateAssetMenu(fileName = "FearConfig", menuName = "Configs/FearConfig")]
    public class FearConfig : ScriptableObject
    {
        [Header("Fear factor")]
        [SerializeField] private float _growthSpeed = 0.6f;
        [SerializeField] private float _decaySpeed = 0.25f;
        [SerializeField] private float _maxValue = 1f;

        [Header("Scare")]
        [SerializeField] private float _scareFearBoost = 0.6f;
        [SerializeField] private float _scareDuration = 0.7f;
        [SerializeField] private float _scarePositionAmplitude = 0.08f;
        [SerializeField] private float _scareRotationAmplitude = 4f;
        [SerializeField] private float _scareFovAmplitude = 12f;
        [SerializeField] private float _scareFrequency = 28f;

        [Header("Post process")]
        [SerializeField] private float _vignetteMax = 0.5f;
        [SerializeField] private float _vignetteSmoothness = 0.5f;
        [SerializeField] private float _filmGrainMax = 0.6f;
        [SerializeField] private float _chromaticAberrationMax = 0.7f;
        [SerializeField] private float _postProcessResponse = 4f;

        public float GrowthSpeed => _growthSpeed;
        public float DecaySpeed => _decaySpeed;
        public float MaxValue => _maxValue;
        public float ScareFearBoost => _scareFearBoost;
        public float ScareDuration => _scareDuration;
        public float ScarePositionAmplitude => _scarePositionAmplitude;
        public float ScareRotationAmplitude => _scareRotationAmplitude;
        public float ScareFovAmplitude => _scareFovAmplitude;
        public float ScareFrequency => _scareFrequency;
        public float VignetteMax => _vignetteMax;
        public float VignetteSmoothness => _vignetteSmoothness;
        public float FilmGrainMax => _filmGrainMax;
        public float ChromaticAberrationMax => _chromaticAberrationMax;
        public float PostProcessResponse => _postProcessResponse;
    }
}
