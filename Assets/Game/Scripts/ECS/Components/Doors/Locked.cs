using Game.Scripts.Configs;

namespace Game.Scripts.ECS.Components.Doors
{
    public struct Locked
    {
        public ItemDefinition Item;
        public string LockedThoughtKey;
        public string UnlockedThoughtKey;
    }
}
