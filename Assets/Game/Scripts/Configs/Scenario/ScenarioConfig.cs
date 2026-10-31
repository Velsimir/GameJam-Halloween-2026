using UnityEngine;

namespace Game.Scripts.Configs.Scenario
{
    [CreateAssetMenu(fileName = "ScenarioConfig", menuName = "Configs/Scenario/ScenarioConfig")]
    public class ScenarioConfig : ScriptableObject
    {
        [SerializeField] private string _notNowThoughtKey = "ui.thought.scenario_not_now";
        [SerializeField, Min(0f)] private float _notNowCooldown = 3f;

        public string NotNowThoughtKey => _notNowThoughtKey;
        public float NotNowCooldown => _notNowCooldown;
    }
}
