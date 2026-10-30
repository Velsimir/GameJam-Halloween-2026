using System;
using Game.Scripts.Configs;
using Game.Scripts.ECS.Systems;
using Game.Scripts.ECS.Systems.Camera;
using Game.Scripts.ECS.Systems.Cursor;
using Game.Scripts.ECS.Systems.DebugHelpers;
using Game.Scripts.ECS.Systems.Doors;
using Game.Scripts.ECS.Systems.Fear;
using Game.Scripts.ECS.Systems.Inspect;
using Game.Scripts.ECS.Systems.Interaction;
using Game.Scripts.ECS.Systems.Items;
using Game.Scripts.ECS.Systems.Thoughts;
using Game.Scripts.ECS.Systems.Walk;
using Game.Scripts.Services.Input;
using Game.Scripts.Views;
using Leopotam.EcsLite;
using Velsi.Typecraft.Dialogue;
using Zenject;

namespace Game.Scripts.ECS
{
    public class EcsStartup : IInitializable, ITickable, IDisposable
    {
        private readonly IInputService _inputService;

        private readonly IEcsWorldProvider _ecsWorldProvider;
        private readonly GameConfigs _gameConfigs;
        private readonly InteractionPromptView _interactionPromptView;
        private readonly DialoguePlayer _thoughtPlayer;
        private EcsSystems _systems;

        public EcsStartup(IInputService inputService, IEcsWorldProvider ecsWorldProvider, GameConfigs gameConfigs,
            InteractionPromptView interactionPromptView, DialoguePlayer thoughtPlayer)
        {
            _thoughtPlayer = thoughtPlayer;
            _interactionPromptView = interactionPromptView;
            _inputService = inputService;
            _ecsWorldProvider = ecsWorldProvider;
            _gameConfigs = gameConfigs;
        }

        public void Initialize()
        {
            CreateSystems();
        }
        
        public void Tick()
        {
            _systems.Run();
        }

        public void Dispose()
        {
            _systems.Destroy();
            _ecsWorldProvider.World.Destroy();
        }

        private void CreateSystems()
        {
            _systems = new EcsSystems(_ecsWorldProvider.World);
            
            AddInputSystems();
            AddSimulationSystems();
            AddPresentationSystems();
            AddDebugSystems();
            AddCleanupSystems();
            
            _systems.Init();
        }
        
        private void AddInputSystems()
        {
            _systems.Add(new PlayerInputSystem(_inputService));
#if UNITY_EDITOR
            _systems.Add(new ScareDebugSystem());
#endif
        }

        private void AddSimulationSystems()
        {
            _systems
                .Add(new PlayerMovementSystem(_gameConfigs.PlayerConfig))
                .Add(new FirstPersonLookRotationSystem(_gameConfigs.PlayerConfig))
                .Add(new WalkCycleSystem(_gameConfigs.PlayerConfig))
                .Add(new InteractionRaycastSystem())
                .Add(new InteractionRequestSystem())
                .Add(new InspectSystem())
                .Add(new PickupSystem())
                .Add(new GravitySystem())
                .Add(new FearZoneContactSystem())
                .Add(new FearFactorSystem(_gameConfigs.FearConfig))
                .Add(new ScareRequestSystem(_gameConfigs.FearConfig))
                .Add(new DoorInteractionSystem());
        }

        private void AddPresentationSystems()
        {
            _systems
                .Add(new ClearCameraOffsetSystem())
                .Add(new CameraWalkBobSystem())
                .Add(new CameraFearShakeSystem())
                .Add(new CameraScareShakeSystem(_gameConfigs.FearConfig))
                .Add(new FearPostProcessSystem(_gameConfigs.FearConfig))
                .Add(new CameraCutsceneSystem())
                .Add(new CameraApplySystem())
                .Add(new PickupViewSystem())
                .Add(new OutlineViewSystem())
                .Add(new DoorAnimationSystem())
                .Add(new CursorLockSystem())
                .Add(new InteractionPromptViewSystem(_interactionPromptView))
                .Add(new ThoughtPlaybackSystem(_thoughtPlayer));
        }

        private void AddDebugSystems()
        {
#if UNITY_EDITOR
            _systems.Add(new Leopotam.EcsLite.UnityEditor.EcsWorldDebugSystem())
                .Add(new Leopotam.EcsLite.UnityEditor.EcsSystemsDebugSystem())
                .Add(new DebugDrawSystem())
                .Add(new InputBlockToggleDebugSystem());
#endif
        }

        private void AddCleanupSystems()
        {
            _systems
                .Add(new ClearInteractionRequestSystem())
                .Add(new ClearScareRequestSystem())
                .Add(new ClearThoughtRequestSystem());
        }
    }
}