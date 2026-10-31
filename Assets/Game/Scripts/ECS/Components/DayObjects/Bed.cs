using Game.Scripts.Services.Audio;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Scripts.ECS.Components.DayObjects
{
    public struct Bed
    {
        public Transform Spawn;
        public Transform Look;
        public float GetUpDuration;
        public float RiseHeight;
        public float TurnAngle;
        public float LyingPitch;
        public float SitPitch;
        public float RisePhase;
        public float TurnPhase;
        public string WakeStepId;
        public bool PendingLie;
        public string LieThoughtKey;
        public Transform AlarmPoint;
    }

    public struct BedWake
    { }

    public struct LyingInBed
    {
        public Transform Look;
        public float GetUpDuration;
        public float RiseHeight;
        public float TurnAngle;
        public float LyingPitch;
        public float SitPitch;
        public float RisePhase;
        public float TurnPhase;
        public float Progress;
        public bool GettingUp;
        public string WakeStepId;
        public SoundHandle Alarm;
    }
}
