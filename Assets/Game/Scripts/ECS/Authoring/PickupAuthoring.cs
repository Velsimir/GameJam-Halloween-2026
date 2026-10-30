using Game.Scripts.Configs;
using Game.Scripts.ECS.Components.Items;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Scripts.ECS.Authoring
{
    public class PickupAuthoring : InteractionObjectAuthoring
    {
        [SerializeField] private ItemDefinition _item;

        protected override string DefaultPromptKey => "ui.interact.take";

        protected override void AddSpecificTag(EcsWorld world, int entity)
        {
            ref var pickup = ref world.GetPool<Pickup>().Add(entity);
            pickup.Item = _item;
            pickup.View = gameObject;
        }
    }
}
