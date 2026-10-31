using System;
using System.Collections.Generic;
using System.Globalization;
using Game.Scripts.Configs.Scenario;

namespace Game.Scripts.Services.Diary
{
    public class DiaryService : IDiaryService
    {
        private readonly List<DiaryDayRecord> _records = new List<DiaryDayRecord>();
        private DateTime? _startDate;

        public IReadOnlyList<DiaryDayRecord> Records => _records;
        public DiaryDayRecord Today { get; private set; }

        public void BeginDay(int day, DiaryDayEntry entry)
        {
            _startDate ??= DateTime.Today;

            foreach (DiaryDayRecord record in _records)
            {
                if (record.Day >= day) continue;
                record.Closed = true;
                record.Revealed = true;
                record.ReadPages = int.MaxValue;
            }

            DiaryDayRecord today = _records.Find(r => r.Day == day);
            if (today == null)
            {
                today = new DiaryDayRecord { Day = day };
                _records.Add(today);
                _records.Sort((a, b) => a.Day.CompareTo(b.Day));
            }

            today.DateOverride = entry?.DateOverride;
            today.NarrativeKeys.Clear();
            if (entry != null)
                today.NarrativeKeys.AddRange(entry.NarrativeKeys);

            Today = today;
        }

        public void AddLine(string key)
        {
            if (string.IsNullOrEmpty(key) || Today == null || Today.Revealed) return;
            Today.LineKeys.Add(key);
        }

        public void Reveal()
        {
            if (Today != null) Today.Revealed = true;
        }

        public string FormatDate(DiaryDayRecord record)
        {
            if (record == null) return string.Empty;
            if (string.IsNullOrEmpty(record.DateOverride) == false) return record.DateOverride;

            DateTime date = (_startDate ?? DateTime.Today).AddDays(record.Day - 1);
            string locale = Velsi.Typecraft.Typecraft.Locale ?? "en";
            CultureInfo culture;
            try
            {
                culture = CultureInfo.GetCultureInfo(locale);
            }
            catch (CultureNotFoundException)
            {
                culture = CultureInfo.GetCultureInfo("en");
            }

            string pattern = locale.StartsWith("en", StringComparison.OrdinalIgnoreCase) ? "MMMM d"
                : locale.StartsWith("zh", StringComparison.OrdinalIgnoreCase) ? "M月d日"
                : "d MMMM";
            return date.ToString(pattern, culture);
        }

        public void Restart()
        {
            _records.Clear();
            Today = null;
            _startDate = null;
        }
    }
}
