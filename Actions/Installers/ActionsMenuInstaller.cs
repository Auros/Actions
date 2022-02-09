using Zenject;
using Actions.UI;
using Actions.Managers;

namespace Actions.Installers
{
    internal class ActionsMenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<CommandCreator>().AsSingle();
            Container.BindInterfacesAndSelfTo<MenuButtonManager>().AsSingle();
            Container.Bind<ActionMainView>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<ActionMacroView>().FromNewComponentAsViewController().AsSingle();

            Container.Bind<ActionFlowCoordinator>().FromNewComponentOnNewGameObject().WithGameObjectName(nameof(ActionFlowCoordinator)).AsSingle();
        }
    }
}