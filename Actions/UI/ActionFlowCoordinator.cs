using HMUI;
using Zenject;
using Actions.UI.Dashboards;
using BeatSaberMarkupLanguage;

namespace Actions.UI
{
    internal class ActionFlowCoordinator : FlowCoordinator
    {
        private MacroDash _macroDash = null!;
        private ActionMainView _mainView = null!;
        private UserManagerDash _userManagerDash = null!;
        private MainFlowCoordinator _mainFlowCoordinator = null!;


        [Inject]
        public void Construct(MacroDash macroDash, ActionMainView mainView, UserManagerDash userManagerDash, MainFlowCoordinator mainFlowCoordinator)
        {
            _mainView = mainView;
            _macroDash = macroDash;
            _userManagerDash = userManagerDash;
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
            _mainView.EditModeToggled += MainView_EditModeToggled;
        }

        private void MainView_EditModeToggled(bool value)
        {
            _macroDash.Moveable = value;
            _userManagerDash.Moveable = value;
        }

        protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
        {
            _mainView.EditModeToggled -= MainView_EditModeToggled;
            base.DidDeactivate(removedFromHierarchy, screenSystemDisabling);
        }

        protected override void BackButtonWasPressed(ViewController topViewController)
        {
            _mainFlowCoordinator.DismissFlowCoordinator(this);
        }
    }
}