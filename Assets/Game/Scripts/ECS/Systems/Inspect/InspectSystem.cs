using Game.Scripts.ECS.Components.Fear;
using Game.Scripts.ECS.Components.Inspect;
using Game.Scripts.ECS.Components.Interaction;
using Game.Scripts.ECS.Extensions;
using Leopotam.EcsLite;

namespace Game.Scripts.ECS.Systems.Inspect
{
    public class InspectSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld _world;
        private EcsFilter _filter;
        private EcsPool<Inspectable> _poolInspectable;
        private EcsPool<Interactable> _poolInteractable;
        private EcsPool<Focused> _poolFocused;
        private EcsPool<InteractionPrompt> _poolPrompt;
        private EcsPool<ScareRequest> _poolScare;

        public void Init(IEcsSystems systems)
        {
            _world = systems.GetWorld();
            _filter = _world.Filter<Inspectable>().Inc<InteractionRequest>().Inc<Interactable>().End();
            _poolInspectable = _world.GetPool<Inspectable>();
            _poolInteractable = _world.GetPool<Interactable>();
            _poolFocused = _world.GetPool<Focused>();
            _poolPrompt = _world.GetPool<InteractionPrompt>();
            _poolScare = _world.GetPool<ScareRequest>();
        }

        public void Run(IEcsSystems systems)
        {
            foreach (int entity in _filter)
            {
                ref var inspectable = ref _poolInspectable.Get(entity);

                string[] keys = inspectable.ThoughtKeys;
                if (keys != null && keys.Length > 0)
                {
                    int index = inspectable.Count < keys.Length ? inspectable.Count : keys.Length - 1;
                    _world.RequestThought(keys[index], inspectable.Interrupt);
                }

                inspectable.Count++;

                if (inspectable.Count == 1 && inspectable.ScareStrength > 0f)
                    _poolScare.Add(_world.NewEntity()).Strength = inspectable.ScareStrength;

                if (inspectable.OneShot)
                {
                    _poolInteractable.Del(entity);
                    if (_poolFocused.Has(entity)) _poolFocused.Del(entity);
                    if (_poolPrompt.Has(entity)) _poolPrompt.Del(entity);
                }
            }
        }
    }
}
