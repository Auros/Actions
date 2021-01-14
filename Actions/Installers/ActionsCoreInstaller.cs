using Zenject;
using Actions.Twitch;
using Actions.Dashboard;

namespace Actions.Installers
{
    internal class ActionsCoreInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.Bind<Http>().AsSingle();
            Container.BindInterfacesTo<TwitchSocialPlatform>().AsSingle();
            Container.BindInterfacesAndSelfTo<PlatformManager>().AsSingle();
        }
    }
}