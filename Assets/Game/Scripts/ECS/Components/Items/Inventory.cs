using System.Collections.Generic;
using Game.Scripts.Configs;
using Leopotam.EcsLite;

namespace Game.Scripts.ECS.Components.Items
{
    public struct Inventory : IEcsAutoReset<Inventory>
    {
        public List<ItemDefinition> Items;

        public void AutoReset(ref Inventory component)
        {
            if (component.Items == null)
                component.Items = new List<ItemDefinition>();
            else
                component.Items.Clear();
        }
    }
}
