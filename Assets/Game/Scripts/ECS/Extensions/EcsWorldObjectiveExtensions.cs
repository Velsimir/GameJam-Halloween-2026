using Game.Scripts.ECS.Components.Objectives;
using Leopotam.EcsLite;
using Velsi.Typecraft.Dialogue;

namespace Game.Scripts.ECS.Extensions
{
    public static class EcsWorldObjectiveExtensions
    {
        public static void ShowObjective(this EcsWorld world, string key)
        {
            world.ShowObjective(key, (DialogueAsset)null);
        }

        public static void ShowObjective(this EcsWorld world, string key, DialogueAsset thought)
        {
            if (string.IsNullOrEmpty(key)) return;

            int entity = world.NewEntity();
            ref ObjectiveShowRequest request = ref world.GetPool<ObjectiveShowRequest>().Add(entity);
            request.Key = key;
            request.ThoughtAsset = thought;
        }

        public static void ShowObjective(this EcsWorld world, string key, string thoughtKey)
        {
            world.ShowObjective(key, ThoughtDialogueFactory.Get(thoughtKey));
        }

        public static void CompleteObjective(this EcsWorld world, string key)
        {
            if (string.IsNullOrEmpty(key)) return;

            int entity = world.NewEntity();
            world.GetPool<ObjectiveCompleteRequest>().Add(entity).Key = key;
        }
    }
}
