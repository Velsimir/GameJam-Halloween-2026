using Game.Scripts.ECS.Components.DayObjects;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Scripts.ECS.Authoring
{
    public class StepInteractAuthoring : InteractionObjectAuthoring
    {
        [SerializeField] private string _thoughtKey;
        [SerializeField] private string _completeStepId;
        [SerializeField] private Transform _episodeSpawn;

        protected override string DefaultPromptKey => "ui.interact.use";

        protected override void AddSpecificTag(EcsWorld world, int entity)
        {
            ref var interact = ref world.GetPool<StepInteract>().Add(entity);
            interact.ThoughtKey = _thoughtKey;
            interact.StepId = _completeStepId;
            interact.EpisodeSpawn = _episodeSpawn;
        }
    }
}
