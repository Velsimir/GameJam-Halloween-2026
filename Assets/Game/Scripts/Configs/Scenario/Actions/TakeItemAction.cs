using System;
using Game.Scripts.ECS.Extensions;
using Game.Scripts.Services.Scenario;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Scripts.Configs.Scenario.Actions
{
    [Serializable]
    public class TakeItemAction : ScenarioAction
    {
        [SerializeField] private ItemDefinition _item;

        public override void Execute(EcsWorld world, ScenarioContext context)
        {
            world.TryRemoveItem(_item);
        }
    }
}
