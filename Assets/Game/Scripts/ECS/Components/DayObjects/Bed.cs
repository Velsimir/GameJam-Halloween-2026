using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Scripts.ECS.Components.DayObjects
{
    public struct Bed
    {
        public Transform Spawn;
        public Transform Look;
        public float GetUpDuration;
        public string WakeStepId;
        public bool PendingLie;
        public string LieThoughtKey;
    }

    public struct BedWake
    { }

    public struct LyingInBed
    {
        public Transform Look;
        public float GetUpDuration;
        public float Progress;
        public bool GettingUp;
        public string WakeStepId;
    }
}
