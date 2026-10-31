using Game.Scripts.Configs;
using Game.Scripts.ECS.Components.DayObjects;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Scripts.ECS.Authoring
{
    public class FridgeAuthoring : DoorAuthoring
    {
        [SerializeField] private ItemDefinition _groceries;
        [SerializeField] private GameObject[] _contents;
        [SerializeField] private string _emptyThoughtKey = "ui.thought.fridge_empty";
        [SerializeField] private string _stockedThoughtKey = "ui.thought.fridge_stocked";
        [SerializeField] private string _fullThoughtKey = "ui.thought.fridge_full";
        [SerializeField] private string _stockStepId;

        protected override void AddSpecificTag(EcsWorld world, int entity)
        {
            base.AddSpecificTag(world, entity);

            ref var fridge = ref world.GetPool<Fridge>().Add(entity);
            fridge.Groceries = _groceries;
            fridge.Contents = _contents;
            fridge.EmptyThoughtKey = _emptyThoughtKey;
            fridge.StockedThoughtKey = _stockedThoughtKey;
            fridge.FullThoughtKey = _fullThoughtKey;
            fridge.StockStepId = _stockStepId;

            if (_contents == null) return;

            foreach (GameObject content in _contents)
            {
                if (content != null) content.SetActive(false);
            }
        }
    }
}
