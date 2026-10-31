using System;
using System.Collections.Generic;
using Game.Scripts.ECS.Components.Thoughts;
using Leopotam.EcsLite;
using Velsi.Typecraft.Dialogue;

namespace Game.Scripts.ECS.Systems.Thoughts
{
    public class ThoughtPlaybackSystem : IEcsInitSystem, IEcsRunSystem, IEcsDestroySystem
    {
        private readonly DialoguePlayer _player;
        private readonly Queue<DialogueAsset> _queue = new Queue<DialogueAsset>();

        public event Action<DialogueAsset> Finished;

        private EcsFilter _filter;
        private EcsPool<ThoughtRequest> _pool;
        private DialogueAsset _current;
        private bool _starting;

        public ThoughtPlaybackSystem(DialoguePlayer player)
        {
            _player = player;
        }

        public void Init(IEcsSystems systems)
        {
            EcsWorld world = systems.GetWorld();
            _filter = world.Filter<ThoughtRequest>().End();
            _pool = world.GetPool<ThoughtRequest>();
            _player.Ended += OnEnded;
        }

        public void Run(IEcsSystems systems)
        {
            foreach (int entity in _filter)
            {
                ref ThoughtRequest request = ref _pool.Get(entity);
                Handle(request.Asset, request.Interrupt);
            }
        }

        public void Destroy(IEcsSystems systems)
        {
            if (_player != null) _player.Ended -= OnEnded;
            _queue.Clear();
        }

        private void Handle(DialogueAsset asset, bool interrupt)
        {
            if (asset == null) return;

            if (interrupt)
            {
                foreach (DialogueAsset dropped in _queue) Finished?.Invoke(dropped);
                _queue.Clear();
                Play(asset);
                return;
            }

            if (!_player.IsPlaying || asset == _current)
            {
                Play(asset);
                return;
            }

            if (!_queue.Contains(asset)) _queue.Enqueue(asset);
        }

        private void Play(DialogueAsset asset)
        {
            DialogueAsset previous = _player.IsPlaying ? _current : null;
            _starting = true;
            _current = asset;
            _player.Play(asset);
            _starting = false;

            if (previous != null && previous != asset) Finished?.Invoke(previous);

            if (!_player.IsPlaying)
            {
                Finished?.Invoke(asset);
                PlayNext();
            }
        }

        private void PlayNext()
        {
            _current = null;
            if (_queue.Count > 0) Play(_queue.Dequeue());
        }

        private void OnEnded()
        {
            if (_starting) return;
            if (_current != null) Finished?.Invoke(_current);
            PlayNext();
        }
    }
}
