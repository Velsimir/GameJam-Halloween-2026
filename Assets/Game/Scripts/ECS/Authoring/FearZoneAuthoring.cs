using Game.Scripts.ECS.Components.Fear;
using Leopotam.EcsLite;
using UnityEngine;
using Zenject;

namespace Game.Scripts.ECS.Authoring
{
    [RequireComponent(typeof(Collider))]
    public class FearZoneAuthoring : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)] private float _target = 0.8f;

        [Inject] private IEcsWorldProvider _ecsWorldProvider;
        private EcsPackedEntity _entity;

        private void Start()
        {
            EcsWorld world = _ecsWorldProvider.World;
            int entity = world.NewEntity();
            _entity = world.PackEntity(entity);
            ref FearZone zone = ref world.GetPool<FearZone>().Add(entity);
            zone.Target = _target;
        }

        private void OnTriggerEnter(Collider other)
        {
            SetInside(other, true);
        }

        private void OnTriggerExit(Collider other)
        {
            SetInside(other, false);
        }

        private void SetInside(Collider other, bool inside)
        {
            if (_ecsWorldProvider == null) return;
            if (other.GetComponentInParent<PlayerAuthoring>() == null) return;

            EcsWorld world = _ecsWorldProvider.World;
            if (!_entity.Unpack(world, out int entity)) return;
            world.GetPool<FearZone>().Get(entity).Inside = inside;
        }

        private void OnDestroy()
        {
            if (_ecsWorldProvider == null || _ecsWorldProvider.IsAlive == false) return;
            if (_entity.Unpack(_ecsWorldProvider.World, out int entity))
                _ecsWorldProvider.World.DelEntity(entity);
        }
    }
}
