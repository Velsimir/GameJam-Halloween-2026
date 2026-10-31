using Game.Scripts.ECS.Components.Camera;
using Game.Scripts.ECS.Components.DayObjects;
using Game.Scripts.ECS.Components.Player;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Scripts.ECS.Systems.Camera
{
    public class CameraCutsceneSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsFilter _filter;
        private EcsPool<CameraOffset> _cameraOffsetPool;
        private EcsPool<PlayerRefs> _refsPool;
        private EcsPool<LyingInBed> _lyingPool;

        public void Init(IEcsSystems systems)
        {
            EcsWorld world = systems.GetWorld();
            _filter = world.Filter<PlayerTag>().Inc<CameraOffset>().Inc<CutsceneControlled>().Inc<PlayerRefs>().Inc<LyingInBed>().End();
            _cameraOffsetPool = world.GetPool<CameraOffset>();
            _refsPool = world.GetPool<PlayerRefs>();
            _lyingPool = world.GetPool<LyingInBed>();
        }

        public void Run(IEcsSystems systems)
        {
            foreach (int entity in _filter)
            {
                ref var lying = ref _lyingPool.Get(entity);
                if (lying.Look == null) continue;

                Transform parent = _refsPool.Get(entity).CameraView.parent;
                ref var offset = ref _cameraOffsetPool.Get(entity);

                Vector3 lyingPosition = parent.InverseTransformPoint(lying.Look.position);
                Quaternion lyingRotation = Quaternion.Inverse(parent.rotation) * lying.Look.rotation;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(lying.Progress));

                offset.Position = Vector3.Lerp(lyingPosition, offset.Position, t);
                offset.Rotation = Quaternion.Slerp(lyingRotation, Quaternion.Euler(offset.Rotation), t).eulerAngles;
            }
        }
    }
}
