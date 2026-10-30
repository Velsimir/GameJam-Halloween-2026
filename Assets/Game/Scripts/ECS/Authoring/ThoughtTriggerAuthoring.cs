using Game.Scripts.ECS.Extensions;
using UnityEngine;
using Velsi.Typecraft.Dialogue;
using Zenject;

namespace Game.Scripts.ECS.Authoring
{
    [RequireComponent(typeof(Collider))]
    public class ThoughtTriggerAuthoring : MonoBehaviour
    {
        [SerializeField] private DialogueAsset _dialogue;
        [SerializeField] private bool _once;
        [SerializeField] private bool _interrupt;

        [Inject] private IEcsWorldProvider _ecsWorldProvider;

        private bool _fired;

        private void OnTriggerEnter(Collider other)
        {
            if (_ecsWorldProvider == null || _dialogue == null) return;
            if (_once && _fired) return;
            if (other.GetComponentInParent<PlayerAuthoring>() == null) return;

            _fired = true;
            _ecsWorldProvider.World.RequestThought(_dialogue, _interrupt);
        }
    }
}
