using Game.Scripts.Configs;
using Game.Scripts.ECS.Components.Doors;
using Game.Scripts.ECS.Components.Interaction;
using Game.Scripts.ECS.Components.Transition;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Scripts.ECS.Authoring
{
    public class TransitionDoorAuthoring : InteractionObjectAuthoring
    {
        [SerializeField] private Transform _spawnPoint;
        [SerializeField] private string _goPromptKey = "ui.interact.go";
        [SerializeField] private ItemDefinition _requiredItem;
        [SerializeField] private string _lockedPromptKey = "ui.door.locked";
        [SerializeField] private string _lockedThoughtKey = "ui.thought.door_locked";
        [SerializeField] private string _unlockedThoughtKey = "ui.thought.door_unlocked";
        [SerializeField] private string _enterThoughtKey;

        protected override void AddSpecificTag(EcsWorld world, int entity)
        {
            ref var door = ref world.GetPool<TransitionDoor>().Add(entity);
            door.Target = _spawnPoint;
            door.GoPromptKey = _goPromptKey;
            door.EnterThoughtKey = _enterThoughtKey;

            string promptKey = _goPromptKey;

            if (_requiredItem != null)
            {
                ref var locked = ref world.GetPool<Locked>().Add(entity);
                locked.Item = _requiredItem;
                locked.LockedThoughtKey = _lockedThoughtKey;
                locked.UnlockedThoughtKey = _unlockedThoughtKey;
                promptKey = _lockedPromptKey;
            }

            var promptPool = world.GetPool<InteractionPrompt>();
            if (promptPool.Has(entity) == false)
                promptPool.Add(entity);
            promptPool.Get(entity).Key = promptKey;
        }
    }
}
