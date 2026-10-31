using UnityEngine;

namespace Game.Scripts.ECS.Components.Anomalies
{
    public struct Anomaly
    {
        public GameObject Target;
        public Renderer Renderer;
        public string ThoughtKey;
        public float MaxDistance;
        public float ViewAngle;
        public bool Seen;
    }
}
