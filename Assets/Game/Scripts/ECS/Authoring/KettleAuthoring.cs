using Game.Scripts.Configs.Audio;
using Game.Scripts.ECS.Components.DayObjects;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Scripts.ECS.Authoring
{
    public class KettleAuthoring : InteractionObjectAuthoring
    {
        [SerializeField, Min(0.1f)] private float _duration = 6f;
        [SerializeField] private Renderer _indicator;
        [SerializeField] private Material _onMaterial;
        [SerializeField] private Material _offMaterial;
        [SerializeField] private SoundDefinition _sound;
        [SerializeField] private string _completeStepId;

        protected override string DefaultPromptKey => "ui.interact.kettle";

        protected override void AddSpecificTag(EcsWorld world, int entity)
        {
            ref var kettle = ref world.GetPool<Kettle>().Add(entity);
            kettle.Duration = _duration;
            kettle.Indicator = _indicator;
            kettle.OnMaterial = _onMaterial;
            kettle.OffMaterial = _offMaterial;
            kettle.Sound = _sound;
            kettle.StepId = _completeStepId;
            kettle.PromptKey = DefaultPromptKey;

            if (_indicator != null && _offMaterial != null)
                _indicator.sharedMaterial = _offMaterial;
        }
    }
}
