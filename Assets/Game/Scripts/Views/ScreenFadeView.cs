using TMPro;
using UnityEngine;

namespace Game.Scripts.Views
{
    [RequireComponent(typeof(CanvasGroup))]
    public class ScreenFadeView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private TMP_Text _caption;

        public float Alpha
        {
            get => _group.alpha;
            set => _group.alpha = Mathf.Clamp01(value);
        }

        public void SetCaption(string text)
        {
            if (_caption == null) return;

            _caption.text = text ?? string.Empty;
            _caption.enabled = string.IsNullOrEmpty(text) == false;
        }

        private void Awake()
        {
            _group.alpha = 0f;
            _group.blocksRaycasts = false;
            _group.interactable = false;
            SetCaption(null);
        }
    }
}
