using IPA;
using SiraUtil;
using SiraUtil.Zenject;
using IPALogger = IPA.Logging.Logger;

namespace Actions
{
    [Plugin(RuntimeOptions.DynamicInit)]
    public class Plugin
    {
        [Init]
        public Plugin(IPALogger log, Zenjector zenjector)
        {
            zenjector
            .On<PCAppInit>()
            .Pseudo(Container =>
            {
                Container.BindLoggerAsSiraLogger(log);
            });
        }

        [OnEnable, OnDisable]
        public void OnState() { /* On State */ }
    }
}