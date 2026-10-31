using System.Collections.Generic;
using Game.Scripts.ECS.Components.Input;
using Game.Scripts.ECS.Components.Objectives;
using Game.Scripts.ECS.Components.Player;
using Game.Scripts.ECS.Components.Transition;
using Game.Scripts.ECS.Extensions;
using Game.Scripts.ECS.Systems.Thoughts;
using Game.Scripts.Views;
using Leopotam.EcsLite;
using Velsi.Typecraft.Dialogue;

namespace Game.Scripts.ECS.Systems.Objectives
{
    public class ObjectiveSystem : IEcsInitSystem, IEcsRunSystem, IEcsDestroySystem
    {
        private enum Stage
        {
            WaitThought,
            WaitCard,
            Flying,
            Listed,
            Completing
        }

        private class Entry
        {
            public string Key;
            public DialogueAsset Thought;
            public Stage Stage;
            public bool CompleteOnLand;
        }

        private readonly ObjectivesView _view;
        private readonly DialoguePlayer _player;
        private readonly ThoughtPlaybackSystem _thoughts;
        private readonly List<Entry> _entries = new List<Entry>();
        private readonly List<DialogueAsset> _finished = new List<DialogueAsset>();

        private EcsWorld _world;
        private EcsFilter _showFilter;
        private EcsFilter _completeFilter;
        private EcsFilter _playerFilter;
        private EcsPool<ObjectiveShowRequest> _showPool;
        private EcsPool<ObjectiveCompleteRequest> _completePool;
        private EcsPool<InputBlocked> _blockedPool;
        private EcsPool<Transitioning> _transitioningPool;

        public ObjectiveSystem(ObjectivesView view, DialoguePlayer player, ThoughtPlaybackSystem thoughts)
        {
            _view = view;
            _player = player;
            _thoughts = thoughts;
        }

        public void Init(IEcsSystems systems)
        {
            _world = systems.GetWorld();
            _showFilter = _world.Filter<ObjectiveShowRequest>().End();
            _completeFilter = _world.Filter<ObjectiveCompleteRequest>().End();
            _playerFilter = _world.Filter<PlayerTag>().End();
            _showPool = _world.GetPool<ObjectiveShowRequest>();
            _completePool = _world.GetPool<ObjectiveCompleteRequest>();
            _blockedPool = _world.GetPool<InputBlocked>();
            _transitioningPool = _world.GetPool<Transitioning>();
            _thoughts.Finished += OnThoughtFinished;
        }

        public void Run(IEcsSystems systems)
        {
            foreach (int entity in _showFilter)
            {
                ref ObjectiveShowRequest request = ref _showPool.Get(entity);
                Show(request.Key, request.ThoughtAsset);
            }

            foreach (int entity in _completeFilter)
                Complete(_completePool.Get(entity).Key);

            ApplyFinishedThoughts();

            bool blocked = IsBlocked();
            _view.SetVisible(!blocked);
            if (!blocked) TryStartCard();
        }

        public void Destroy(IEcsSystems systems)
        {
            if (_thoughts != null) _thoughts.Finished -= OnThoughtFinished;
            _entries.Clear();
            _finished.Clear();
        }

        private void OnThoughtFinished(DialogueAsset asset)
        {
            _finished.Add(asset);
        }

        private void Show(string key, DialogueAsset thought)
        {
            if (Find(key) != null) return;

            var entry = new Entry { Key = key, Thought = thought };
            _entries.Add(entry);

            if (thought == null)
            {
                entry.Stage = Stage.WaitCard;
                return;
            }

            entry.Stage = Stage.WaitThought;
            _world.RequestThought(thought);
        }

        private void Complete(string key)
        {
            Entry entry = Find(key);
            if (entry == null) return;

            switch (entry.Stage)
            {
                case Stage.WaitThought:
                case Stage.WaitCard:
                    _entries.Remove(entry);
                    break;
                case Stage.Flying:
                    entry.CompleteOnLand = true;
                    break;
                case Stage.Listed:
                    BeginComplete(entry);
                    break;
            }
        }

        private void ApplyFinishedThoughts()
        {
            if (_finished.Count == 0) return;

            foreach (DialogueAsset asset in _finished)
            {
                foreach (Entry entry in _entries)
                {
                    if (entry.Stage == Stage.WaitThought && entry.Thought == asset)
                        entry.Stage = Stage.WaitCard;
                }
            }

            _finished.Clear();
        }

        private void TryStartCard()
        {
            if (_player.IsPlaying) return;

            foreach (Entry entry in _entries)
            {
                if (entry.Stage == Stage.Flying) return;
            }

            foreach (Entry entry in _entries)
            {
                if (entry.Stage != Stage.WaitCard) continue;

                entry.Stage = Stage.Flying;
                Fly(entry);
                return;
            }
        }

        private void Fly(Entry entry)
        {
            _view.PlayIntro(entry.Key, () =>
            {
                entry.Stage = Stage.Listed;
                if (entry.CompleteOnLand) BeginComplete(entry);
            });
        }

        private void BeginComplete(Entry entry)
        {
            entry.Stage = Stage.Completing;
            _view.Complete(entry.Key, () => _entries.Remove(entry));
        }

        private Entry Find(string key)
        {
            foreach (Entry entry in _entries)
            {
                if (entry.Key == key) return entry;
            }

            return null;
        }

        private bool IsBlocked()
        {
            foreach (int player in _playerFilter)
                return _blockedPool.Has(player) && _transitioningPool.Has(player) == false;

            return false;
        }
    }
}
