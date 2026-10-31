using System;
using UnityEngine;

namespace Game.Scripts.Configs.Scenario
{
    [Serializable]
    public class DiaryDayEntry
    {
        [SerializeField, Tooltip("Пусто = дата вычисляется от реальной даты игрока. Иначе показывается как есть")]
        private string _dateOverride;
        [SerializeField, Tooltip("Страницы нарратива дня: один ключ Typecraft = одна страница")]
        private string[] _narrativeKeys = Array.Empty<string>();

        public string DateOverride => _dateOverride;
        public string[] NarrativeKeys => _narrativeKeys ?? Array.Empty<string>();
    }
}
