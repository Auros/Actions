using System;
using Zenject;
using Actions.UI.Dashboards;

namespace Actions.Installers
{
    public class ActionsDashboardGameInstaller : Installer
    {
        private readonly Config _config;
        ActionsDashboardGameInstaller(Config config)
        {
            _config = config;
        }

        public override void InstallBindings()
        {
            if (!_config.ShowInGame)
                return;
            Container.Bind(typeof(IInitializable), typeof(IDisposable), typeof(MacroDash)).To<MacroDash>().FromNewComponentAsViewController().AsSingle();
            Container.Bind(typeof(IInitializable), typeof(IDisposable), typeof(UserManagerDash)).To<UserManagerDash>().FromNewComponentAsViewController().AsSingle();
        }
    }
}