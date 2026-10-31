using System;
using System.Collections.Generic;
using Game.Scripts.Configs.Scenario.Actions;
using Game.Scripts.ECS.Extensions;
using UnityEngine;
using Velsi.Typecraft.Dialogue;

namespace Game.Scripts.Configs.Scenario
{
    [Serializable]
    public class DayStep
    {
        [SerializeField] private string _id;
        [SerializeField] private string _objectiveKey;

        [Header("Thoughts")]
        [SerializeField] private DialogueAsset _startThought;
        [SerializeField] private string _startThoughtKey;
        [SerializeField] private DialogueAsset _reminderThought;
        [SerializeField] private string _reminderThoughtKey;
        [SerializeField, Min(0f)] private float _idleSeconds = 30f;

        [Header("Completion")]
        [SerializeField] private string _targetId;
        [SerializeField] private ScenarioCondition _condition;
        [SerializeField] private ItemDefinition _item;

        [Header("Episode")]
        [SerializeField] private DayEpisode _episode;

        [Header("Actions")]
        [SerializeReference, SubclassSelector] private List<ScenarioAction> _startActions = new List<ScenarioAction>();
        [SerializeReference, SubclassSelector] private List<ScenarioAction> _completeActions = new List<ScenarioAction>();

        public string Id => _id;
        public string ObjectiveKey => _objectiveKey;
        public float IdleSeconds => _idleSeconds;
        public string TargetId => _targetId;
        public ScenarioCondition Condition => _condition;
        public ItemDefinition Item => _item;
        public DayEpisode Episode => _episode != null && _episode.Enabled ? _episode : null;
        public IReadOnlyList<ScenarioAction> StartActions => _startActions;
        public IReadOnlyList<ScenarioAction> CompleteActions => _completeActions;

        public DialogueAsset StartThought => _startThought != null ? _startThought : ThoughtDialogueFactory.Get(_startThoughtKey);
        public DialogueAsset ReminderThought => _reminderThought != null ? _reminderThought : ThoughtDialogueFactory.Get(_reminderThoughtKey);

        public string Validate()
        {
            switch (_condition)
            {
                case ScenarioCondition.Interact:
                case ScenarioCondition.EnterZone:
                    return string.IsNullOrEmpty(_targetId) ? $"{_condition} requires TargetId" : null;
                case ScenarioCondition.HasItem:
                    return _item == null ? "HasItem requires Item" : null;
                case ScenarioCondition.External:
                    return string.IsNullOrEmpty(_id) ? "External requires Id" : null;
                default:
                    return null;
            }
        }
    }
}
