using Game.Scripts.ECS.Components.Anomalies;
using Game.Scripts.ECS.Components.Input;
using Game.Scripts.ECS.Components.Player;
using Game.Scripts.ECS.Extensions;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Scripts.ECS.Systems.Anomalies
{
    public class AnomalySightSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly RaycastHit[] _hits = new RaycastHit[16];

        private EcsWorld _world;
        private EcsFilter _anomalyFilter;
        private EcsFilter _playerFilter;
        private EcsPool<Anomaly> _poolAnomaly;
        private EcsPool<PlayerRefs> _poolRefs;
        private EcsPool<InputBlocked> _poolBlocked;

        public void Init(IEcsSystems systems)
        {
            _world = systems.GetWorld();
            _anomalyFilter = _world.Filter<Anomaly>().End();
            _playerFilter = _world.Filter<PlayerTag>().Inc<PlayerRefs>().End();
            _poolAnomaly = _world.GetPool<Anomaly>();
            _poolRefs = _world.GetPool<PlayerRefs>();
            _poolBlocked = _world.GetPool<InputBlocked>();
        }

        public void Run(IEcsSystems systems)
        {
            foreach (int player in _playerFilter)
            {
                if (_poolBlocked.Has(player))
                    continue;

                PlayerRefs refs = _poolRefs.Get(player);

                foreach (int entity in _anomalyFilter)
                {
                    ref var anomaly = ref _poolAnomaly.Get(entity);

                    if (anomaly.Seen || anomaly.Target == null || anomaly.Target.activeInHierarchy == false)
                        continue;

                    if (IsSeen(refs, anomaly) == false)
                        continue;

                    anomaly.Seen = true;
                    _world.RequestThought(anomaly.ThoughtKey);
                }
            }
        }

        private bool IsSeen(PlayerRefs refs, Anomaly anomaly)
        {
            Transform head = refs.HeadTransform;
            Vector3 origin = head.position;
            Vector3 point = anomaly.Renderer != null ? anomaly.Renderer.bounds.center : anomaly.Target.transform.position;
            Vector3 toPoint = point - origin;
            float distance = toPoint.magnitude;

            if (distance > anomaly.MaxDistance || distance < 0.01f)
                return false;

            if (Vector3.Angle(head.forward, toPoint) > anomaly.ViewAngle)
                return false;

            int count = Physics.RaycastNonAlloc(origin, toPoint / distance, _hits, distance, ~0, QueryTriggerInteraction.Ignore);

            for (int i = 0; i < count; i++)
            {
                Transform other = _hits[i].collider.transform;

                if (other.IsChildOf(anomaly.Target.transform) || anomaly.Target.transform.IsChildOf(other))
                    continue;

                if (refs.CharacterController != null && other.IsChildOf(refs.CharacterController.transform))
                    continue;

                return false;
            }

            return true;
        }
    }
}
