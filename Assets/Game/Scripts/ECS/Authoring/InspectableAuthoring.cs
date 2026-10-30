using Game.Scripts.ECS.Components.Inspect;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Scripts.ECS.Authoring
{
    public class InspectableAuthoring : InteractionObjectAuthoring
    {
        [SerializeField] private string[] _thoughtKeys;
        [SerializeField] private bool _oneShot;
        [SerializeField, Tooltip("Прервать текущую мысль и очистить очередь")] private bool _interruptThought;
        [SerializeField, Range(0f, 1f)] private float _scareStrength;

        protected override string DefaultPromptKey => "ui.interact.inspect";

        protected override void AddSpecificTag(EcsWorld world, int entity)
        {
            ref var inspectable = ref world.GetPool<Inspectable>().Add(entity);
            inspectable.ThoughtKeys = _thoughtKeys;
            inspectable.OneShot = _oneShot;
            inspectable.Interrupt = _interruptThought;
            inspectable.ScareStrength = _scareStrength;
        }
    }
}
