using Game.Scripts.Configs;
using Game.Scripts.Configs.Audio;
using Game.Scripts.ECS.Components.Doors;
using Game.Scripts.ECS.Components.Interaction;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Scripts.ECS.Authoring
{
    public class DoorAuthoring : InteractionObjectAuthoring
    {
        [SerializeField] private Transform _pivot;
        [SerializeField] private float _openingAngle;
        [SerializeField] private float _closingAngle;
        [SerializeField] private float _speed;
        [SerializeField] private string _openPromptKey = "ui.interact.open";
        [SerializeField] private string _closePromptKey = "ui.interact.close";
        [SerializeField] private ItemDefinition _requiredItem;
        [SerializeField] private string _lockedPromptKey = "ui.door.locked";
        [SerializeField] private string _lockedThoughtKey = "ui.thought.door_locked";
        [SerializeField] private string _unlockedThoughtKey = "ui.thought.door_unlocked";
        [SerializeField] private float _rattleDuration = 0.35f;
        [SerializeField] private float _rattleAngle = 2.5f;
        [SerializeField] private float _rattleCycles = 3f;
        [SerializeField] private ItemDefinition _lockItem;
        [SerializeField] private string _lockStepId;
        [SerializeField] private string _lockPromptKey = "ui.interact.lock_door";
        [SerializeField] private string _lockDoneThoughtKey = "ui.thought.door_lock_done";
        [SerializeField] private string _lockNoItemThoughtKey = "ui.thought.door_lock_no_keys";
        [SerializeField] private SoundDefinition _openSound;
        [SerializeField] private SoundDefinition _closeSound;

        protected override void AddSpecificTag(EcsWorld world, int entity)
        {
            ref var door = ref world.GetPool<Door>().Add(entity);

            door.Pivot = _pivot;
            door.InitialRotation = _pivot.localRotation;
            door.OpenAngle = _openingAngle;
            door.CloseAngle = _closingAngle;
            door.Speed = _speed;
            door.OpenPromptKey = _openPromptKey;
            door.ClosePromptKey = _closePromptKey;
            door.LockedPromptKey = _lockedPromptKey;
            door.RattleDuration = _rattleDuration;
            door.RattleAngle = _rattleAngle;
            door.RattleCycles = _rattleCycles;
            door.OpenSound = _openSound;
            door.CloseSound = _closeSound;

            string promptKey = door.IsOpen ? door.ClosePromptKey : door.OpenPromptKey;

            if (_requiredItem != null)
            {
                ref var locked = ref world.GetPool<Locked>().Add(entity);
                locked.Item = _requiredItem;
                locked.LockedThoughtKey = _lockedThoughtKey;
                locked.UnlockedThoughtKey = _unlockedThoughtKey;
                promptKey = door.LockedPromptKey;
            }

            if (_lockItem != null && string.IsNullOrEmpty(_lockStepId) == false)
            {
                ref var lockable = ref world.GetPool<LockableDoor>().Add(entity);
                lockable.Item = _lockItem;
                lockable.StepId = _lockStepId;
                lockable.LockPromptKey = _lockPromptKey;
                lockable.LockedThoughtKey = _lockedThoughtKey;
                lockable.UnlockedThoughtKey = _unlockedThoughtKey;
                lockable.LockDoneThoughtKey = _lockDoneThoughtKey;
                lockable.NoItemThoughtKey = _lockNoItemThoughtKey;
            }

            var promptPool = world.GetPool<InteractionPrompt>();
            if (promptPool.Has(entity) == false)
                promptPool.Add(entity);
            promptPool.Get(entity).Key = promptKey;
        }
    }
}
