using UnityEngine;
using UnityEngine.UI;
using Velsi.Typecraft.Text;

namespace Game.Scripts.Views
{
    public class ObjectiveRowView : MonoBehaviour
    {
        [SerializeField] private TypecraftText _text;
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private RectTransform _strike;
        [SerializeField] private LayoutElement _layout;

        private float _height;

        public string Key { get; private set; }

        public RectTransform Rect => (RectTransform)transform;

        public float Alpha
        {
            get => _group.alpha;
            set => _group.alpha = Mathf.Clamp01(value);
        }

        public float TextWidth
        {
            get
            {
                _text.Target.ForceMeshUpdate();
                return _text.Target.preferredWidth;
            }
        }

        private void Awake()
        {
            if (_layout != null) _height = _layout.preferredHeight;
            _strike.gameObject.SetActive(false);
        }

        public void Bind(string table, string key)
        {
            Key = key;
            _text.SetKey(table, key);
            SetStrike(0f);
        }

        public void SetStrike(float progress)
        {
            _strike.gameObject.SetActive(progress > 0f);
            _strike.sizeDelta = new Vector2(TextWidth * Mathf.Clamp01(progress), _strike.sizeDelta.y);
        }

        public void SetCollapse(float progress)
        {
            if (_layout != null) _layout.preferredHeight = Mathf.Lerp(_height, 0f, Mathf.Clamp01(progress));
        }
    }
}
