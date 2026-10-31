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
                Vector3 standPosition = offset.Position;
                Quaternion standRotation = Quaternion.Euler(offset.Rotation);

                Quaternion lyingRotation = Quaternion.Euler(lying.LyingPitch, lying.TurnAngle, 0f);
                Quaternion sitRotation = Quaternion.Euler(lying.SitPitch, lying.TurnAngle, 0f);

                Vector3 sitPosition = Vector3.Lerp(lyingPosition, standPosition, lying.RisePhase) + Vector3.up * lying.RiseHeight;
                Vector3 turnPosition = Vector3.Lerp(lyingPosition, standPosition, lying.RisePhase + lying.TurnPhase);

                float progress = Mathf.Clamp01(lying.Progress);
                float riseEnd = lying.RisePhase;
                float turnEnd = Mathf.Min(lying.RisePhase + lying.TurnPhase, 0.99f);

                if (progress < riseEnd)
                {
                    float t = Mathf.SmoothStep(0f, 1f, progress / riseEnd);
                    offset.Position = Vector3.Lerp(lyingPosition, sitPosition, t);
                    offset.Rotation = Quaternion.Slerp(lyingRotation, sitRotation, t).eulerAngles;
                }
                else if (progress < turnEnd)
                {
                    float t = Mathf.SmoothStep(0f, 1f, (progress - riseEnd) / (turnEnd - riseEnd));
                    offset.Position = Vector3.Lerp(sitPosition, turnPosition, t);
                    offset.Rotation = Quaternion.Slerp(sitRotation, standRotation, t).eulerAngles;
                }
                else
                {
                    float t = Mathf.SmoothStep(0f, 1f, (progress - turnEnd) / (1f - turnEnd));
                    offset.Position = Vector3.Lerp(turnPosition, standPosition, t);
                    offset.Rotation = standRotation.eulerAngles;
                }
            }
        }
    }
}
