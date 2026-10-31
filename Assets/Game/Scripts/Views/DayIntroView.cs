using UnityEngine;
using Velsi.Typecraft.Text;

namespace Game.Scripts.Views
{
    public class DayIntroView : MonoBehaviour
    {
        private const string Table = "Main";

        [SerializeField] private CanvasGroup _group;
        [SerializeField] private TypecraftText _text;

        public float Alpha
        {
            get => _group.alpha;
            set => _group.alpha = Mathf.Clamp01(value);
        }

        public void Show(string key, int day)
        {
            _text.SetKey(Table, key);
            _text.SetArguments(day);
            Alpha = 1f;
        }

        public void Hide()
        {
            Alpha = 0f;
        }

        private void Awake()
        {
            _group.alpha = 1f;
            _group.blocksRaycasts = false;
            _group.interactable = false;
        }
    }
}
