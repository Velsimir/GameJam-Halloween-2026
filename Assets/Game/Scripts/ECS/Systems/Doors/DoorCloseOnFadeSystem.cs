using Game.Scripts.ECS.Components.Doors;
using Game.Scripts.ECS.Components.Interaction;
using Game.Scripts.Views;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Scripts.ECS.Systems.Doors
{
    public class DoorCloseOnFadeSystem : IEcsInitSystem, IEcsRunSystem
    {
        private const float BlackThreshold = 0.99f;

        private readonly ScreenFadeView _view;

        private EcsFilter _filter;
        private EcsPool<Door> _poolDoor;
        private EcsPool<InteractionPrompt> _poolPrompt;
        private EcsPool<Acting> _poolActing;
        private bool _wasBlack;

        public DoorCloseOnFadeSystem(ScreenFadeView view)
        {
            _view = view;
        }

        public void Init(IEcsSystems systems)
        {
            EcsWorld world = systems.GetWorld();
            _filter = world.Filter<Door>().End();
            _poolDoor = world.GetPool<Door>();
            _poolPrompt = world.GetPool<InteractionPrompt>();
            _poolActing = world.GetPool<Acting>();
        }

        public void Run(IEcsSystems systems)
        {
            bool isBlack = _view.Alpha >= BlackThreshold;

            if (isBlack && _wasBlack == false)
                CloseAll();

            _wasBlack = isBlack;
        }

        private void CloseAll()
        {
            foreach (int entity in _filter)
            {
                ref var door = ref _poolDoor.Get(entity);

                if (door.IsOpen == false)
                    continue;

                door.IsOpen = false;
                door.CurrentAngle = door.CloseAngle;
                door.Pivot.localRotation = door.InitialRotation * Quaternion.Euler(0, door.CloseAngle, 0);

                if (_poolPrompt.Has(entity))
                    _poolPrompt.Get(entity).Key = door.OpenPromptKey;

                if (_poolActing.Has(entity))
                    _poolActing.Del(entity);
            }
        }
    }
}
