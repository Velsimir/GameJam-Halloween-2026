using Game.Scripts.Configs;
using Game.Scripts.ECS.Components.DayObjects;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Scripts.ECS.Authoring
{
    public class KeyHookAuthoring : InteractionObjectAuthoring
    {
        [SerializeField] private ItemDefinition _keys;
        [SerializeField] private GameObject _keysVisual;
        [SerializeField] private string _takePromptKey = "ui.interact.take_keys";
        [SerializeField] private string _hangPromptKey = "ui.interact.hang_keys";
        [SerializeField] private string _noKeysThoughtKey = "ui.thought.hook_no_keys";
        [SerializeField] private string _hangThoughtKey = "ui.thought.keys_hung";
        [SerializeField] private string _takeStepId;
        [SerializeField] private string _hangStepId;

        protected override string DefaultPromptKey => _takePromptKey;

        protected override void AddSpecificTag(EcsWorld world, int entity)
        {
            ref var hook = ref world.GetPool<KeyHook>().Add(entity);
            hook.Keys = _keys;
            hook.KeysVisual = _keysVisual;
            hook.TakePromptKey = _takePromptKey;
            hook.HangPromptKey = _hangPromptKey;
            hook.NoKeysThoughtKey = _noKeysThoughtKey;
            hook.HangThoughtKey = _hangThoughtKey;
            hook.TakeStepId = _takeStepId;
            hook.HangStepId = _hangStepId;
        }
    }
}
