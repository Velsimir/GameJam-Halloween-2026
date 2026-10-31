using System;
using System.Collections.Generic;
using System.Collections;
using Game.Scripts.Configs;
using UnityEngine;
using UnityEngine.UI;
using Velsi.Typecraft.Text;
using Zenject;

namespace Game.Scripts.Views
{
    public class ObjectivesView : MonoBehaviour
    {
        private const string Table = "Main";
        private const string TitleKey = "ui.objectives.title";
        private const float TitleFadeTime = 0.25f;

        private static readonly Vector3[] Corners = new Vector3[4];

        [SerializeField] private CanvasGroup _root;
        [SerializeField] private RectTransform _list;
        [SerializeField] private TypecraftText _title;
        [SerializeField] private CanvasGroup _titleGroup;
        [SerializeField] private ObjectiveRowView _rowPrefab;
        [SerializeField] private ObjectiveRowView _card;

        [Inject] private GameConfigs _gameConfigs;

        private readonly List<ObjectiveRowView> _rows = new List<ObjectiveRowView>();
        private bool _visible = true;
        private int _landed;

        public void SetVisible(bool visible)
        {
            _visible = visible;
        }

        public void PlayIntro(string key, Action onLanded)
        {
            StartCoroutine(IntroRoutine(key, onLanded));
        }

        public void Complete(string key, Action onDone)
        {
            StartCoroutine(CompleteRoutine(key, onDone));
        }

        private IEnumerator IntroRoutine(string key, Action onLanded)
        {
            ObjectiveConfig config = _gameConfigs.ObjectiveConfig;

            ObjectiveRowView row = Instantiate(_rowPrefab, _list);
            row.Bind(Table, key);
            row.Alpha = 0f;
            _rows.Add(row);

            _card.Bind(Table, key);
            _card.Alpha = 0f;
            _card.gameObject.SetActive(true);

            yield return null;
            LayoutRebuilder.ForceRebuildLayoutImmediate(_list);

            float width = _card.TextWidth;
            RectTransform cardRect = _card.Rect;
            cardRect.sizeDelta = new Vector2(width, row.Rect.rect.height);

            RectTransform rootRect = (RectTransform)_root.transform;
            Vector3 center = rootRect.TransformPoint(rootRect.rect.center);
            float centerScale = config.CenterScale;
            cardRect.localScale = Vector3.one * centerScale;
            Vector3 start = center - new Vector3(width * 0.5f * cardRect.lossyScale.x, 0f, 0f);
            cardRect.position = start;

            yield return Tween(config.AppearTime, config.AppearCurve, k =>
            {
                _card.Alpha = k;
                cardRect.localScale = Vector3.one * Mathf.Lerp(centerScale * config.AppearFromScale, centerScale, k);
            });

            _card.Alpha = 1f;
            cardRect.localScale = Vector3.one * centerScale;
            yield return new WaitForSeconds(config.HoldTime);

            yield return Tween(config.FlyTime, config.FlyCurve, k =>
            {
                cardRect.position = Vector3.LerpUnclamped(start, LeftMiddle(row.Rect), k);
                cardRect.localScale = Vector3.one * Mathf.LerpUnclamped(centerScale, 1f, k);
            });

            row.Alpha = 1f;
            _card.gameObject.SetActive(false);
            _landed++;
            onLanded?.Invoke();
        }

        private IEnumerator CompleteRoutine(string key, Action onDone)
        {
            ObjectiveConfig config = _gameConfigs.ObjectiveConfig;

            ObjectiveRowView row = _rows.Find(r => r.Key == key);
            if (row == null)
            {
                onDone?.Invoke();
                yield break;
            }

            yield return Tween(config.StrikeTime, config.StrikeCurve, row.SetStrike);
            yield return Tween(config.FadeOutTime, config.FadeCurve, k => row.Alpha = 1f - k);
            yield return Tween(config.CollapseTime, config.FadeCurve, row.SetCollapse);

            _rows.Remove(row);
            _landed = Mathf.Max(0, _landed - 1);
            Destroy(row.gameObject);
            onDone?.Invoke();
        }

        private void Awake()
        {
            _title.SetKey(Table, TitleKey);
            _titleGroup.alpha = 0f;
            _card.gameObject.SetActive(false);
            _root.alpha = 1f;
        }

        private void Update()
        {
            float time = _gameConfigs.ObjectiveConfig.VisibilityFadeTime;
            float step = time > 0f ? Time.unscaledDeltaTime / time : 1f;
            _root.alpha = Mathf.MoveTowards(_root.alpha, _visible ? 1f : 0f, step);

            float titleStep = Time.unscaledDeltaTime / TitleFadeTime;
            _titleGroup.alpha = Mathf.MoveTowards(_titleGroup.alpha, _landed > 0 ? 1f : 0f, titleStep);
        }

        private static Vector3 LeftMiddle(RectTransform rect)
        {
            rect.GetWorldCorners(Corners);
            return (Corners[0] + Corners[1]) * 0.5f;
        }

        private static IEnumerator Tween(float duration, AnimationCurve curve, Action<float> apply)
        {
            if (duration <= 0f)
            {
                apply(curve.Evaluate(1f));
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                apply(curve.Evaluate(Mathf.Clamp01(elapsed / duration)));
                yield return null;
            }

            apply(curve.Evaluate(1f));
        }
    }
}
