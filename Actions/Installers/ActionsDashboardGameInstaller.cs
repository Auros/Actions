namespace Actions.Installers
{
    internal class ActionsDashboardGameInstaller : ActionsDashboardMenuInstaller
    {
        private readonly Config _config;

        public ActionsDashboardGameInstaller(Config config)
        {
            _config = config;
        }

        public override void InstallBindings()
        {
            if (!_config.ShowInGame)
            {
                return;
            }

            base.InstallBindings();
        }
    }
}