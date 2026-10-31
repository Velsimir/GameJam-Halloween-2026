using Game.Scripts.ECS.Components.Doors;
using Game.Scripts.ECS.Components.DayObjects;
using Game.Scripts.ECS.Components.Interaction;
using Game.Scripts.ECS.Extensions;
using Leopotam.EcsLite;

namespace Game.Scripts.ECS.Systems.DayObjects
{
    public class MailboxSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld _world;
        private EcsFilter _filter;
        private EcsPool<Mailbox> _poolMailbox;
        private EcsPool<Door> _poolDoor;

        public void Init(IEcsSystems systems)
        {
            _world = systems.GetWorld();
            _filter = _world.Filter<Mailbox>().Inc<Door>().Inc<InteractionRequest>().End();
            _poolMailbox = _world.GetPool<Mailbox>();
            _poolDoor = _world.GetPool<Door>();
        }

        public void Run(IEcsSystems systems)
        {
            foreach (int entity in _filter)
            {
                if (_poolDoor.Get(entity).IsOpen == false) continue;

                ref var mailbox = ref _poolMailbox.Get(entity);
                string key = string.IsNullOrEmpty(mailbox.LetterKey) ? mailbox.DefaultLetterKey : mailbox.LetterKey;

                _world.RequestThought(key);
                _world.CompleteScenarioStep(mailbox.OpenStepId);
            }
        }
    }
}
