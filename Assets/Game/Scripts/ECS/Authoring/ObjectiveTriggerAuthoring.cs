using Game.Scripts.ECS.Extensions;
using UnityEngine;
using Zenject;

namespace Game.Scripts.ECS.Authoring
{
    public enum ObjectiveTriggerMode
    {
        Show,
        Complete
    }

    [RequireComponent(typeof(Collider))]
    public class ObjectiveTriggerAuthoring : MonoBehaviour
    {
        [SerializeField] private ObjectiveTriggerMode _mode;
        [SerializeField] private string _objectiveKey;
        [SerializeField] private string _thoughtKey;
        [SerializeField] private bool _once = true;

        [Inject] private IEcsWorldProvider _ecsWorldProvider;

        private bool _fired;

        private void OnTriggerEnter(Collider other)
        {
            if (_ecsWorldProvider == null || string.IsNullOrEmpty(_objectiveKey)) return;
            if (_once && _fired) return;
            if (other.GetComponentInParent<PlayerAuthoring>() == null) return;

            _fired = true;
            if (_mode == ObjectiveTriggerMode.Show) _ecsWorldProvider.World.ShowObjective(_objectiveKey, _thoughtKey);
            else _ecsWorldProvider.World.CompleteObjective(_objectiveKey);
        }
    }
}
