using Game.Scripts.ECS.Components.Scenario;
using Leopotam.EcsLite;

namespace Game.Scripts.ECS.Extensions
{
    public static class EcsWorldScenarioExtensions
    {
        public static void CompleteScenarioStep(this EcsWorld world, string stepId)
        {
            if (string.IsNullOrEmpty(stepId)) return;

            world.GetPool<ScenarioExternalComplete>().Add(world.NewEntity()).StepId = stepId;
        }
    }
}
