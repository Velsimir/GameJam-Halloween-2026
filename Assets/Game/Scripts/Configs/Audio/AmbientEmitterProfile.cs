using UnityEngine;

namespace Game.Scripts.Configs.Audio
{
    public enum AmbientEmitterMode
    {
        AtPoint,
        AroundPlayer
    }

    [CreateAssetMenu(fileName = "AmbientEmitter", menuName = "Configs/Audio/AmbientEmitterProfile")]
    public class AmbientEmitterProfile : ScriptableObject
    {
        [SerializeField] private SoundDefinition _sound;
        [SerializeField] private AmbientEmitterMode _mode;
        [SerializeField, Min(1)] private int _minDay = 1;
        [SerializeField, Min(0.1f)] private float _intervalMin = 20f;
        [SerializeField, Min(0.1f)] private float _intervalMax = 40f;
        [SerializeField, Range(0.1f, 1f), Tooltip("Множитель интервала при страхе 1")] private float _fearIntervalScale = 0.5f;

        [Header("Around player")]
        [SerializeField, Range(0f, 180f), Tooltip("Угол от направления взгляда: 90 = сбоку, 180 = за спиной")] private float _angleMin = 100f;
        [SerializeField, Range(0f, 180f)] private float _angleMax = 170f;
        [SerializeField, Min(0.5f)] private float _distanceMin = 2f;
        [SerializeField, Min(0.5f)] private float _distanceMax = 4f;

        public SoundDefinition Sound => _sound;
        public AmbientEmitterMode Mode => _mode;
        public int MinDay => _minDay;
        public float IntervalMin => _intervalMin;
        public float IntervalMax => Mathf.Max(_intervalMax, _intervalMin);
        public float FearIntervalScale => _fearIntervalScale;
        public float AngleMin => Mathf.Min(_angleMin, _angleMax);
        public float AngleMax => Mathf.Max(_angleMin, _angleMax);
        public float DistanceMin => Mathf.Min(_distanceMin, _distanceMax);
        public float DistanceMax => Mathf.Max(_distanceMin, _distanceMax);
    }
}
