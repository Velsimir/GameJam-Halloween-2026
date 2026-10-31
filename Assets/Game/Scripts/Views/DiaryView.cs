using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Scripts.Configs.Audio;
using Game.Scripts.Services.Audio;
using Game.Scripts.Services.Diary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Velsi.Typecraft.Dialogue;
using Velsi.Typecraft.Text;
using Zenject;

namespace Game.Scripts.Views
{
    public class DiaryView : MonoBehaviour
    {
        private const string Table = "Main";

        [Serializable]
        private class PageSlot
        {
            public RectTransform Root;
            public TypecraftText Date;
            public TypecraftText Body;
            public Button Click;

            [NonSerialized] public LineTypewriter Typewriter;
        }

        private class PageData
        {
            public string Raw;
            public DiaryDayRecord Record;
            public bool FirstOfDay;
            public bool Pad;
            public int TodayIndex = -1;
        }

        [SerializeField] private GameObject _window;
        [SerializeField] private CanvasGroup _windowGroup;
        [SerializeField] private CanvasGroup _pagesGroup;
        [SerializeField] private PageSlot _left;
        [SerializeField] private PageSlot _right;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _prevButton;
        [SerializeField] private Button _nextButton;
        [SerializeField] private SoundDefinition _openSound;
        [SerializeField] private SoundDefinition _closeSound;
        [SerializeField] private SoundDefinition _flipSound;
        [SerializeField, Min(0f)] private float _fadeTime = 0.2f;
        [SerializeField, Min(0f)] private float _flipTime = 0.15f;
        [SerializeField, Min(0f)] private float _printDelay = 0.35f;
        [SerializeField, Min(0f)] private float _autoFlipDelay = 0.7f;
        [SerializeField, Min(0)] private int _emptySpreads = 3;
        [SerializeField, Min(0f)] private float _bodySafety = 6f;

        [Inject] private IDiaryService _diary;
        [Inject] private IAudioService _audio;

        private readonly List<PageData> _pages = new List<PageData>();
        private CancellationTokenSource _printCts;
        private bool _open;
        private bool _ready;
        private bool _writable;
        private bool _flipping;
        private bool _closedFlag;
        private bool _closing;
        private bool _typing;
        private bool _printing;
        private bool _skip;
        private int _spread;
        private int _todayStart;
        private int _todayCount;

        public bool IsOpen => _open;

        public void Open(bool writable)
        {
            if (_open) return;

            _open = true;
            _ready = false;
            _closing = false;
            _closedFlag = false;
            _writable = writable;
            _window.SetActive(true);
            _windowGroup.alpha = 0f;
            _windowGroup.blocksRaycasts = true;
            _audio.Play(_openSound);
            OpenAsync().Forget();
        }

        public void RequestClose()
        {
            if (_open == false || _closing) return;

            _audio.Play(_closeSound);
            CloseAsync().Forget();
        }

        public bool TryConsumeClosed(out bool allRead)
        {
            allRead = false;
            if (_closedFlag == false) return false;

            _closedFlag = false;
            DiaryDayRecord today = _diary.Today;
            allRead = today == null || today.ReadPages >= _todayCount;
            return true;
        }

        private void Awake()
        {
            Prepare(_left);
            Prepare(_right);
            _left.Click.onClick.AddListener(() => OnPageClicked(false));
            _right.Click.onClick.AddListener(() => OnPageClicked(true));
            _closeButton.onClick.AddListener(RequestClose);
            _prevButton.onClick.AddListener(() => TryFlip(_spread - 1));
            _nextButton.onClick.AddListener(() => TryFlip(_spread + 1));
            _window.SetActive(false);
        }

        private void OnDestroy()
        {
            CancelPrint();
            _left.Typewriter?.Dispose();
            _right.Typewriter?.Dispose();
        }

        private static void Prepare(PageSlot slot)
        {
            foreach (Behaviour behaviour in slot.Body.GetComponents<Behaviour>())
                behaviour.enabled = true;

            slot.Typewriter = new LineTypewriter(slot.Body);
        }

        private async UniTaskVoid OpenAsync()
        {
            if (_writable) _diary.Reveal();

            Canvas.ForceUpdateCanvases();
            await BuildPagesAsync();

            _spread = StartSpread();
            ShowSpread(_spread);
            _ready = true;

            await FadeAsync(_windowGroup, 1f, _fadeTime);
            if (_closing) return;

            StartPrinting();
        }

