using Game.Scripts.ECS.Components.Scenario;
using Game.Scripts.Services.Scenario;
using Leopotam.EcsLite;
using UnityEngine;
using Zenject;

namespace Game.Scripts.ECS.Authoring
{
    [DefaultExecutionOrder(100)]
    [DisallowMultipleComponent]
    public class ScenarioTargetAuthoring : MonoBehaviour
    {
        [SerializeField] private string _id;
        [SerializeField] private bool _gated = true;
        [SerializeField] private GameObject _toggleTarget;

        [Inject] private IEcsWorldProvider _ecsWorldProvider;

        private EcsPackedEntity _entity;
        private bool _ownsEntity;
        private bool _hasEntity;

        public string Id => _id;

        private void Awake()
        {
            ScenarioTargetRegistry.Register(_id, ToggleTarget);
        }

        private void Start()
        {
            EcsWorld world = _ecsWorldProvider.World;
            InteractionObjectAuthoring interaction = GetComponent<InteractionObjectAuthoring>();

            int entity;
            if (interaction != null && interaction.Entity.Unpack(world, out entity))
            {
                _ownsEntity = false;
            }
            else
            {
                if (interaction != null)
                    Debug.LogWarning($"[Scenario] {name}: interaction entity not ready, target '{_id}' gets own entity", this);

                entity = world.NewEntity();
                _ownsEntity = true;
            }

            ref ScenarioTarget target = ref world.GetPool<ScenarioTarget>().Add(entity);
            target.Id = _id;
            target.Gated = _gated;

            if (_gated)
                world.GetPool<ScenarioLocked>().Add(entity);

            _entity = world.PackEntity(entity);
            _hasEntity = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            TryMarkZoneEntered(other);
        }

        private void OnTriggerStay(Collider other)
        {
            TryMarkZoneEntered(other);
        }

        private void OnDestroy()
        {
            ScenarioTargetRegistry.Unregister(_id, ToggleTarget);

            if (_hasEntity == false || _ownsEntity == false || _ecsWorldProvider == null) return;
            if (_ecsWorldProvider.IsAlive == false) return;

            if (_entity.Unpack(_ecsWorldProvider.World, out int entity))
                _ecsWorldProvider.World.DelEntity(entity);
        }

        private void TryMarkZoneEntered(Collider other)
        {
            if (_hasEntity == false || _ownsEntity == false || _ecsWorldProvider == null) return;

            EcsWorld world = _ecsWorldProvider.World;
            if (_entity.Unpack(world, out int entity) == false) return;
            if (world.GetPool<ScenarioActive>().Has(entity) == false) return;

            EcsPool<ScenarioZoneEntered> pool = world.GetPool<ScenarioZoneEntered>();
            if (pool.Has(entity)) return;
            if (other.GetComponentInParent<PlayerAuthoring>() == null) return;

            pool.Add(entity);
        }

        private GameObject ToggleTarget => _toggleTarget != null ? _toggleTarget : gameObject;
    }
}
