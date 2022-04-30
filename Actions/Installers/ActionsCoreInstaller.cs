using Actions.Dashboard;
using Actions.Twitch;
using Zenject;

namespace Actions.Installers
{
    internal class ActionsCoreInstaller : Installer
    {
        private readonly Config _config;

        public ActionsCoreInstaller(Config config)
        {
            _config = config;
        }

        public override void InstallBindings()
        {
            Container.BindInstance(_config).AsSingle();
            Container.BindInterfacesTo<TwitchSocialPlatform>().AsSingle();
            Container.BindInterfacesAndSelfTo<PlatformManager>().AsSingle();
        }
    }
}