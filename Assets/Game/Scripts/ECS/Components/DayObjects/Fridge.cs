using Game.Scripts.Configs;
using UnityEngine;

namespace Game.Scripts.ECS.Components.DayObjects
{
    public struct Fridge
    {
        public ItemDefinition Groceries;
        public GameObject[] Contents;
        public bool Stocked;
        public string EmptyThoughtKey;
        public string StockedThoughtKey;
        public string FullThoughtKey;
        public string StockStepId;
    }
}
