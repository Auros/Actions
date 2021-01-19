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
    [Plugin(RuntimeOptions.DynamicInit), Slog]
    public class Plugin
    {
        [Init]
        public Plugin(Conf conf, IPALogger log, Zenjector zenjector, PluginMetadata metadata)
        {
            Config config = conf.Generated<Config>();
            config.Version = metadata.Version;

            zenjector
                .On<PCAppInit>()
                .Register<ActionsCoreInstaller>()
                .Pseudo(Container =>
                {
                    Container.BindLoggerAsSiraLogger(log);
                    Container.BindInstance(config).AsSingle();
                    Container.BindInstance(metadata).WithId(nameof(Actions)).AsCached();
                });

            zenjector.OnMenu<ActionsMenuInstaller>();
            zenjector.OnMenu<ActionsDashboardInstaller>();
            zenjector.OnGame<ActionsDashboardInstaller>().When(() => config.ShowInGame);
        }

        [OnEnable, OnDisable]
        public void OnState() { /* On State */ }
    }
}