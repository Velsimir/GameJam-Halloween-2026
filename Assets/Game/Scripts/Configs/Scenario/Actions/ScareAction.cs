using System;
using Game.Scripts.ECS.Components.Fear;
using Game.Scripts.Services.Scenario;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Scripts.Configs.Scenario.Actions
{
    [Serializable]
    public class ScareAction : ScenarioAction
    {
        [SerializeField, Range(0f, 1f)] private float _strength = 1f;

        public override void Execute(EcsWorld world, ScenarioContext context)
        {
            world.GetPool<ScareRequest>().Add(world.NewEntity()).Strength = _strength;
        }
    }
}