        private async UniTaskVoid CloseAsync()
        {
            _closing = true;
            CancelPrint();
            _typing = false;
            _windowGroup.blocksRaycasts = false;

            await FadeAsync(_windowGroup, 0f, _fadeTime);

            _window.SetActive(false);
            _open = false;
            _ready = false;
            _closedFlag = true;
        }

        private async UniTask BuildPagesAsync()
        {
            _pages.Clear();
            _todayStart = 0;
            _todayCount = 0;

            DiaryDayRecord today = _diary.Today;
            float width = _left.Body.Target.rectTransform.rect.width;
            float height = _left.Body.Target.rectTransform.rect.height - _bodySafety;

            foreach (DiaryDayRecord record in _diary.Records)
            {
                bool isToday = record == today;
                if (isToday && record.Revealed == false)
                {
                    _todayStart = _pages.Count;
                    continue;
                }

                int first = _pages.Count;
                foreach (string key in record.NarrativeKeys)
                    _pages.Add(new PageData { Raw = await LoadRawAsync(key), Record = record });

                List<string> lines = new List<string>(record.LineKeys.Count);
                foreach (string key in record.LineKeys)
                    lines.Add(await LoadRawAsync(key));

                AddLinePages(record, lines, width, height);

                if (_pages.Count > first)
                    _pages[first].FirstOfDay = true;

                if (isToday)
                {
                    _todayStart = first;
                    _todayCount = _pages.Count - first;
                    for (int i = first; i < _pages.Count; i++)
                        _pages[i].TodayIndex = i - first;
                }

                if (_pages.Count % 2 != 0)
                    _pages.Add(new PageData { Pad = true });
            }

            if (today == null || today.Revealed == false)
                _todayStart = _pages.Count;

            for (int i = 0; i < _emptySpreads * 2; i++)
                _pages.Add(new PageData { Pad = true });
        }

        private void AddLinePages(DiaryDayRecord record, List<string> lines, float width, float height)
        {
            if (lines.Count == 0) return;

            TMP_Text measure = _left.Body.Target;
            List<string> current = new List<string>();
            foreach (string line in lines)
            {
                current.Add(line);
                if (current.Count > 1 && Measure(measure, current, width) > height)
                {
                    current.RemoveAt(current.Count - 1);
                    _pages.Add(new PageData { Raw = string.Join("\n", current), Record = record });
                    current.Clear();
                    current.Add(line);
                }
            }

            if (current.Count > 0)
                _pages.Add(new PageData { Raw = string.Join("\n", current), Record = record });
        }

        private static float Measure(TMP_Text text, List<string> lines, float width)
        {
            string rendered = Velsi.Typecraft.Typecraft.Render(string.Join("\n", lines));
            return text.GetPreferredValues(rendered, width, 0f).y;
        }

        private static UniTask<string> LoadRawAsync(string key)
        {
            var source = new UniTaskCompletionSource<string>();
            LocalizedText.Request(Table, key, null, (text, _) => source.TrySetResult(text));
            return source.Task;
        }

        private int StartSpread()
        {
            DiaryDayRecord today = _diary.Today;
            if (_writable && today != null && today.ReadPages < _todayCount)
                return (_todayStart + today.ReadPages) / 2;

            return _todayStart / 2;
        }

        private int SpreadCount => _pages.Count / 2;

        private int MaxSpread()
        {
            DiaryDayRecord today = _diary.Today;
            if (_writable && today != null && today.ReadPages < _todayCount)
                return (_todayStart + today.ReadPages) / 2;

            return SpreadCount - 1;
        }

        private bool HasUnread()
        {
            DiaryDayRecord today = _diary.Today;
            return _writable && today != null && today.ReadPages < _todayCount;
        }

        private bool IsUnread(int pageIndex)
        {
            if (_writable == false || pageIndex < 0 || pageIndex >= _pages.Count) return false;

            PageData page = _pages[pageIndex];
            return page.TodayIndex >= 0 && page.TodayIndex >= _diary.Today.ReadPages;
        }

        private void ShowSpread(int spread)
        {
            ShowPage(_left, spread * 2);
            ShowPage(_right, spread * 2 + 1);
            _prevButton.interactable = spread > 0;
            _nextButton.interactable = spread < MaxSpread();
        }

        private void ShowPage(PageSlot slot, int index)
        {
            slot.Typewriter.Cancel();
            PageData page = index >= 0 && index < _pages.Count ? _pages[index] : null;

            bool hasContent = page != null && page.Pad == false;
            slot.Date.SetMarkup(hasContent && page.FirstOfDay ? _diary.FormatDate(page.Record) : string.Empty);

            if (hasContent && IsUnread(index) == false)
            {
                slot.Typewriter.Begin(() => slot.Body.SetMarkup(page.Raw));
                slot.Typewriter.TryComplete();
                return;
            }

            slot.Body.SetMarkup(string.Empty);
        }

