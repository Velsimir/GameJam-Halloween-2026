using UnityEngine;

namespace Game.Scripts.Configs
{
    [CreateAssetMenu(fileName = "ObjectiveConfig", menuName = "Configs/ObjectiveConfig")]
    public class ObjectiveConfig : ScriptableObject
    {
        [Header("Durations")]
        [SerializeField, Min(0f)] private float _appearTime = 0.6f;
        [SerializeField, Min(0f)] private float _holdTime = 1.8f;
        [SerializeField, Min(0f)] private float _flyTime = 1f;
        [SerializeField, Min(0f)] private float _strikeTime = 0.45f;
        [SerializeField, Min(0f)] private float _fadeOutTime = 0.5f;
        [SerializeField, Min(0f)] private float _collapseTime = 0.3f;
        [SerializeField, Min(0f)] private float _visibilityFadeTime = 0.25f;

        [Header("Card")]
        [SerializeField, Min(1f)] private float _centerScale = 2.6f;
        [SerializeField, Range(0.1f, 1f)] private float _appearFromScale = 0.85f;

        [Header("Curves")]
        [SerializeField] private AnimationCurve _appearCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private AnimationCurve _flyCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private AnimationCurve _strikeCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        [SerializeField] private AnimationCurve _fadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        public float AppearTime => _appearTime;
        public float HoldTime => _holdTime;
        public float FlyTime => _flyTime;
        public float StrikeTime => _strikeTime;
        public float FadeOutTime => _fadeOutTime;
        public float CollapseTime => _collapseTime;
        public float VisibilityFadeTime => _visibilityFadeTime;
        public float CenterScale => _centerScale;
        public float AppearFromScale => _appearFromScale;
        public AnimationCurve AppearCurve => _appearCurve;
        public AnimationCurve FlyCurve => _flyCurve;
        public AnimationCurve StrikeCurve => _strikeCurve;
        public AnimationCurve FadeCurve => _fadeCurve;
    }
}
