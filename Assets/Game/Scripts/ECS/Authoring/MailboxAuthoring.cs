using Game.Scripts.ECS.Components.DayObjects;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Scripts.ECS.Authoring
{
    public class MailboxAuthoring : DoorAuthoring
    {
        [SerializeField] private string _defaultLetterKey = "ui.thought.mailbox_empty";
        [SerializeField] private string _openStepId;

        protected override void AddSpecificTag(EcsWorld world, int entity)
        {
            base.AddSpecificTag(world, entity);

            ref var mailbox = ref world.GetPool<Mailbox>().Add(entity);
            mailbox.DefaultLetterKey = _defaultLetterKey;
            mailbox.OpenStepId = _openStepId;
        }
    }
}
