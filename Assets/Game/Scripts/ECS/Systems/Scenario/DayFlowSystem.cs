using System;
using Cysharp.Threading.Tasks;using Game.Scripts.Configs;
using Game.Scripts.Configs.Scenario;
using Game.Scripts.ECS.Components.Input;
using Game.Scripts.ECS.Components.Player;
using Game.Scripts.ECS.Components.Scenario;
using Game.Scripts.Services.Audio;
using Game.Scripts.Services.DayProgress;
using Game.Scripts.Services.Diary;
using Game.Scripts.Services.SceneLoader;
using Game.Scripts.Views;
using Leopotam.EcsLite;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Scripts.ECS.Systems.Scenario
{
    public class DayFlowSystem : IEcsInitSystem, IEcsRunSystem, IEcsDestroySystem
    {
        private readonly GameConfigs _configs;
        private readonly IDayProgressService _dayProgress;
        private readonly IDiaryService _diary;
        private readonly ISceneLoader _sceneLoader;
        private readonly ScreenFadeView _fadeView;
        private readonly EndScreenView _endView;
        private readonly IAudioService _audio;

        private SoundHandle _music;
        private EcsWorld _world;
        private EcsFilter _finishedFilter;
        private EcsFilter _playerFilter;
        private bool _running;

        public DayFlowSystem(GameConfigs configs, IDayProgressService dayProgress, ISceneLoader sceneLoader,
            ScreenFadeView fadeView, EndScreenView endView, IDiaryService diary, IAudioService audio)
        {
            _audio = audio;
            _configs = configs;
            _dayProgress = dayProgress;
            _diary = diary;
            _sceneLoader = sceneLoader;
            _fadeView = fadeView;
            _endView = endView;
        }

        public void Init(IEcsSystems systems)
        {
            _world = systems.GetWorld();
            _finishedFilter = _world.Filter<DayFinished>().End();
            _playerFilter = _world.Filter<PlayerTag>().End();
            _endView.RestartClicked += OnRestart;
            _endView.QuitClicked += OnQuit;
        }

        public void Run(IEcsSystems systems)
        {
            if (_running || _finishedFilter.GetEntitiesCount() == 0) return;

            _running = true;
            BlockPlayer();
            ChangeDayAsync().Forget();
        }

        public void Destroy(IEcsSystems systems)
        {
            _endView.RestartClicked -= OnRestart;
            _endView.QuitClicked -= OnQuit;
        }

        private void BlockPlayer()
        {
            EcsPool<InputBlocked> blocked = _world.GetPool<InputBlocked>();
            EcsPool<DayChanging> changing = _world.GetPool<DayChanging>();

            foreach (int player in _playerFilter)
            {
                if (blocked.Has(player) == false) blocked.Add(player);
                if (changing.Has(player) == false) changing.Add(player);
            }
        }

        private async UniTaskVoid ChangeDayAsync()
        {
            DayFlowConfig flow = _configs.DayFlowConfig;
            ScenesName scene = CurrentScene();

            try
            {
                await FadeAsync(_fadeView, 0f, 1f, flow.DayFadeOut);

                DayScenario next = _configs.FindNextDayScenario(_dayProgress.CurrentDay);
                if (next == null)
                {
                    Debug.Log($"[DayFlow] day {_dayProgress.CurrentDay} was the last, showing the end screen");
                    await ShowEndAsync(flow);
                    return;
                }

                _dayProgress.CurrentDay = next.Day;
                Debug.Log($"[DayFlow] next day {next.Day}, loading {scene}");
                await _sceneLoader.LoadSceneSingleAsync(scene);
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async UniTask ShowEndAsync(DayFlowConfig flow)
        {
            _endView.EnableInput();
            _music = _audio.Play(flow.EndMusic);
            await FadeAsync(_endView, 0f, 1f, flow.EndFadeIn);
        }

        private void OnRestart()
        {
            _music.Stop();
            _dayProgress.Restart();
            _diary.Restart();
            Debug.Log($"[DayFlow] restart from day {_dayProgress.CurrentDay}");
            _sceneLoader.LoadSceneSingleAsync(CurrentScene()).Forget();
        }

        private void OnQuit()
        {
            Debug.Log("[DayFlow] quit");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private static ScenesName CurrentScene()
        {
            if (Enum.TryParse(SceneManager.GetActiveScene().name, out ScenesName scene) && scene != ScenesName.Init)
                return scene;

            return ScenesName.Apartment;
        }

        private static async UniTask FadeAsync(ScreenFadeView view, float from, float to, float duration)
        {
            await FadeAsync(alpha => view.Alpha = alpha, from, to, duration);
        }

        private static async UniTask FadeAsync(EndScreenView view, float from, float to, float duration)
        {
            await FadeAsync(alpha => view.Alpha = alpha, from, to, duration);
        }

        private static async UniTask FadeAsync(Action<float> apply, float from, float to, float duration)
        {
            float timer = 0f;
            while (timer < duration)
            {
                timer += Time.deltaTime;
                apply(Mathf.Lerp(from, to, timer / duration));
                await UniTask.Yield();
                if (Application.isPlaying == false) return;
            }

            apply(to);
        }
    }
}
