using Leopotam.EcsLite;

namespace Game.Scripts.ECS
{
    public interface IEcsWorldProvider
    {
        EcsWorld World { get; }
        bool IsAlive { get; }
        void Reset();
    }
}
