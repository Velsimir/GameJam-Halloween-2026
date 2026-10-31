using Game.Scripts.Configs.Audio;
using UnityEngine;

namespace Game.Scripts.ECS.Components.Audio
{
    public struct AmbientEmitter
    {
        public AmbientEmitterProfile Profile;
        public Transform Point;
        public bool Inside;
        public float Timer;
    }
}
