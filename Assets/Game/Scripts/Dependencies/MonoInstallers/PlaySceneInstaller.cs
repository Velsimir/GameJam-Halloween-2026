using Game.Scripts.ECS;
using Game.Scripts.Views;
using Velsi.Typecraft.Dialogue;
using Zenject;

namespace Game.Scripts.Dependencies.MonoInstallers
{
    public class PlaySceneInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<InteractionPromptView>().FromComponentInHierarchy().AsSingle();
            Container.Bind<ObjectivesView>().FromComponentInHierarchy().AsSingle();
            Container.Bind<ScreenFadeView>().FromComponentInHierarchy().AsSingle();
            Container.Bind<DayIntroView>().FromComponentInHierarchy().AsSingle();
            Container.Bind<EndScreenView>().FromComponentInHierarchy().AsSingle();
            Container.Bind<DiaryView>().FromComponentInHierarchy().AsSingle();
            Container.Bind<DialoguePlayer>().FromComponentInHierarchy().AsSingle();
            Container.BindInterfacesTo<EcsStartup>().AsSingle().NonLazy();
        }
    }
}
