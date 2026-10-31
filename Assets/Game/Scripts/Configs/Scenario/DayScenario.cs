using System.Collections.Generic;
using UnityEngine;

namespace Game.Scripts.Configs.Scenario
{
    [CreateAssetMenu(fileName = "Day", menuName = "Configs/Scenario/DayScenario")]
    public class DayScenario : ScriptableObject
    {
        [SerializeField, Min(1)] private int _day = 1;
        [SerializeField] private string _titleKey;
        [SerializeField] private DiaryDayEntry _diary = new DiaryDayEntry();
        [SerializeField] private List<DayStep> _steps = new List<DayStep>();

        public int Day => _day;
        public string TitleKey => _titleKey;
        public DiaryDayEntry Diary => _diary;
        public IReadOnlyList<DayStep> Steps => _steps;

        public IEnumerable<string> ValidationErrors()
        {
            for (int i = 0; i < _steps.Count; i++)
            {
                string error = _steps[i]?.Validate();
                if (error != null)
                    yield return $"{name} step {i + 1} '{_steps[i].Id}': {error}";
            }
        }

        private void OnValidate()
        {
            foreach (string error in ValidationErrors())
                Debug.LogWarning($"[Scenario] {error}", this);
        }
    }
}
