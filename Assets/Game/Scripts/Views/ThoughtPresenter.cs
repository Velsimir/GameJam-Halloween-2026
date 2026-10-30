using UnityEngine;
using Velsi.Typecraft.Dialogue;

namespace Game.Scripts.Views
{
    [AddComponentMenu("Game/Thought Presenter")]
    public class ThoughtPresenter : TextDialoguePresenter
    {
        [SerializeField, Min(0f)] private float _fadeInTime = 0.3f;
        [SerializeField, Min(0f)] private float _fadeOutTime = 0.5f;

        private float _alpha;
        private float _target;

        protected override void SetVisible(bool visible)
        {
            base.SetVisible(visible);
            _target = visible ? 1f : 0f;
            if (Root != null)
            {
                Root.interactable = false;
                Root.blocksRaycasts = false;
            }

            ApplyAlpha();
        }

        protected override void Update()
        {
            base.Update();

            float time = _target > _alpha ? _fadeInTime : _fadeOutTime;
            float step = time > 0f ? Time.unscaledDeltaTime / time : 1f;
            _alpha = Mathf.MoveTowards(_alpha, _target, step);
            ApplyAlpha();
        }

        private void ApplyAlpha()
        {
            if (Root != null) Root.alpha = _alpha;
            else if (Text != null && Text.Target != null) Text.Target.alpha = _alpha;
        }
    }
}
