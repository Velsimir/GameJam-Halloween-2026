using Game.Scripts.Configs;
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
        public AudioClip Clip;
        public float Volume;
        public string StepId;
        public ItemDefinition ConsumeItem;
        public GameObject[] ShowOnPeak;
        public GameObject[] HideOnPeak;
    }
}
