using Game.Scripts.Configs;
using Game.Scripts.Configs.Audio;
using Game.Scripts.ECS.Components.DayObjects;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Scripts.ECS.Authoring
{
    public class FadeActionAuthoring : InteractionObjectAuthoring
    {
        [SerializeField, Tooltip("<0 = из ScreenFadeConfig")] private float _fadeOutTime = -1f;
        [SerializeField, Tooltip("<0 = из ScreenFadeConfig")] private float _holdTime = -1f;
        [SerializeField, Tooltip("<0 = из ScreenFadeConfig")] private float _fadeInTime = -1f;
        [SerializeField] private SoundDefinition _sound;
        [SerializeField] private string _completeStepId;
        [SerializeField] private ItemDefinition _requiredItem;
        [SerializeField] private bool _consumeItem;
        [SerializeField] private string _missingItemThoughtKey;
        [SerializeField] private GameObject[] _showOnPeak;
        [SerializeField] private GameObject[] _hideOnPeak;

        protected override string DefaultPromptKey => "ui.interact.use";

        protected override void AddSpecificTag(EcsWorld world, int entity)
        {
            ref var trigger = ref world.GetPool<FadeActionTrigger>().Add(entity);
            trigger.FadeOut = _fadeOutTime;
            trigger.Hold = _holdTime;
            trigger.FadeIn = _fadeInTime;
            trigger.Sound = _sound;
            trigger.StepId = _completeStepId;
            trigger.RequiredItem = _requiredItem;
            trigger.ConsumeItem = _consumeItem;
            trigger.MissingItemThoughtKey = _missingItemThoughtKey;
            trigger.ShowOnPeak = _showOnPeak;
            trigger.HideOnPeak = _hideOnPeak;
        }
    }
}
