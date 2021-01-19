using Zenject;
using SiraUtil;
using Actions.UI;

namespace Actions.Installers
{
    public class ActionsMenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<MenuButtonManager>().AsSingle();
            Container.Bind<ActionMainView>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<ActionMacroView>().FromNewComponentAsViewController().AsSingle();

            Container.Bind<ActionFlowCoordinator>().FromNewComponentOnNewGameObject(nameof(ActionFlowCoordinator)).AsSingle();
        }
    }
}