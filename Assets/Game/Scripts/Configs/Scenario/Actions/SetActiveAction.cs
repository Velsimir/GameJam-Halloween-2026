using System;
using Game.Scripts.Services.Scenario;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Scripts.Configs.Scenario.Actions
{
    [Serializable]
    public class SetActiveAction : ScenarioAction
    {
        [SerializeField] private string _targetId;
        [SerializeField] private bool _active = true;

        public override void Execute(EcsWorld world, ScenarioContext context)
        {
            context.SetActive(_targetId, _active);
        }
    }
}
