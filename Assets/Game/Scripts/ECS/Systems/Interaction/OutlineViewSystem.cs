using Game.Scripts.ECS.Components.Interaction;
using Game.Scripts.ECS.Components.Scenario;
using Leopotam.EcsLite;

namespace Game.Scripts.ECS.Systems.Interaction
{
    public class OutlineViewSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsFilter _filterNeedEnableOutline;
        private EcsFilter _filterNeedDisableOutline;
        private EcsFilter _filterLockedOutline;
        private EcsPool<OutlineEnabled> _poolOutlineEnabled;
        private EcsPool<OutlineRefs> _poolOutlineRefs;

        public void Init(IEcsSystems systems)
        {
            EcsWorld world = systems.GetWorld();
            _filterNeedEnableOutline = world.Filter<Focused>().Inc<OutlineRefs>().Exc<OutlineEnabled>().Exc<ScenarioLocked>().End();
            _filterNeedDisableOutline = world.Filter<OutlineEnabled>().Inc<OutlineRefs>().Exc<Focused>().End();
            _filterLockedOutline = world.Filter<OutlineEnabled>().Inc<OutlineRefs>().Inc<ScenarioLocked>().End();

            _poolOutlineEnabled = world.GetPool<OutlineEnabled>();
            _poolOutlineRefs = world.GetPool<OutlineRefs>();
        }

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in _filterNeedDisableOutline)
                DisableOutline(entity);

            foreach (var entity in _filterLockedOutline)
                DisableOutline(entity);

            foreach (var entity in _filterNeedEnableOutline)
            {
                ref var outlineRefs = ref _poolOutlineRefs.Get(entity);
                outlineRefs.MeshRenderer.sharedMaterials = outlineRefs.WithOutline;
                _poolOutlineEnabled.Add(entity);
            }
        }

        private void DisableOutline(int entity)
        {
            ref var outlineRefs = ref _poolOutlineRefs.Get(entity);
            outlineRefs.MeshRenderer.sharedMaterials = outlineRefs.Default;
            _poolOutlineEnabled.Del(entity);
        }
    }
}