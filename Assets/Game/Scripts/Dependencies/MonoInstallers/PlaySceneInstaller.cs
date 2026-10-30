using Game.Scripts.ECS;
using Game.Scripts.Views;
using Zenject;

namespace Game.Scripts.Dependencies.MonoInstallers
{
    public class PlaySceneInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<InteractionPromptView>().FromComponentInHierarchy().AsSingle();
            Container.BindInterfacesTo<EcsStartup>().AsSingle().NonLazy();
        }
    }
}
