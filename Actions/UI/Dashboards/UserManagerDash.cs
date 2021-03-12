using HMUI;
using System;
using Zenject;
using Tweening;
using System.Linq;
using UnityEngine;
using Actions.Twitch;
using Actions.Dashboard;
using Actions.Components;
using System.ComponentModel;
using BeatSaberMarkupLanguage;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.FloatingScreen;

namespace Actions.UI.Dashboards
{
    [ViewDefinition("Actions.Views.user-manager-dash.bsml")]
    [HotReload(RelativePathToLayout = @"..\..\Views\user-manager-dash.bsml")]
    internal class UserManagerDash : FloatingViewController<UserManagerDash>, IInitializable, IDisposable
    {
        [Inject]
        private readonly Config _config = null!;

        [Inject]
        private readonly TweeningManager _tweeningManager = null!;

        [Inject]
        private readonly PlatformManager _platformManager = null!;

        [UIValue("user-hosts")]
        protected readonly List<object> userHosts = new List<object>();

        [UIParams]
        protected readonly BSMLParserParams parserParams = null!;

        [UIComponent("user-container")]
        protected readonly RectTransform userContainer = null!;

        [UIValue("timeout-value")]
        protected ManagementAction SelectedAction { get; set; }

        [UIComponent("execute-text")]
        protected readonly CurvedTextMeshPro executeText = null!;

        [UIComponent("name-text")]
        protected readonly CurvedTextMeshPro nameText = null!;

        [UIComponent("nothing-text")]
        protected CurvedTextMeshPro nothingText = null!;

        private bool _normal;
        [UIValue("normal")]
        public bool Normal
        {
            get => _normal;
            set
            {
                _normal = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(NotNormal));
            }
        }

        [UIValue("not-normal")]
        public bool NotNormal => !Normal;

        protected CanvasGroup userContainerCanvas = null!;
        protected DateTime _specialCommandExecutionTime;
        private IActionUser? _lastClickedUser;
        private string rootString = "";
        protected Macro? _specialMacro;
        private bool opened = false;

        public void Initialize()
        {
            gameObject.SetActive(_config.Enabled);
            rootString = gameObject.scene.name;
            SceneManager.activeSceneChanged += SceneChanged;
            _platformManager.ChannelActivity += ActivityReceived;

            _config.Updated += Config_Updated;
            _floatingScreen!.HandleReleased += HandleReleased;
            _floatingScreen!.ScreenPosition = _config.UserManagerDashboardPosition;
            _floatingScreen!.ScreenRotation = Quaternion.Euler(_config.UserManagerDashboardRotation);
        }

        private void Config_Updated(Config config)
        {
            gameObject.SetActive(config.Enabled);
        }

        public void SetSpecialMacro(Macro macro)
        {
            _specialMacro = macro;
            _specialCommandExecutionTime = DateTime.Now;
        }

        private void SceneChanged(Scene oldScene, Scene newScene)
        {
            if (newScene.name == rootString)
            {
                if (opened)
                {
                    foreach (UserHost host in userHosts)
                        host.Update();
                }
            }
        }

        private void HandleReleased(object _, FloatingScreenHandleEventArgs e)
        {
            _config.UserManagerDashboardPosition = e.Position;
            _config.UserManagerDashboardRotation = e.Rotation.eulerAngles;
        }

        private void ActivityReceived(IActionUser user)
        {
            if (nothingText != null)
            {
                nothingText.gameObject.SetActive(false);
                nothingText = null!;
            }

            // progen shift
            var firstHost = (userHosts[0] as UserHost)!;

            // wont change anything
            if (firstHost.User != null)
            {
                if (firstHost.User.ID == user.ID)
                    return;
            }

            var hosts = userHosts.Cast<UserHost>().ToArray();
            var existingHost = hosts.FirstOrDefault(uh => uh.User == user);
            int indexOrLast = existingHost is null ? hosts.Count() - 1 : hosts.IndexOf(existingHost);
            for (int i = indexOrLast; i > 0; i--)
            {
                var current = hosts.ElementAt(i);
                var newer = hosts.ElementAt(i - 1);
                current.User = newer.User;
            }
            firstHost.User = user;
            if (opened && isActiveAndEnabled)
            {
                foreach (UserHost host in userHosts)
                    host.Update();
            }
        }

        public void Dispose()
        {
            _config.Updated -= Config_Updated;
            SceneManager.activeSceneChanged -= SceneChanged;
            _floatingScreen!.HandleReleased -= HandleReleased;
            _platformManager.ChannelActivity -= ActivityReceived;
        }

