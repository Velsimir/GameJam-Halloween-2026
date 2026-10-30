using Unity.Cinemachine;

namespace Game.Scripts.ECS.Components.Camera
{
    public struct CinemachineRefs
    {
        public CinemachineMixingCamera CameraMixing;
        public CinemachineCamera[] Cameras;
        public float[] BaseFov;
    }
}
