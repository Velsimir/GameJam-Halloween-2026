using Game.Scripts.Configs;

namespace Game.Scripts.ECS.Components.Doors
{
    public struct LockableDoor
    {
        public ItemDefinition Item;
        public string LockPromptKey;
        public string LockedThoughtKey;
        public string UnlockedThoughtKey;
        public string LockDoneThoughtKey;
        public string NoItemThoughtKey;
        public string StepId;
    }
}
