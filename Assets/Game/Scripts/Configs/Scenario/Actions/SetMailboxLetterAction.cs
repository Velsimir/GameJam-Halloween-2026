using System;
using Game.Scripts.ECS.Components.DayObjects;
using Game.Scripts.ECS.Components.Scenario;
using Game.Scripts.Services.Scenario;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Scripts.Configs.Scenario.Actions
{
    [Serializable]
    public class SetMailboxLetterAction : ScenarioAction
    {
        [SerializeField] private string _targetId;
        [SerializeField] private string _letterThoughtKey;

        public override void Execute(EcsWorld world, ScenarioContext context)
        {
            EcsPool<Mailbox> mailboxes = world.GetPool<Mailbox>();
            EcsPool<ScenarioTarget> targets = world.GetPool<ScenarioTarget>();

            foreach (int entity in world.Filter<Mailbox>().Inc<ScenarioTarget>().End())
            {
                if (targets.Get(entity).Id == _targetId)
                    mailboxes.Get(entity).LetterKey = _letterThoughtKey;
            }
        }
    }
}
