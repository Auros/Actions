using System;
using Zenject;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.MenuButtons;
using Actions.Dashboard;

namespace Actions.UI
{
    internal class MenuButtonManager : IInitializable, IDisposable
    {
        private readonly MenuButton _menuButton;
        private readonly ISocialPlatform _socialPlatform;
        private readonly MainFlowCoordinator _mainFlowCoordinator;
        private readonly ActionFlowCoordinator _actionFlowCoordinator;

        public MenuButtonManager(ISocialPlatform socialPlatform, ActionFlowCoordinator actionFlowCoordinator, MainFlowCoordinator mainFlowCoordinator)
        {
            _socialPlatform = socialPlatform;
            _mainFlowCoordinator = mainFlowCoordinator;
            _actionFlowCoordinator = actionFlowCoordinator;
            _menuButton = new MenuButton(nameof(Actions), ShowFlow);
        }
        public void Initialize() => MenuButtons.instance.RegisterButton(_menuButton);

        public void Dispose()
        {
            if (MenuButtons.IsSingletonAvailable && BSMLParser.IsSingletonAvailable)
            {
                MenuButtons.instance.UnregisterButton(_menuButton);
            }
        }

        private void ShowFlow()
        {
            if (_socialPlatform.Initialized)
                _mainFlowCoordinator.PresentFlowCoordinator(_actionFlowCoordinator);
        }
    }
}