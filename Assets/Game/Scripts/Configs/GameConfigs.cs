using Game.Scripts.ECS.Authoring;
using UnityEngine;

namespace Game.Scripts.Configs
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Configs/GameConfigs")]
    public class GameConfigs : ScriptableObject
    {
        [SerializeField] private PlayerConfig _playerConfig;
        [SerializeField] private WalkConfig _walkConfig;
        [SerializeField] private GravityConfig _gravityConfigs;
        [SerializeField] private FearConfig _fearConfig;
        [SerializeField] private InteractionPromptConfig _interactionPromptConfig;

        [SerializeField] private ScreenFadeConfig _screenFadeConfig;
        [SerializeField] private ObjectiveConfig _objectiveConfig;

        public ObjectiveConfig ObjectiveConfig => _objectiveConfig;
        public ScreenFadeConfig ScreenFadeConfig => _screenFadeConfig;
        public PlayerConfig PlayerConfig => _playerConfig;
        public WalkConfig WalkConfig => _walkConfig;
        public GravityConfig GravityConfig => _gravityConfigs;
        public FearConfig FearConfig => _fearConfig;
        public InteractionPromptConfig InteractionPromptConfig => _interactionPromptConfig;
    }
}