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
        [SerializeField, Min(0.1f)] private float _getUpDuration = 2.5f;
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
            bed.GetUpDuration = _getUpDuration;
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
