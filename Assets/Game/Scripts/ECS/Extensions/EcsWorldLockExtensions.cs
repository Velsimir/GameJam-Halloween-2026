using Game.Scripts.ECS.Components.Doors;
using Leopotam.EcsLite;

namespace Game.Scripts.ECS.Extensions
{
    public static class EcsWorldLockExtensions
    {
        public static bool CanUnlock(this EcsWorld world, in Locked locked)
        {
            return world.HasItem(locked.Item);
        }
    }
}
