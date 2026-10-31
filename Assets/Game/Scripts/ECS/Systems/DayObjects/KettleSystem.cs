using Game.Scripts.ECS.Components.DayObjects;
using Game.Scripts.ECS.Components.Interaction;
using Game.Scripts.ECS.Extensions;
using Game.Scripts.Services.Audio;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Scripts.ECS.Systems.DayObjects
{
    public class KettleSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly IAudioService _audio;

        private EcsWorld _world;
        private EcsFilter _kettleFilter;
        private EcsFilter _requestFilter;
        private EcsPool<Kettle> _poolKettle;
        private EcsPool<InteractionPrompt> _poolPrompt;

        public KettleSystem(IAudioService audio)
        {
            _audio = audio;
        }

        public void Init(IEcsSystems systems)
        {
            _world = systems.GetWorld();
            _kettleFilter = _world.Filter<Kettle>().End();
            _requestFilter = _world.Filter<Kettle>().Inc<InteractionRequest>().End();
            _poolKettle = _world.GetPool<Kettle>();
            _poolPrompt = _world.GetPool<InteractionPrompt>();
        }

        public void Run(IEcsSystems systems)
        {
            foreach (int entity in _requestFilter)
            {
                ref var kettle = ref _poolKettle.Get(entity);
                if (kettle.On) continue;

                SetOn(ref kettle, true);
                _audio.Play(kettle.Sound);
                _world.CompleteScenarioStep(kettle.StepId);
            }

            foreach (int entity in _kettleFilter)
            {
                ref var kettle = ref _poolKettle.Get(entity);
                if (kettle.On == false) continue;

                kettle.Timer += Time.deltaTime;
                if (kettle.Timer >= kettle.Duration)
                    SetOn(ref kettle, false);

                if (_poolPrompt.Has(entity))
                    _poolPrompt.Get(entity).Key = kettle.On ? string.Empty : kettle.PromptKey;
            }
        }

        private static void SetOn(ref Kettle kettle, bool on)
        {
            kettle.On = on;
            kettle.Timer = 0f;

            Material material = on ? kettle.OnMaterial : kettle.OffMaterial;
            if (kettle.Indicator != null && material != null)
                kettle.Indicator.sharedMaterial = material;
        }
    }
}
