using HMUI;
using Zenject;
using BeatSaberMarkupLanguage;

namespace Actions.UI
{
    internal class ActionFlowCoordinator : FlowCoordinator
    {
        private ActionMainView _mainView = null!;
        private MainFlowCoordinator _mainFlowCoordinator = null!;

        [Inject]
        public void Construct(ActionMainView mainView, MainFlowCoordinator mainFlowCoordinator)
        {
            _mainView = mainView;
            _mainFlowCoordinator = mainFlowCoordinator;
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            if (firstActivation)
            {
                showBackButton = true;
                SetTitle(nameof(Actions));
            }

            if (addedToHierarchy) ProvideInitialViewControllers(_mainView);
        }

        protected override void BackButtonWasPressed(ViewController topViewController)
        {
            _mainFlowCoordinator.DismissFlowCoordinator(this);
        }
    }
}