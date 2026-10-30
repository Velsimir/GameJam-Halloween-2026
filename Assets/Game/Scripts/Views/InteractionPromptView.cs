using Game.Scripts.Configs;
using UnityEngine;
using UnityEngine.UI;
using Velsi.Typecraft.Text;
using Zenject;

namespace Game.Scripts.Views
{
    public class InteractionPromptView : MonoBehaviour
    {
        private const string Table = "Main";

        [SerializeField] private CanvasGroup _promptGroup;
        [SerializeField] private TypecraftText _text;
        [SerializeField] private Image _crosshair;

        [Inject] private GameConfigs _gameConfigs;

        private bool _visible;

        public void Show(string key)
        {
            _text.SetKey(Table, key);
            _visible = true;
        }

        public void Hide()
        {
            _visible = false;
        }

        public void SetCrosshairVisible(bool visible)
        {
            _crosshair.enabled = visible;
        }

        private void Awake()
        {
            _promptGroup.alpha = 0f;
        }

        private void Update()
        {
            InteractionPromptConfig config = _gameConfigs.InteractionPromptConfig;
            float time = _visible ? config.FadeInTime : config.FadeOutTime;
            float step = time > 0f ? Time.unscaledDeltaTime / time : 1f;
            _promptGroup.alpha = Mathf.MoveTowards(_promptGroup.alpha, _visible ? 1f : 0f, step);
        }
    }
}
