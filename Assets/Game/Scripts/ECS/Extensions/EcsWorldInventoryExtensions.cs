using Game.Scripts.Configs;
using Game.Scripts.ECS.Components.Items;
using Game.Scripts.ECS.Components.Player;
using Leopotam.EcsLite;

namespace Game.Scripts.ECS.Extensions
{
    public static class EcsWorldInventoryExtensions
    {
        public static bool HasItem(this EcsWorld world, ItemDefinition item)
        {
            if (item == null) return true;

            EcsPool<Inventory> pool = world.GetPool<Inventory>();
            foreach (int player in world.Filter<PlayerTag>().Inc<Inventory>().End())
            {
                if (pool.Get(player).Items.Contains(item))
                    return true;
            }

            return false;
        }

        public static bool TryAddItem(this EcsWorld world, ItemDefinition item)
        {
            if (item == null) return false;

            EcsPool<Inventory> pool = world.GetPool<Inventory>();
            foreach (int player in world.Filter<PlayerTag>().Inc<Inventory>().End())
            {
                pool.Get(player).Items.Add(item);
                return true;
            }

            return false;
        }
    }
}
