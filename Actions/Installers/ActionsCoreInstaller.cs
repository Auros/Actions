using Actions.Dashboard;
using Actions.Twitch;
using Zenject;

namespace Actions.Installers
{
    internal class ActionsCoreInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<TwitchSocialPlatform>().AsSingle();
            Container.BindInterfacesAndSelfTo<PlatformManager>().AsSingle();
        }
    }
}