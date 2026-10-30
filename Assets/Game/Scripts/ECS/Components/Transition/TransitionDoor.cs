using UnityEngine;

namespace Game.Scripts.ECS.Components.Transition
{
    public struct TransitionDoor
    {
        public Transform Target;
        public string GoPromptKey;
        public string EnterThoughtKey;
    }
}
