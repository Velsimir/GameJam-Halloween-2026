using Game.Scripts.ECS.Components.Anomalies;
using Leopotam.EcsLite;
using UnityEngine;
using Zenject;

namespace Game.Scripts.ECS.Authoring
{
    public class AnomalyAuthoring : MonoBehaviour
    {
        [SerializeField] private GameObject _target;
        [SerializeField] private string _thoughtKey;
        [SerializeField, Min(0.5f)] private float _maxDistance = 8f;
        [SerializeField, Range(5f, 90f)] private float _viewAngle = 35f;

        [Inject] private IEcsWorldProvider _ecsWorldProvider;

        private EcsPackedEntity _entity;
        private bool _hasEntity;

        private void Start()
        {
            GameObject target = _target != null ? _target : gameObject;
            EcsWorld world = _ecsWorldProvider.World;
            int entity = world.NewEntity();

            ref Anomaly anomaly = ref world.GetPool<Anomaly>().Add(entity);
            anomaly.Target = target;
            anomaly.Renderer = target.GetComponentInChildren<Renderer>(true);
            anomaly.ThoughtKey = _thoughtKey;
            anomaly.MaxDistance = _maxDistance;
            anomaly.ViewAngle = _viewAngle;

            _entity = world.PackEntity(entity);
            _hasEntity = true;
        }

        private void OnDestroy()
        {
            if (_hasEntity == false || _ecsWorldProvider == null || _ecsWorldProvider.IsAlive == false) return;

            if (_entity.Unpack(_ecsWorldProvider.World, out int entity))
                _ecsWorldProvider.World.DelEntity(entity);
        }
    }
}
