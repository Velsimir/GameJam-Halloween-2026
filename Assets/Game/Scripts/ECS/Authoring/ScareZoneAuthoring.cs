using Game.Scripts.ECS.Components.Fear;
using UnityEngine;
using Zenject;

namespace Game.Scripts.ECS.Authoring
{
    [RequireComponent(typeof(Collider))]
    public class ScareZoneAuthoring : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)] private float _strength = 1f;

        [Inject] private IEcsWorldProvider _ecsWorldProvider;

        private void OnTriggerEnter(Collider other)
        {
            if (_ecsWorldProvider == null) return;
            if (other.GetComponentInParent<PlayerAuthoring>() == null) return;

            var world = _ecsWorldProvider.World;
            world.GetPool<ScareRequest>().Add(world.NewEntity()).Strength = _strength;
        }
    }
}
