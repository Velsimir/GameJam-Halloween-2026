using Game.Scripts.Configs;
using UnityEngine;

namespace Game.Scripts.ECS.Components.Items
{
    public struct Pickup
    {
        public ItemDefinition Item;
        public GameObject View;
    }
}
