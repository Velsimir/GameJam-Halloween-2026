using Game.Scripts.ECS.Components.Diary;
using Game.Scripts.ECS.Components.Interaction;
using Game.Scripts.ECS.Components.Scenario;
using Game.Scripts.ECS.Extensions;
using Game.Scripts.Services.Diary;
using Leopotam.EcsLite;

namespace Game.Scripts.ECS.Systems.Diary
{
    public class DiaryInteractionSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly IDiaryService _diary;

        private EcsWorld _world;
        private EcsFilter _filter;
        private EcsFilter _progressFilter;
        private EcsPool<DiaryObject> _poolDiary;
        private EcsPool<ScenarioActive> _poolActive;
        private EcsPool<ScenarioProgress> _poolProgress;
        private EcsPool<DiaryOpenRequest> _poolOpenRequest;

        public DiaryInteractionSystem(IDiaryService diary)
        {
            _diary = diary;
        }

        public void Init(IEcsSystems systems)
        {
            _world = systems.GetWorld();
            _filter = _world.Filter<DiaryObject>().Inc<InteractionRequest>().End();
            _progressFilter = _world.Filter<ScenarioProgress>().End();
            _poolDiary = _world.GetPool<DiaryObject>();
            _poolActive = _world.GetPool<ScenarioActive>();
            _poolProgress = _world.GetPool<ScenarioProgress>();
            _poolOpenRequest = _world.GetPool<DiaryOpenRequest>();
        }

        public void Run(IEcsSystems systems)
        {
            foreach (int entity in _filter)
            {
                ref DiaryObject diary = ref _poolDiary.Get(entity);
                bool writable = IsWritable(entity);

                if (writable == false && (_diary.Today == null || _diary.Today.Revealed == false))
                    _world.RequestThought(diary.TooEarlyThoughtKey);

                ref DiaryOpenRequest request = ref _poolOpenRequest.Add(_world.NewEntity());
                request.Writable = writable;
                request.CompleteStepId = diary.CompleteStepId;
            }
        }

        private bool IsWritable(int entity)
        {
            if (_poolActive.Has(entity)) return true;

            foreach (int progress in _progressFilter)
                return _poolProgress.Get(progress).Finished;

            return true;
        }
    }
}