        protected override async void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            if (firstActivation)
            {
                for (int i = 0; i < 10; i++)
                    userHosts.Add(new UserHost(UserClicked));
            }
            await SiraUtil.Utilities.AwaitSleep(100);
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            if (firstActivation)
            {
                nameText.fontSizeMin = 4.5f;
                nameText.fontSizeMax = 7.5f;
                nameText.enableAutoSizing = true;
                executeText.enableAutoSizing = true;

                userContainerCanvas = userContainer.gameObject.AddComponent<CanvasGroup>();
                userContainerCanvas.alpha = 0f;
            }
        }

        [UIAction("toggle")]
        protected void Clicked()
        {
            _tweeningManager.KillAllTweens(this);
            var currentAlpha = userContainerCanvas.alpha;
            if (opened)
            {
                var tween = _tweeningManager.AddTween(new FloatTween(currentAlpha, 0f, UpdateCanvasAlpha, 0.5f, EaseType.InOutQuad), this);
                tween.onCompleted += delegate ()
                {
                    if (!opened)
                        userContainerCanvas.gameObject.SetActive(false);
                };
            }
            else
            {
                userContainerCanvas.gameObject.SetActive(true);
                foreach (UserHost user in userHosts)
                    user.Update();
                _tweeningManager.AddTween(new FloatTween(currentAlpha, 1f, UpdateCanvasAlpha, 0.5f, EaseType.InOutQuad), this);
            }
            opened = !opened;
        }

        private void UpdateCanvasAlpha(float val)
        {
            userContainerCanvas.alpha = val;
        }

        private void UserClicked(IActionUser user)
        {
            _lastClickedUser = user;
            parserParams.EmitEvent("show-modal");
            nameText.text = _lastClickedUser.Name;
            nameText.fontSizeMax = 10.0f;
            nameText.fontSizeMin = 4.5f;

            if (_specialCommandExecutionTime.AddSeconds(10) > DateTime.Now && _specialMacro != null && _specialMacro.Content.Contains("{user}"))
            {
                executeText.text = _specialMacro.Content.Replace("{user}", user.Name);
                _specialCommandExecutionTime = default;
                executeText.fontSizeMax = 7.0f;
                executeText.fontSizeMin = 2.5f;
                Normal = false;
                return;
            }
            Normal = true;
        }

        [UIAction("format-timeout")]
        protected string FormatTimeout(ManagementAction action)
        {
            return (action switch
            {
                ManagementAction.Seconds1 => "1 Second",
                ManagementAction.Seconds30 => "30 Seconds",
                ManagementAction.Seconds69 => "69 Seconds",
                ManagementAction.Minutes1 => "1 Minute",
                ManagementAction.Minutes5 => "5 Minutes",
                ManagementAction.Minutes10 => "10 Minutes",
                ManagementAction.Minutes30 => "30 Minutes",
                ManagementAction.Hours1 => "1 Hour",
                ManagementAction.Hours4 => "4 Hours",
                ManagementAction.Hours12 => "12 Hours",
                ManagementAction.Days1 => "1 Day",
                ManagementAction.Days2 => "2 Days",
                ManagementAction.Weeks1 => "1 Week",
                ManagementAction.Weeks2 => "2 Weeks",
                ManagementAction.Forever => "Forever (Ban)",
                _ => throw new NotImplementedException()
            }).ToString();
        }

        [UIAction("execute")]
        protected void Execute()
        {
            if (_lastClickedUser is null || !(_lastClickedUser is TwitchActionUser) || _specialMacro is null)
                return;

            var command = _specialMacro.Content.Replace("{user}", _lastClickedUser.Name);
            if (_specialMacro.IsCommand)
                _platformManager.SendCommand(command);
            else
                _platformManager.SendMessage(command);

            _specialMacro = null;

            parserParams.EmitEvent("hide-modal");
        }

        [UIAction("timeout")]
        protected void Timeout()
        {
            if (_lastClickedUser is null || !(_lastClickedUser is TwitchActionUser))
                return;

            float? timeoutDuration = SelectedAction switch
            {
                ManagementAction.Seconds1 => 1,
                ManagementAction.Seconds30 => 30,
                ManagementAction.Seconds69 => 69,
                ManagementAction.Minutes1 => 60,
                ManagementAction.Minutes5 => 300,
                ManagementAction.Minutes10 => 600,
                ManagementAction.Minutes30 => 1800,
                ManagementAction.Hours1 => 3600,
                ManagementAction.Hours4 => 14400,
                ManagementAction.Hours12 => 43200,
                ManagementAction.Days1 => 86400,
                ManagementAction.Days2 => 172800,
                ManagementAction.Weeks1 => 604800,
                ManagementAction.Weeks2 => 1209600,
                ManagementAction.Forever => null,
                _ => throw new NotImplementedException()
            };

            _lastClickedUser.Ban(timeoutDuration);
            parserParams.EmitEvent("hide-modal");
        }

        public class UserHost : INotifyPropertyChanged
        {
            [UIValue("username")] protected string Username => User?.Name ?? "";
            [UIValue("has-content")] protected bool HasContent => !(User is null);
            [UIAction("clicked")] protected void Clicked() => _clickedCallback?.Invoke(User!);

            public IActionUser? User { get; set; }
            private readonly Action<IActionUser>? _clickedCallback;
            public event PropertyChangedEventHandler? PropertyChanged;
            [UIComponent("user-image")] protected readonly ImageView _userImage = null!;

            public UserHost(Action<IActionUser>? clickedCallback = null) => _clickedCallback = clickedCallback;

            public void Update()
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Username)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasContent)));
                if (User != null && _userImage != null)
                    _userImage.SetImage(User.ProfilePictureURL);
            }
        }
    }
}