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
            zenjector.UseMetadataBinder<Plugin>();
            
            zenjector.Install<ActionsCoreInstaller>(Location.App, config);
            zenjector.Install<ActionsMenuInstaller>(Location.Menu);
            zenjector.Install<ActionsDashboardInstaller>(Location.Menu, null!);
            zenjector.Install<ActionsDashboardInstaller>(Location.Tutorial | Location.Player, config);
        }
    }
}