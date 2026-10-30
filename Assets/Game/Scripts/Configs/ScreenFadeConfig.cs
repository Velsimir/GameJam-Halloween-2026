using UnityEngine;

namespace Game.Scripts.Configs
{
    [CreateAssetMenu(fileName = "ScreenFadeConfig", menuName = "Configs/ScreenFadeConfig")]
    public class ScreenFadeConfig : ScriptableObject
    {
        [SerializeField] private float _fadeOutTime = 0.6f;
        [SerializeField] private float _holdTime = 0.3f;
        [SerializeField] private float _fadeInTime = 0.8f;

        public float FadeOutTime => _fadeOutTime;
        public float HoldTime => _holdTime;
        public float FadeInTime => _fadeInTime;
    }
}
