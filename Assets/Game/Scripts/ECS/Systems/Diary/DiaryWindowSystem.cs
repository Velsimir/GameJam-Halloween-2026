using Game.Scripts.ECS.Components.Diary;
using Game.Scripts.ECS.Components.Input;
using Game.Scripts.ECS.Components.Player;
using Game.Scripts.Services.Input;
using Game.Scripts.Views;
using Leopotam.EcsLite;

namespace Game.Scripts.ECS.Systems.Diary
{
    public class DiaryWindowSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly IInputService _inputService;
        private readonly DiaryView _view;

        private EcsWorld _world;
        private EcsFilter _requestFilter;
        private EcsFilter _openPlayerFilter;
        private EcsFilter _playerFilter;
        private EcsPool<DiaryOpenRequest> _poolRequest;
        private EcsPool<DiaryOpen> _poolOpen;
        private EcsPool<DiaryClosed> _poolClosed;
        private EcsPool<InputBlocked> _poolBlocked;
        private bool _cancelWasPressed;
        private bool _addedBlock;

        public DiaryWindowSystem(IInputService inputService, DiaryView view)
        {
            _inputService = inputService;
            _view = view;
        }

        public void Init(IEcsSystems systems)
        {
            _world = systems.GetWorld();
            _requestFilter = _world.Filter<DiaryOpenRequest>().End();
            _playerFilter = _world.Filter<PlayerTag>().End();
            _openPlayerFilter = _world.Filter<PlayerTag>().Inc<DiaryOpen>().End();
            _poolRequest = _world.GetPool<DiaryOpenRequest>();
            _poolOpen = _world.GetPool<DiaryOpen>();
            _poolClosed = _world.GetPool<DiaryClosed>();
            _poolBlocked = _world.GetPool<InputBlocked>();
        }

        public void Run(IEcsSystems systems)
        {
            bool cancelPressed = _inputService.Cancel.CurrentValue;
            bool cancelJustPressed = cancelPressed && _cancelWasPressed == false;
            _cancelWasPressed = cancelPressed;

            foreach (int player in _openPlayerFilter)
            {
                if (cancelJustPressed)
                    _view.RequestClose();

                if (_view.TryConsumeClosed(out bool allRead))
                    Close(player, allRead);

                return;
            }

            foreach (int requestEntity in _requestFilter)
            {
                Open(_poolRequest.Get(requestEntity));
                return;
            }
        }

        private void Open(DiaryOpenRequest request)
        {
            foreach (int player in _playerFilter)
            {
                ref DiaryOpen open = ref _poolOpen.Add(player);
                open.CompleteStepId = request.CompleteStepId;
                open.Writable = request.Writable;

                _addedBlock = _poolBlocked.Has(player) == false;
                if (_addedBlock)
                    _poolBlocked.Add(player);

                _view.Open(request.Writable);
                return;
            }
        }

        private void Close(int player, bool allRead)
        {
            DiaryOpen open = _poolOpen.Get(player);
            _poolOpen.Del(player);

            if (_addedBlock && _poolBlocked.Has(player))
                _poolBlocked.Del(player);

            _addedBlock = false;

            ref DiaryClosed closed = ref _poolClosed.Add(_world.NewEntity());
            closed.CompleteStepId = open.CompleteStepId;
            closed.Writable = open.Writable;
            closed.AllRead = allRead;
        }
    }
}
