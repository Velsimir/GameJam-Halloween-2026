using System;
using Game.Scripts.Services.Scenario;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Scripts.Configs.Scenario.Actions
{
    [Serializable]
    public class SoundAction : ScenarioAction
    {
        [SerializeField] private AudioClip _clip;
        [SerializeField, Range(0f, 1f)] private float _volume = 1f;

        public override void Execute(EcsWorld world, ScenarioContext context)
        {
            context.PlaySound(_clip, _volume);
        }
    }
}
