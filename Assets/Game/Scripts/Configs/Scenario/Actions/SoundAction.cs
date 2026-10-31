using System;
using Game.Scripts.Configs.Audio;
using Game.Scripts.Services.Scenario;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Scripts.Configs.Scenario.Actions
{
    [Serializable]
    public class SoundAction : ScenarioAction
    {
        [SerializeField] private SoundDefinition _sound;

        public override void Execute(EcsWorld world, ScenarioContext context)
        {
            context.PlaySound(_sound);
        }
    }
}
