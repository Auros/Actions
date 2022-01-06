using System;
using Zenject;
using Actions.UI.Dashboards;

namespace Actions.Installers
{
    internal class ActionsDashboardInstaller : Installer
    {
        private readonly Config? _config;

        public ActionsDashboardInstaller(Config? config)
        {
            _config = config;
        }

        public override void InstallBindings()
        {
            if (_config == null || !_config.ShowInGame)
            {
                return;
            }

            Container.Bind(typeof(IInitializable), typeof(IDisposable), typeof(MacroDash)).To<MacroDash>().FromNewComponentAsViewController().AsSingle();
            Container.Bind(typeof(IInitializable), typeof(IDisposable), typeof(UserManagerDash)).To<UserManagerDash>().FromNewComponentAsViewController().AsSingle();
        }
    }
}