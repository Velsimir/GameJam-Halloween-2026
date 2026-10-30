using UnityEngine;
using UnityEngine.Rendering;

namespace Game.Scripts.Rendering.VHS
{
    [RequireComponent(typeof(Volume))]
    public class VhsBoost : MonoBehaviour, IVhsBoost
    {
        [SerializeField] private Volume _heavyVolume;

        private float _targetWeight;
        private float _smoothTime = 0.2f;
        private float _smoothedWeight;
        private float _smoothVelocity;

        private float _pulsePeak;
        private float _pulseAttack;
        private float _pulseHold;
        private float _pulseRelease;
        private float _pulseTime = float.MaxValue;

        public float Weight { get; private set; }

        private void Reset()
        {
            _heavyVolume = GetComponent<Volume>();
        }

        private void Awake()
        {
            if (_heavyVolume == null)
            {
                _heavyVolume = GetComponent<Volume>();
            }

            Apply(0f);
        }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;

            _smoothedWeight = Mathf.SmoothDamp(_smoothedWeight, _targetWeight, ref _smoothVelocity, Mathf.Max(_smoothTime, 0.0001f), Mathf.Infinity, dt);

            float pulse = EvaluatePulse(dt);

            Apply(Mathf.Max(_smoothedWeight, pulse));
        }

        public void Pulse(float peak, float attack, float hold, float release)
        {
            _pulsePeak = Mathf.Clamp01(peak);
            _pulseAttack = Mathf.Max(attack, 0.0001f);
            _pulseHold = Mathf.Max(hold, 0f);
            _pulseRelease = Mathf.Max(release, 0.0001f);
            _pulseTime = 0f;
        }

        public void SetTarget(float weight, float smoothTime)
        {
            _targetWeight = Mathf.Clamp01(weight);
            _smoothTime = smoothTime;
        }

        public void Clear()
        {
            _targetWeight = 0f;
            _smoothedWeight = 0f;
            _smoothVelocity = 0f;
            _pulseTime = float.MaxValue;
            Apply(0f);
        }

        private float EvaluatePulse(float dt)
        {
            if (_pulseTime == float.MaxValue)
            {
                return 0f;
            }

            _pulseTime += dt;

            if (_pulseTime < _pulseAttack)
            {
                return _pulsePeak * (_pulseTime / _pulseAttack);
            }

            float afterAttack = _pulseTime - _pulseAttack;
            if (afterAttack < _pulseHold)
            {
                return _pulsePeak;
            }

            float afterHold = afterAttack - _pulseHold;
            if (afterHold < _pulseRelease)
            {
                return _pulsePeak * (1f - afterHold / _pulseRelease);
            }

            _pulseTime = float.MaxValue;
            return 0f;
        }

        private void Apply(float weight)
        {
            Weight = weight;
            if (_heavyVolume != null)
            {
                _heavyVolume.weight = weight;
            }
        }
    }
}
