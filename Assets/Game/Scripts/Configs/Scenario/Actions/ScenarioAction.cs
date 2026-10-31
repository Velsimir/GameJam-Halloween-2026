using System;
using Game.Scripts.Services.Scenario;
using Leopotam.EcsLite;

namespace Game.Scripts.Configs.Scenario.Actions
{
    [Serializable]
    public abstract class ScenarioAction
    {
        public abstract void Execute(EcsWorld world, ScenarioContext context);
    }
}
