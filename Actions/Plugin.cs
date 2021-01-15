using IPA;
using SiraUtil;
using IPA.Loader;
using SiraUtil.Zenject;
using Actions.Installers;
using SiraUtil.Attributes;
using IPALogger = IPA.Logging.Logger;

namespace Actions
{
    [Plugin(RuntimeOptions.DynamicInit), Slog]
    public class Plugin
    {
        [Init]
        public Plugin(IPALogger log, Zenjector zenjector, PluginMetadata metadata)
        {
            zenjector
            .On<PCAppInit>()
            .Register<ActionsCoreInstaller>()
            .Pseudo(Container =>
            {
                Container.BindLoggerAsSiraLogger(log);
                Container.BindInstance(metadata).WithId(nameof(Actions)).AsSingle();
            });

            zenjector.OnMenu<ActionsDashboardInstaller>();
        }

        [OnEnable, OnDisable]
        public void OnState() { /* On State */ }
    }
}