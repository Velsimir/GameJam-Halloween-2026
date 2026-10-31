using Game.Scripts.Configs.Audio;
using Game.Scripts.ECS.Components.Audio;
using Leopotam.EcsLite;
using UnityEngine;
using Zenject;

namespace Game.Scripts.ECS.Authoring
{
    [RequireComponent(typeof(Collider))]
    public class AmbientEmitterAuthoring : MonoBehaviour
    {
        [SerializeField] private AmbientEmitterProfile _profile;
        [SerializeField] private Transform _point;

        [Inject] private IEcsWorldProvider _ecsWorldProvider;
        private EcsPackedEntity _entity;

        private void Start()
        {
            if (_profile == null) return;

            EcsWorld world = _ecsWorldProvider.World;
            int entity = world.NewEntity();
            _entity = world.PackEntity(entity);
            ref AmbientEmitter emitter = ref world.GetPool<AmbientEmitter>().Add(entity);
            emitter.Profile = _profile;
            emitter.Point = _point != null ? _point : transform;
            emitter.Timer = -1f;
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
            if (_ecsWorldProvider == null || _ecsWorldProvider.IsAlive == false) return;
            if (other.GetComponentInParent<PlayerAuthoring>() == null) return;
            if (_entity.Unpack(_ecsWorldProvider.World, out int entity) == false) return;

            _ecsWorldProvider.World.GetPool<AmbientEmitter>().Get(entity).Inside = inside;
        }

        private void OnDestroy()
        {
            if (_ecsWorldProvider == null || _ecsWorldProvider.IsAlive == false) return;
            if (_entity.Unpack(_ecsWorldProvider.World, out int entity))
                _ecsWorldProvider.World.DelEntity(entity);
        }
    }
}
