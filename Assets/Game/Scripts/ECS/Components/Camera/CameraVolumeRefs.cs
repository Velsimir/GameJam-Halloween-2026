using UnityEngine.Rendering.Universal;

namespace Game.Scripts.ECS.Components.Camera
{
    public struct CameraVolumeRefs
    {
        public Vignette Vignette;
        public FilmGrain FilmGrain;
        public ChromaticAberration ChromaticAberration;
        public float Smoothed;
    }
}
