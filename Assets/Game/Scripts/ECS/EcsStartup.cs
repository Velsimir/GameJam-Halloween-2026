using System;
using Game.Scripts.Configs;
using Game.Scripts.Configs.Scenario;
using Game.Scripts.ECS.Systems;
using Game.Scripts.ECS.Systems.Anomalies;
using Game.Scripts.ECS.Systems.Audio;
using Game.Scripts.ECS.Systems.Camera;
using Game.Scripts.ECS.Systems.Cursor;
using Game.Scripts.ECS.Systems.DayObjects;
using Game.Scripts.ECS.Systems.DebugHelpers;
using Game.Scripts.ECS.Systems.Diary;
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
using Game.Scripts.Services.Audio;
using Game.Scripts.Services.Diary;
using Game.Scripts.Services.Input;
using Game.Scripts.Services.SceneLoader;
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
        private readonly DayIntroView _dayIntroView;
        private readonly EndScreenView _endScreenView;
        private readonly ISceneLoader _sceneLoader;
        private readonly IAudioService _audio;
        private readonly IDiaryService _diary;
        private readonly DiaryView _diaryView;
        private EcsSystems _systems;
        private DayScenario _dayScenario;

        public EcsStartup(IInputService inputService, IEcsWorldProvider ecsWorldProvider, GameConfigs gameConfigs,
            InteractionPromptView interactionPromptView, DialoguePlayer thoughtPlayer, ScreenFadeView screenFadeView,
            ObjectivesView objectivesView, IDayProgressService dayProgress, DayIntroView dayIntroView,
            EndScreenView endScreenView, ISceneLoader sceneLoader, IAudioService audio, IDiaryService diary,
            DiaryView diaryView)
        {
            _dayIntroView = dayIntroView;
            _endScreenView = endScreenView;
            _sceneLoader = sceneLoader;
            _audio = audio;
            _diary = diary;
            _diaryView = diaryView;
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
            _audio.StopAll(0.3f);
            _ecsWorldProvider.Reset();
        }

        private void CreateSystems()
        {
            _systems = new EcsSystems(_ecsWorldProvider.World);
            _dayScenario = _gameConfigs.FindDayScenario(_dayProgress.CurrentDay);
            if (_dayScenario != null)
                _diary.BeginDay(_dayScenario.Day, _dayScenario.Diary);
            
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
                .Add(new ScenarioDebugSystem(_dayScenario))
                .Add(new DayDebugSystem(_dayProgress))
                .Add(new DiaryDebugSystem());
#endif
            _systems.Add(new DayIntroSystem(_gameConfigs.DayFlowConfig, _dayIntroView, _audio,
                _dayProgress.CurrentDay, _dayScenario != null));
        }

        private void AddSimulationSystems()
        {
            _systems
                .Add(new PlayerMovementSystem(_gameConfigs.PlayerConfig))
                .Add(new FirstPersonLookRotationSystem(_gameConfigs.PlayerConfig))
                .Add(new WalkCycleSystem(_gameConfigs.PlayerConfig))
                .Add(new FootstepDetectSystem())
                .Add(new InteractionRaycastSystem())
                .Add(new AnomalySightSystem())
                .Add(new BedFocusSystem())
                .Add(new InteractionRequestSystem())
                .Add(new ScenarioGateSystem(_gameConfigs.ScenarioConfig, _thoughtPlayer))
                .Add(new DiaryInteractionSystem(_diary))
                .Add(new DiaryWindowSystem(_inputService, _diaryView))
                .Add(new DiaryClosedSystem(_dayScenario))
                .Add(new InspectSystem())
                .Add(new PickupSystem())
                .Add(new DoorInteractionSystem(_audio))
                .Add(new BedSystem(_gameConfigs.AudioConfig, _audio, _dayProgress.CurrentDay))
                .Add(new FadeActionSystem(_gameConfigs.ScreenFadeConfig, _screenFadeView, _audio))
                .Add(new KeyHookSystem())
                .Add(new KettleSystem(_audio))
                .Add(new FridgeSystem())
                .Add(new MailboxSystem())
                .Add(new StepInteractSystem(_dayScenario))
                .Add(new PendingStepCompleteSystem())
                .Add(new DayScenarioSystem(_dayScenario, _diary, _audio))
                .Add(new DayFlowSystem(_gameConfigs, _dayProgress, _sceneLoader, _screenFadeView, _endScreenView, _diary, _audio))
                .Add(new GravitySystem())
                .Add(new FearZoneContactSystem())
                .Add(new FearFactorSystem(_gameConfigs.FearConfig))
                .Add(new ScareRequestSystem(_gameConfigs.FearConfig))
                .Add(new TransitionDoorInteractionSystem())
                .Add(new TransitionSystem(_gameConfigs.ScreenFadeConfig, _gameConfigs.DayFlowConfig, _screenFadeView, _audio));
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
                .Add(new DoorCloseOnFadeSystem(_screenFadeView))
                .Add(new DoorAnimationSystem())
                .Add(new BackgroundAudioSystem(_gameConfigs.AudioConfig, _audio))
                .Add(new FootstepAudioSystem(_gameConfigs.AudioConfig, _audio))
                .Add(new FearBreathSystem(_gameConfigs.AudioConfig, _gameConfigs.FearConfig, _audio))
                .Add(new AmbientEmitterSystem(_gameConfigs.FearConfig, _audio, _dayProgress.CurrentDay))
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
                .Add(new ClearFootstepEventSystem())
                .Add(new ClearInteractionRequestSystem())
                .Add(new ClearDiaryOpenRequestSystem())
                .Add(new ClearDiaryClosedSystem())
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