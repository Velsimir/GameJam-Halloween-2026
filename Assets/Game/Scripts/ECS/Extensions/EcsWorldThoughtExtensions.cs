using Game.Scripts.ECS.Components.Thoughts;
using Leopotam.EcsLite;
using Velsi.Typecraft.Dialogue;

namespace Game.Scripts.ECS.Extensions
{
    public static class EcsWorldThoughtExtensions
    {
        public static void RequestThought(this EcsWorld world, DialogueAsset asset, bool interrupt = false)
        {
            if (asset == null) return;

            int entity = world.NewEntity();
            ref ThoughtRequest request = ref world.GetPool<ThoughtRequest>().Add(entity);
            request.Asset = asset;
            request.Interrupt = interrupt;
        }

        public static void RequestThought(this EcsWorld world, string key, bool interrupt = false)
        {
            world.RequestThought(ThoughtDialogueFactory.Get(key), interrupt);
        }
    }
}
