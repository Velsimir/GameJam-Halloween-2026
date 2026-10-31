using Game.Scripts.ECS.Components.DayObjects;
using Game.Scripts.ECS.Components.Interaction;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Scripts.ECS.Authoring
{
    public class BedAuthoring : FadeActionAuthoring
    {
        [SerializeField] private Transform _spawn;
        [SerializeField] private Transform _look;
        [SerializeField] private Transform _alarmPoint;
        [SerializeField, Min(0.1f)] private float _getUpDuration = 4.5f;
        [SerializeField, Min(0f)] private float _riseHeight = 0.3f;
        [SerializeField] private float _turnAngle = 60f;
        [SerializeField, Range(-90f, 0f)] private float _lyingPitch = -90f;
        [SerializeField, Range(-90f, 0f)] private float _sitPitch = -10f;
        [SerializeField, Range(0.05f, 0.9f)] private float _risePhase = 0.35f;
        [SerializeField, Range(0.05f, 0.9f)] private float _turnPhase = 0.3f;
        [SerializeField] private string _wakeStepId;
        [SerializeField] private bool _lieOnStart;
        [SerializeField] private string _lieThoughtKey;
        [SerializeField] private string _getUpPromptKey = "ui.interact.get_up";

        protected override string DefaultPromptKey => "ui.interact.sleep";

        protected override void AddSpecificTag(EcsWorld world, int entity)
        {
            base.AddSpecificTag(world, entity);

            ref var bed = ref world.GetPool<Bed>().Add(entity);
            bed.Spawn = _spawn;
            bed.Look = _look;
            bed.AlarmPoint = _alarmPoint;
            bed.GetUpDuration = _getUpDuration;
            bed.RiseHeight = _riseHeight;
            bed.TurnAngle = _turnAngle;
            bed.LyingPitch = _lyingPitch;
            bed.SitPitch = _sitPitch;
            bed.RisePhase = _risePhase;
            bed.TurnPhase = _turnPhase;
            bed.WakeStepId = _wakeStepId;
            bed.PendingLie = _lieOnStart;
            bed.LieThoughtKey = _lieThoughtKey;

            int wake = world.NewEntity();
            world.GetPool<Interactable>().Add(wake);
            world.GetPool<InteractionPrompt>().Add(wake).Key = _getUpPromptKey;
            world.GetPool<BedWake>().Add(wake);
        }
    }
}
