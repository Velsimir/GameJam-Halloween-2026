using Game.Scripts.ECS.Components.Diary;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Scripts.ECS.Authoring
{
    public class DiaryAuthoring : InteractionObjectAuthoring
    {
        [SerializeField] private string _completeStepId = "diary";
        [SerializeField] private string _tooEarlyThoughtKey = "ui.thought.diary.too_early";

        protected override string DefaultPromptKey => "ui.interact.read";

        protected override void AddSpecificTag(EcsWorld world, int entity)
        {
            ref DiaryObject diary = ref world.GetPool<DiaryObject>().Add(entity);
            diary.CompleteStepId = _completeStepId;
            diary.TooEarlyThoughtKey = _tooEarlyThoughtKey;
        }
    }
}
