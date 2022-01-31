using IPA;
using SiraUtil;
using IPA.Loader;
using SiraUtil.Zenject;
using IPA.Config.Stores;
using Actions.Installers;
using SiraUtil.Attributes;
using Conf = IPA.Config.Config;
using IPALogger = IPA.Logging.Logger;

namespace Actions
{
    [Plugin(RuntimeOptions.DynamicInit), Slog, NoEnableDisable]
    public class Plugin
    {
        [Init]
        public Plugin(Conf conf, IPALogger log, Zenjector zenjector, PluginMetadata metadata)
        {
            Config config = conf.Generated<Config>();
            config.Version = metadata.HVersion;

            zenjector.UseLogger(log);
            zenjector.Install<ActionsCoreInstaller>(Location.App);
            zenjector.Install(Location.App, Container =>
            {
                Container.BindInstance(config).AsSingle();
                Container.BindInstance(metadata).WithId(nameof(Actions)).AsCached();
            });

            zenjector.Install<ActionsMenuInstaller>(Location.Menu);
            zenjector.Install<ActionsDashboardMenuInstaller>(Location.Menu);
            zenjector.Install<ActionsDashboardGameInstaller>(Location.Player);
        }

        [OnEnable, OnDisable]
        public void OnState() { /* On State */ }
    }
}