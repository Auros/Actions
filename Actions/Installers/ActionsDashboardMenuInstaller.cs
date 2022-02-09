using System;
using Actions.UI.Dashboards;
using Zenject;

namespace Actions.Installers
{
    internal class ActionsDashboardMenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.Bind(typeof(IInitializable), typeof(IDisposable), typeof(MacroDash)).To<MacroDash>().FromNewComponentAsViewController().AsSingle();
            Container.Bind(typeof(IInitializable), typeof(IDisposable), typeof(UserManagerDash)).To<UserManagerDash>().FromNewComponentAsViewController().AsSingle();
        }
    }
}