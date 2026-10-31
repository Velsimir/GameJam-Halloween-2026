using System.Collections.Generic;

namespace Game.Scripts.Services.Diary
{
    public class DiaryDayRecord
    {
        public int Day;
        public string DateOverride;
        public List<string> NarrativeKeys = new List<string>();
        public List<string> LineKeys = new List<string>();
        public bool Revealed;
        public int ReadPages;
        public bool Closed;
    }
}
