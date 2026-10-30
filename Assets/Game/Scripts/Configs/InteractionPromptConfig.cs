using UnityEngine;

namespace Game.Scripts.Configs
{
    [CreateAssetMenu(fileName = "InteractionPromptConfig", menuName = "Configs/InteractionPromptConfig")]
    public class InteractionPromptConfig : ScriptableObject
    {
        [SerializeField] private float _fadeInTime = 0.15f;
        [SerializeField] private float _fadeOutTime = 0.2f;

        public float FadeInTime => _fadeInTime;
        public float FadeOutTime => _fadeOutTime;
    }
}
