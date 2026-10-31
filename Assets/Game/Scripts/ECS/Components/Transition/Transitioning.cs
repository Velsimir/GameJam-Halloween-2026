using Game.Scripts.Configs.Scenario;
using UnityEngine;

namespace Game.Scripts.ECS.Components.Transition
{
    public enum TransitionPhase
    {
        FadeOut,
        Episode,
        Hold,
        FadeIn
    }

    public struct Transitioning
    {
        public TransitionPhase Phase;
        public float Timer;
        public Transform Target;
        public string EnterThoughtKey;
        public DayEpisode Episode;
        public string StepId;
    }
}
