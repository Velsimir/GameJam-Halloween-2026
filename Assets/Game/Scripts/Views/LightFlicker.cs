using UnityEngine;

namespace Game.Scripts.Views
{
    [RequireComponent(typeof(Light))]
    public class LightFlicker : MonoBehaviour
    {
        [SerializeField] private float _minFactor = 0.55f;
        [SerializeField] private float _maxFactor = 1f;
        [SerializeField] private float _speed = 6f;
        [SerializeField, Range(0f, 1f)] private float _dropoutChance = 0.02f;
        [SerializeField] private float _dropoutDuration = 0.08f;

        private Light _light;
        private float _baseIntensity;
        private float _seed;
        private float _dropoutUntil;

        private void Awake()
        {
            _light = GetComponent<Light>();
            _baseIntensity = _light.intensity;
            _seed = Random.value * 100f;
        }

        private void Update()
        {
            if (Time.time < _dropoutUntil)
            {
                _light.intensity = 0f;
                return;
            }

            if (Random.value < _dropoutChance * Time.deltaTime * 60f)
                _dropoutUntil = Time.time + _dropoutDuration;

            float noise = Mathf.PerlinNoise(_seed, Time.time * _speed);
            _light.intensity = _baseIntensity * Mathf.Lerp(_minFactor, _maxFactor, noise);
        }
    }
}
