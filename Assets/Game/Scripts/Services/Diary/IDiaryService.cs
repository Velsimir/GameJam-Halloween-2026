using System.Collections.Generic;
using Game.Scripts.Configs.Scenario;

namespace Game.Scripts.Services.Diary
{
    public interface IDiaryService
    {
        IReadOnlyList<DiaryDayRecord> Records { get; }
        DiaryDayRecord Today { get; }
        void BeginDay(int day, DiaryDayEntry entry);
        void AddLine(string key);
        void Reveal();
        string FormatDate(DiaryDayRecord record);
        void Restart();
    }
}
