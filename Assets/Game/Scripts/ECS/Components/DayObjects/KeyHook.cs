using Game.Scripts.Configs;
using UnityEngine;

namespace Game.Scripts.ECS.Components.DayObjects
{
    public struct KeyHook
    {
        public ItemDefinition Keys;
        public GameObject KeysVisual;
        public string TakePromptKey;
        public string HangPromptKey;
        public string NoKeysThoughtKey;
        public string HangThoughtKey;
        public string TakeStepId;
        public string HangStepId;
    }
}
