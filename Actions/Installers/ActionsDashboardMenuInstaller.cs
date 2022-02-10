using System;
using Zenject;
using Actions.UI.Dashboards;

namespace Actions.Installers
{
    public class ActionsDashboardMenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.Bind(typeof(IInitializable), typeof(IDisposable), typeof(MacroDash)).To<MacroDash>().FromNewComponentAsViewController().AsSingle();
            Container.Bind(typeof(IInitializable), typeof(IDisposable), typeof(UserManagerDash)).To<UserManagerDash>().FromNewComponentAsViewController().AsSingle();
        }
    }
}