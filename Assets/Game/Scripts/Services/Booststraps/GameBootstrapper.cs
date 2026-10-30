using Cysharp.Threading.Tasks;
using Game.Scripts.Services.Loading;
using Game.Scripts.Services.Loading.Operations;
using Game.Scripts.Services.SceneLoader;
using Zenject;

namespace Game.Scripts.Services.Booststraps
{
    public class GameBootstrapper : IInitializable
    {
        private readonly ILoadingService _loadingService;
        private readonly ISceneLoader _sceneLoader;

        public GameBootstrapper(ILoadingService loadingService, ISceneLoader sceneLoader)
        {
            _loadingService = loadingService;
            _sceneLoader = sceneLoader;
        }

        public void Initialize()
        {
            LoadGameScene();
        }

        private void LoadGameScene()
        {
            ILoadingOperation[] operations =
            {
                new LoadingSceneOperation(_sceneLoader, ResolveGameScene())
            };

            _loadingService.BeginLoadingAsync(operations).Forget();
        }

        private static ScenesName ResolveGameScene()
        {
#if UNITY_EDITOR
            string saved = UnityEditor.EditorPrefs.GetString(EditorGameSceneKey, ScenesName.Playroom.ToString());
            if (System.Enum.TryParse(saved, out ScenesName scene) && scene != ScenesName.Init)
                return scene;
#endif
            return ScenesName.Playroom;
        }

        public const string EditorGameSceneKey = "PlayFromBootstrapGameScene";
    }
}