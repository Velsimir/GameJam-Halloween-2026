using System.Collections.Generic;
using UnityEngine;

namespace Game.Scripts.Services.Scenario
{
    public static class ScenarioTargetRegistry
    {
        private static readonly Dictionary<string, GameObject> Targets = new Dictionary<string, GameObject>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset() => Targets.Clear();

        public static void Register(string id, GameObject target)
        {
            if (string.IsNullOrEmpty(id) || target == null) return;

            if (Targets.TryGetValue(id, out GameObject current) && current != null && current != target)
                Debug.LogWarning($"[Scenario] duplicate target id '{id}': '{current.name}' replaced by '{target.name}'", target);

            Targets[id] = target;
        }

        public static void Unregister(string id, GameObject target)
        {
            if (string.IsNullOrEmpty(id)) return;
            if (Targets.TryGetValue(id, out GameObject current) && current == target)
                Targets.Remove(id);
        }

        public static bool TryGet(string id, out GameObject target)
        {
            target = null;
            if (string.IsNullOrEmpty(id)) return false;
            return Targets.TryGetValue(id, out target) && target != null;
        }
    }
}
