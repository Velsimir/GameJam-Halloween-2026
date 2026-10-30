using UnityEngine;

namespace Game.Scripts.Views
{
    [RequireComponent(typeof(CanvasGroup))]
    public class ScreenFadeView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _group;

        public float Alpha
        {
            get => _group.alpha;
            set => _group.alpha = Mathf.Clamp01(value);
        }

        private void Awake()
        {
            _group.alpha = 0f;
            _group.blocksRaycasts = false;
            _group.interactable = false;
        }
    }
}
