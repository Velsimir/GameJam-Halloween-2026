using Game.Scripts.Configs;
using Game.Scripts.Configs.Audio;
using Game.Scripts.ECS.Components.Transition;
using UnityEngine;

namespace Game.Scripts.ECS.Components.DayObjects
{
    public struct FadeAction
    {
        public TransitionPhase Phase;
        public float Timer;
        public float FadeOut;
        public float Hold;
        public float FadeIn;
        public SoundDefinition Sound;
        public string StepId;
        public ItemDefinition ConsumeItem;
        public GameObject[] ShowOnPeak;
        public GameObject[] HideOnPeak;
    }
}
