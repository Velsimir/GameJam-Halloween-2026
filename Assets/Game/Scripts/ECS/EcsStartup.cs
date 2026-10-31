using System;
using Game.Scripts.Configs;
using Game.Scripts.Configs.Scenario;
using Game.Scripts.ECS.Systems;
using Game.Scripts.ECS.Systems.Camera;
using Game.Scripts.ECS.Systems.Cursor;
using Game.Scripts.ECS.Systems.DayObjects;
using Game.Scripts.ECS.Systems.DebugHelpers;
using Game.Scripts.ECS.Systems.Doors;
using Game.Scripts.ECS.Systems.Fear;
using Game.Scripts.ECS.Systems.Inspect;
using Game.Scripts.ECS.Systems.Interaction;
using Game.Scripts.ECS.Systems.Items;
using Game.Scripts.ECS.Systems.Objectives;
using Game.Scripts.ECS.Systems.Scenario;
using Game.Scripts.ECS.Systems.Thoughts;
using Game.Scripts.ECS.Systems.Transition;
using Game.Scripts.ECS.Systems.Walk;
using Game.Scripts.Services.DayProgress;
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
        private readonly ScreenFadeView _screenFadeView;
        private readonly ObjectivesView _objectivesView;
        private readonly IDayProgressService _dayProgress;
        private EcsSystems _systems;
        private DayScenario _dayScenario;

        public EcsStartup(IInputService inputService, IEcsWorldProvider ecsWorldProvider, GameConfigs gameConfigs,
            InteractionPromptView interactionPromptView, DialoguePlayer thoughtPlayer, ScreenFadeView screenFadeView,
            ObjectivesView objectivesView, IDayProgressService dayProgress)
        {
            _dayProgress = dayProgress;
            _objectivesView = objectivesView;
            _screenFadeView = screenFadeView;
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
            _dayScenario = _gameConfigs.FindDayScenario(_dayProgress.CurrentDay);
            
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
            _systems.Add(new ScareDebugSystem())
                .Add(new ObjectiveDebugSystem())
                .Add(new ScenarioDebugSystem(_dayScenario));
#endif
        }

        private void AddSimulationSystems()
        {
            _systems
                .Add(new PlayerMovementSystem(_gameConfigs.PlayerConfig))
                .Add(new FirstPersonLookRotationSystem(_gameConfigs.PlayerConfig))
                .Add(new WalkCycleSystem(_gameConfigs.PlayerConfig))
                .Add(new InteractionRaycastSystem())
                .Add(new BedFocusSystem())
                .Add(new InteractionRequestSystem())
                .Add(new ScenarioGateSystem(_gameConfigs.ScenarioConfig, _thoughtPlayer))
                .Add(new InspectSystem())
                .Add(new PickupSystem())
                .Add(new DoorInteractionSystem())
                .Add(new BedSystem())
                .Add(new FadeActionSystem(_gameConfigs.ScreenFadeConfig, _screenFadeView))
                .Add(new KeyHookSystem())
                .Add(new KettleSystem())
                .Add(new FridgeSystem())
                .Add(new MailboxSystem())
                .Add(new StepInteractSystem())
                .Add(new DayScenarioSystem(_dayScenario))
                .Add(new GravitySystem())
                .Add(new FearZoneContactSystem())
                .Add(new FearFactorSystem(_gameConfigs.FearConfig))
                .Add(new ScareRequestSystem(_gameConfigs.FearConfig))
                .Add(new TransitionDoorInteractionSystem())
                .Add(new TransitionSystem(_gameConfigs.ScreenFadeConfig, _screenFadeView));
        }

        private void AddPresentationSystems()
        {
            var thoughtPlayback = new ThoughtPlaybackSystem(_thoughtPlayer);
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
                .Add(new ObjectiveSystem(_objectivesView, _thoughtPlayer, thoughtPlayback))
                .Add(thoughtPlayback);
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
                .Add(new ClearThoughtRequestSystem())
                .Add(new ClearObjectiveShowRequestSystem())
                .Add(new ClearObjectiveCompleteRequestSystem())
                .Add(new ClearScenarioZoneEnteredSystem())
                .Add(new ClearScenarioExternalCompleteSystem())
                .Add(new ClearDayFinishedSystem());
        }
    }
}