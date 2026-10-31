using Game.Scripts.Configs;
using Game.Scripts.Configs.Audio;
using UnityEngine;

namespace Game.Scripts.ECS.Components.DayObjects
{
    public struct FadeActionTrigger
    {
        public float FadeOut;
        public float Hold;
        public float FadeIn;
        public SoundDefinition Sound;
        public string StepId;
        public ItemDefinition RequiredItem;
        public bool ConsumeItem;
        public string MissingItemThoughtKey;
        public GameObject[] ShowOnPeak;
        public GameObject[] HideOnPeak;
    }
}
