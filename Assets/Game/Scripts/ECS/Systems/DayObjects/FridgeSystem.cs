using Game.Scripts.ECS.Components.Doors;
using Game.Scripts.ECS.Components.DayObjects;
using Game.Scripts.ECS.Components.Interaction;
using Game.Scripts.ECS.Extensions;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Scripts.ECS.Systems.DayObjects
{
    public class FridgeSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld _world;
        private EcsFilter _filter;
        private EcsPool<Fridge> _poolFridge;
        private EcsPool<Door> _poolDoor;
        private EcsPool<Locked> _poolLocked;

        public void Init(IEcsSystems systems)
        {
            _world = systems.GetWorld();
            _filter = _world.Filter<Fridge>().Inc<Door>().Inc<InteractionRequest>().End();
            _poolFridge = _world.GetPool<Fridge>();
            _poolDoor = _world.GetPool<Door>();
            _poolLocked = _world.GetPool<Locked>();
        }

        public void Run(IEcsSystems systems)
        {
            foreach (int entity in _filter)
            {
                if (_poolLocked.Has(entity) || _poolDoor.Get(entity).IsOpen == false) continue;

                ref var fridge = ref _poolFridge.Get(entity);

                if (fridge.Stocked)
                {
                    _world.RequestThought(fridge.FullThoughtKey);
                    continue;
                }

                if (_world.TryRemoveItem(fridge.Groceries) == false)
                {
                    _world.RequestThought(fridge.EmptyThoughtKey);
                    continue;
                }

                fridge.Stocked = true;
                SetContents(fridge.Contents);
                _world.RequestThought(fridge.StockedThoughtKey);
                _world.CompleteScenarioStep(fridge.StockStepId);
            }
        }

        private static void SetContents(GameObject[] contents)
        {
            if (contents == null) return;

            foreach (GameObject content in contents)
            {
                if (content != null) content.SetActive(true);
            }
        }
    }
}
