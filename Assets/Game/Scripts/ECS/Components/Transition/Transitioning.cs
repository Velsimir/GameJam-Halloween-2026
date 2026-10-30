using UnityEngine;

namespace Game.Scripts.ECS.Components.Transition
{
    public enum TransitionPhase
    {
        FadeOut,
        Hold,
        FadeIn
    }

    public struct Transitioning
    {
        public TransitionPhase Phase;
        public float Timer;
        public Transform Target;
        public string EnterThoughtKey;
    }
}
