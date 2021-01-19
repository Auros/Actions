using System;
using Zenject;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.MenuButtons;

namespace Actions.UI
{
    internal class MenuButtonManager : IInitializable, IDisposable
    {
        private readonly MenuButton _menuButton;
        private readonly MainFlowCoordinator _mainFlowCoordinator;
        private readonly ActionFlowCoordinator _actionFlowCoordinator;

        public MenuButtonManager(ActionFlowCoordinator actionFlowCoordinator, MainFlowCoordinator mainFlowCoordinator)
        {
            _mainFlowCoordinator = mainFlowCoordinator;
            _actionFlowCoordinator = actionFlowCoordinator;
            _menuButton = new MenuButton(nameof(Actions), ShowFlow);
        }

        private void ShowFlow() => _mainFlowCoordinator.PresentFlowCoordinator(_actionFlowCoordinator);
        public void Initialize() => MenuButtons.instance.RegisterButton(_menuButton);

        public void Dispose()
        {
            if (MenuButtons.IsSingletonAvailable && BSMLParser.IsSingletonAvailable)
            {
                MenuButtons.instance.UnregisterButton(_menuButton);
            }
        }
    }
}