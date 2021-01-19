using System;
using Zenject;
using SiraUtil;
using Actions.UI.Dashboards;

namespace Actions.Installers
{
    public class ActionsDashboardInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.Bind(typeof(IInitializable), typeof(MacroDash)).To<MacroDash>().FromNewComponentAsViewController().AsSingle();
            Container.Bind(typeof(IInitializable), typeof(IDisposable), typeof(UserManagerDash)).To<UserManagerDash>().FromNewComponentAsViewController().AsSingle();
        }
    }
}