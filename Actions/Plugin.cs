using Actions.Installers;
using IPA.Config.Stores;
using IPA;
using IPA.Loader;
using IPA.Logging;
using SiraUtil.Attributes;
using SiraUtil.Zenject;
using Conf = IPA.Config.Config;

namespace Actions
{
    [Plugin(RuntimeOptions.DynamicInit), Slog, NoEnableDisable]
    public class Plugin
    {
        [Init]
        public Plugin(Conf conf, Logger log, Zenjector zenjector, PluginMetadata metadata)
        {
            Config config = conf.Generated<Config>();
            config.Version = metadata.HVersion;

            zenjector.UseLogger(log);
            zenjector.Install<ActionsCoreInstaller>(Location.App);
            zenjector.Install(Location.App, container =>
            {
                container.BindInstance(config).AsSingle();
                container.BindInstance(metadata).WithId(nameof(Actions)).AsCached();
            });

            zenjector.Install<ActionsMenuInstaller>(Location.Menu);
            zenjector.Install<ActionsDashboardMenuInstaller>(Location.Menu);
            zenjector.Install<ActionsDashboardGameInstaller>(Location.Player);
        }
    }
}