        private void StartPrinting()
        {
            CancelPrint();
            if (IsUnread(_spread * 2) == false && IsUnread(_spread * 2 + 1) == false) return;

            _printCts = new CancellationTokenSource();
            PrintLoopAsync(_printCts.Token).Forget();
        }

        private void CancelPrint()
        {
            if (_printCts == null) return;

            _printCts.Cancel();
            _printCts.Dispose();
            _printCts = null;
            _typing = false;
            _printing = false;
        }

        private async UniTaskVoid PrintLoopAsync(CancellationToken token)
        {
            _printing = true;
            try
            {
                while (true)
                {
                    for (int i = _spread * 2; i < _spread * 2 + 2; i++)
                    {
                        if (IsUnread(i) == false) continue;

                        PageData page = _pages[i];
                        await TypePageAsync(i == _spread * 2 ? _left : _right, page.Raw, token);
                        _diary.Today.ReadPages = Math.Max(_diary.Today.ReadPages, page.TodayIndex + 1);
                    }

                    _nextButton.interactable = _spread < MaxSpread();
                    if (HasUnread() == false) return;

                    await WaitAsync(_autoFlipDelay, token);
                    await FlipToAsync(_spread + 1, token);
                }
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                _printing = false;
            }
        }

        private async UniTask TypePageAsync(PageSlot slot, string raw, CancellationToken token)
        {
            _typing = true;
            _skip = false;
            try
            {
                float waited = 0f;
                while (waited < _printDelay && _skip == false)
                {
                    await UniTask.Yield();
                    token.ThrowIfCancellationRequested();
                    waited += Time.unscaledDeltaTime;
                }

                var done = new UniTaskCompletionSource();
                void OnCompleted() => done.TrySetResult();

                slot.Typewriter.Completed += OnCompleted;
                try
                {
                    slot.Typewriter.Begin(() => slot.Body.SetMarkup(raw));
                    if (_skip) slot.Typewriter.TryComplete();

                    while (done.Task.Status == UniTaskStatus.Pending)
                    {
                        if (_skip) slot.Typewriter.TryComplete();
                        _skip = false;
                        await UniTask.Yield();
                    token.ThrowIfCancellationRequested();
                    }
                }
                finally
                {
                    slot.Typewriter.Completed -= OnCompleted;
                }
            }
            finally
            {
                _typing = false;
            }
        }

        private void OnPageClicked(bool right)
        {
            if (_ready == false || _closing || _flipping) return;

            if (_typing)
            {
                _skip = true;
                return;
            }

            if (_printing) return;

            TryFlip(right ? _spread + 1 : _spread - 1);
        }

        private void TryFlip(int target)
        {
            if (_ready == false || _closing || _flipping) return;
            if (target < 0 || target > MaxSpread() || target == _spread) return;

            CancelPrint();
            FlipManualAsync(target).Forget();
        }

        private async UniTaskVoid FlipManualAsync(int target)
        {
            await FlipToAsync(target, destroyCancellationToken);
            if (_closing) return;

            StartPrinting();
        }

        private async UniTask FlipToAsync(int target, CancellationToken token)
        {
            _flipping = true;
            try
            {
                _audio.Play(_flipSound);
                await FadeAsync(_pagesGroup, 0f, _flipTime, token);
                _spread = target;
                ShowSpread(_spread);
                await FadeAsync(_pagesGroup, 1f, _flipTime, token);
            }
            finally
            {
                _flipping = false;
            }
        }

        private static async UniTask WaitAsync(float seconds, CancellationToken token)
        {
            float elapsed = 0f;
            while (elapsed < seconds)
            {
                elapsed += Time.unscaledDeltaTime;
                await UniTask.Yield();
                token.ThrowIfCancellationRequested();
            }
        }

        private async UniTask FadeAsync(CanvasGroup group, float target, float time, CancellationToken token = default)
        {
            float start = group.alpha;
            float elapsed = 0f;
            while (elapsed < time)
            {
                elapsed += Time.unscaledDeltaTime;
                group.alpha = Mathf.Lerp(start, target, elapsed / time);
                await UniTask.Yield();
                    token.ThrowIfCancellationRequested();
            }

            group.alpha = target;
        }
    }
}
