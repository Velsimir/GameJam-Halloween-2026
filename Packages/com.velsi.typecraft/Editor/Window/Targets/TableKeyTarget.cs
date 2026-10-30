using System;
using TMPro;
using Velsi.Typecraft.Editor.Localization;
using Velsi.Typecraft.Text;

namespace Velsi.Typecraft.Editor.Window.Targets
{
    public sealed class TableKeyTarget : ITextEditTarget
    {
        private readonly ITableBackend _backend;
        private readonly string _table;
        private readonly string _key;
        private readonly Func<string> _follow;
        private readonly Func<string> _locale;

        public TableKeyTarget(ITableBackend backend, string table, string key, Func<string> locale) : this(backend, table, key, locale, locale)
        {
        }

        private TableKeyTarget(ITableBackend backend, string table, string key, Func<string> follow, Func<string> locale)
        {
            _backend = backend;
            _table = table;
            _key = key;
            _follow = follow;
            _locale = locale;
        }

        public bool IsPinned => _locale != _follow;

        public TableKeyTarget Pinned(string locale) => new TableKeyTarget(_backend, _table, _key, _follow, () => locale);

        public TableKeyTarget Followed() => new TableKeyTarget(_backend, _table, _key, _follow, _follow);

        private string Locale => _locale() ?? Typecraft.Locale;
        private bool IsPseudo => Pseudolocalizer.IsPseudo(Locale);

        public string Title => $"{_table}/{_key} [{Locale}]";

        public string Markup
        {
            get
            {
                string markup = MarkupLayers.Display(_backend, _table, _key, IsPseudo ? _backend.BaseLocale : Locale);
                return IsPseudo ? Pseudolocalizer.Transform(markup) : markup;
            }
        }

        public bool IsValid => true;
        public bool CanWrite => !IsPseudo;
        public string ReadOnlyReason => IsPseudo ? "Псевдолокаль собирается из основного языка автоматически — выбери настоящий язык." : null;
        public string FontRole => null;
        public TMP_Text Reference => null;

        public void Write(string markup)
        {
            if (CanWrite)
                MarkupLayers.Write(_backend, _table, _key, Locale, markup);
        }

        public void SetFontRole(string role)
        {
        }
    }
}
