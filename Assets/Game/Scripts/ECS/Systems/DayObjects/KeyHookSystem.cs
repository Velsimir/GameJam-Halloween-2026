using Game.Scripts.ECS.Components.DayObjects;
using Game.Scripts.ECS.Components.Interaction;
using Game.Scripts.ECS.Extensions;
using Leopotam.EcsLite;

namespace Game.Scripts.ECS.Systems.DayObjects
{
    public class KeyHookSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld _world;
        private EcsFilter _hookFilter;
        private EcsFilter _requestFilter;
        private EcsPool<KeyHook> _poolHook;
        private EcsPool<InteractionPrompt> _poolPrompt;

        public void Init(IEcsSystems systems)
        {
            _world = systems.GetWorld();
            _hookFilter = _world.Filter<KeyHook>().Inc<InteractionPrompt>().End();
            _requestFilter = _world.Filter<KeyHook>().Inc<InteractionRequest>().End();
            _poolHook = _world.GetPool<KeyHook>();
            _poolPrompt = _world.GetPool<InteractionPrompt>();
        }

        public void Run(IEcsSystems systems)
        {
            foreach (int entity in _requestFilter)
                Interact(ref _poolHook.Get(entity));

            foreach (int entity in _hookFilter)
                _poolPrompt.Get(entity).Key = PromptFor(ref _poolHook.Get(entity));
        }

        private void Interact(ref KeyHook hook)
        {
            if (hook.KeysVisual != null && hook.KeysVisual.activeSelf)
            {
                if (_world.TryAddItem(hook.Keys) == false) return;

                hook.KeysVisual.SetActive(false);
                _world.RequestThought(hook.Keys.PickupThoughtKey);
                _world.CompleteScenarioStep(hook.TakeStepId);
                return;
            }

            if (_world.TryRemoveItem(hook.Keys) == false)
            {
                _world.RequestThought(hook.NoKeysThoughtKey);
                return;
            }

            if (hook.KeysVisual != null)
                hook.KeysVisual.SetActive(true);

            _world.RequestThought(hook.HangThoughtKey);
            _world.CompleteScenarioStep(hook.HangStepId);
        }

        private string PromptFor(ref KeyHook hook)
        {
            if (hook.KeysVisual != null && hook.KeysVisual.activeSelf)
                return hook.TakePromptKey;

            return _world.HasItem(hook.Keys) ? hook.HangPromptKey : string.Empty;
        }
    }
}
