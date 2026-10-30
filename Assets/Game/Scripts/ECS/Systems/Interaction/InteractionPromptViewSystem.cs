using Game.Scripts.ECS.Components.Input;
using Game.Scripts.ECS.Components.Interaction;
using Game.Scripts.ECS.Components.Player;
using Game.Scripts.Views;
using Leopotam.EcsLite;

namespace Game.Scripts.ECS.Systems.Interaction
{
    public class InteractionPromptViewSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly InteractionPromptView _view;

        private EcsFilter _playerFilter;
        private EcsFilter _focusedFilter;
        private EcsPool<InputBlocked> _blockedPool;
        private EcsPool<InteractionPrompt> _promptPool;
        private string _shownKey;
        private bool? _crosshairVisible;

        public InteractionPromptViewSystem(InteractionPromptView view)
        {
            _view = view;
        }

        public void Init(IEcsSystems systems)
        {
            EcsWorld world = systems.GetWorld();
            _playerFilter = world.Filter<PlayerTag>().End();
            _focusedFilter = world.Filter<Focused>().Inc<InteractionPrompt>().End();
            _blockedPool = world.GetPool<InputBlocked>();
            _promptPool = world.GetPool<InteractionPrompt>();
        }

        public void Run(IEcsSystems systems)
        {
            bool blocked = false;
            foreach (int player in _playerFilter)
            {
                blocked = _blockedPool.Has(player);
                break;
            }

            bool crosshairVisible = !blocked;
            if (_crosshairVisible != crosshairVisible)
            {
                _crosshairVisible = crosshairVisible;
                _view.SetCrosshairVisible(crosshairVisible);
            }

            string key = null;
            if (!blocked)
            {
                foreach (int entity in _focusedFilter)
                {
                    key = _promptPool.Get(entity).Key;
                    break;
                }
            }

            if (string.IsNullOrEmpty(key)) key = null;
            if (key == _shownKey) return;

            if (key != null) _view.Show(key);
            else _view.Hide();
            _shownKey = key;
        }
    }
}
