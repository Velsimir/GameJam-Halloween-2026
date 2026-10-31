using UnityEngine;

namespace Game.Scripts.ECS.Components.DayObjects
{
    public struct Kettle
    {
        public bool On;
        public float Timer;
        public float Duration;
        public Renderer Indicator;
        public Material OnMaterial;
        public Material OffMaterial;
        public AudioClip Clip;
        public float Volume;
        public string StepId;
        public string PromptKey;
    }
}
