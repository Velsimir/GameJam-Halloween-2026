using System;
using Game.Scripts.ECS.Extensions;
using Game.Scripts.Services.Scenario;
using Leopotam.EcsLite;
using UnityEngine;
using Velsi.Typecraft.Dialogue;

namespace Game.Scripts.Configs.Scenario.Actions
{
    [Serializable]
    public class ThoughtAction : ScenarioAction
    {
        [SerializeField] private DialogueAsset _dialogue;
        [SerializeField] private string _key;
        [SerializeField] private bool _interrupt;

        public override void Execute(EcsWorld world, ScenarioContext context)
        {
            if (_dialogue != null) world.RequestThought(_dialogue, _interrupt);
            else world.RequestThought(_key, _interrupt);
        }
    }
}
