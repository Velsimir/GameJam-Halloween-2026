using Game.Scripts.Services.Booststraps;
using Game.Scripts.Services.SceneLoader;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Game.Scripts.Editor
{
    [InitializeOnLoad]
    public class EntryPointSceneAutoLoader
    {
        private const string MenuPath = "PlayFromBootstrap/Enebled";
        private const string PlayRoomMenuPath = "PlayFromBootstrap/Scene/PlayRoom";
        private const string ModelTestMenuPath = "PlayFromBootstrap/Scene/ModelTest";
        private const string PlayFromBootstrapKey = "PlayFromBootstrapKey";
        private const int BootSceneIndex = 0;

        static EntryPointSceneAutoLoader()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        [MenuItem(MenuPath)]
        private static void Toggle()
        {
            bool result = EditorPrefs.GetBool(PlayFromBootstrapKey);
            EditorPrefs.SetBool(PlayFromBootstrapKey, !result);
        }

        [MenuItem(MenuPath, true)]
        private static bool ToggleValidate()
        {
            Menu.SetChecked(MenuPath, EditorPrefs.GetBool(PlayFromBootstrapKey));
            return true;
        }

        [MenuItem(PlayRoomMenuPath, false, 100)]
        private static void SelectPlayRoom() => SelectScene(ScenesName.Playroom);

        [MenuItem(PlayRoomMenuPath, true)]
        private static bool SelectPlayRoomValidate()
        {
            Menu.SetChecked(PlayRoomMenuPath, GetSelectedScene() == ScenesName.Playroom);
            return true;
        }

        [MenuItem(ModelTestMenuPath, false, 101)]
        private static void SelectModelTest() => SelectScene(ScenesName.ModelTest);

        [MenuItem(ModelTestMenuPath, true)]
        private static bool SelectModelTestValidate()
        {
            Menu.SetChecked(ModelTestMenuPath, GetSelectedScene() == ScenesName.ModelTest);
            return true;
        }

        private static ScenesName GetSelectedScene()
        {
            string saved = EditorPrefs.GetString(GameBootstrapper.EditorGameSceneKey, ScenesName.Playroom.ToString());
            return System.Enum.TryParse(saved, out ScenesName scene) ? scene : ScenesName.Playroom;
        }

        private static void SelectScene(ScenesName scene)
        {
            EditorPrefs.SetString(GameBootstrapper.EditorGameSceneKey, scene.ToString());
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                if (EditorPrefs.GetBool(PlayFromBootstrapKey) == false)
                {
                    EditorSceneManager.playModeStartScene = null;
                    return;
                }

                if (EditorBuildSettings.scenes.Length == 0)
                    return;

                EditorSceneManager.playModeStartScene =
                    AssetDatabase.LoadAssetAtPath<SceneAsset>(EditorBuildSettings.scenes[BootSceneIndex].path);
            }
        }
    }
}
