using System;
using Zenject;
using System.Linq;
using UnityEngine;
using Actions.Dashboard;
using System.Collections.Generic;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;

namespace Actions.UI
{
    [ViewDefinition("Actions.Views.main-view.bsml")]
    [HotReload(RelativePathToLayout = @"..\Views\main-view.bsml")]
    internal class ActionMainView : BSMLAutomaticViewController
    {
        [Inject]
        protected readonly Config _config = null!;

        [Inject]
        protected readonly ISocialPlatform _socialPlatform = null!;

        public event Action<bool>? EditModeToggled;

        [UIValue("channel")]
        protected string Channel
        {
            get => _config.Channel;
            set => _config.Channel = value;
        }

        [UIValue("tts-prefix")]
        protected bool TTSPrefix
        {
            get => _config.PrefixForTTS;
            set => _config.PrefixForTTS = value;
        }

        [UIValue("show-in-game")]
        protected bool ShowInGame
        {
            get => _config.ShowInGame;
            set => _config.ShowInGame = value;
        }

        [UIValue("allow-mods")]
        protected bool AllowMods
        {
            get => _config.AllowModsToCreate;
            set => _config.AllowModsToCreate = value;
        }

        [UIValue("edit-mode")]
        protected bool EditMode { get; set; }

        [UIValue("version")]
        protected string Version => $"v{_config.Version}";

        [UIValue("channels")]
        protected readonly List<object> channels = new List<object>();

        [UIParams]
        protected readonly BSMLParserParams parserParams = null!;

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            if (firstActivation)
                if (_socialPlatform is Twitch.TwitchSocialPlatform tsp)
                    channels.AddRange(tsp.Channels.Cast<object>());
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
        }

        protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
        {
            EditMode = false;
            EditModeToggle(false);
            parserParams.EmitEvent("update-edit");
            base.DidDeactivate(removedFromHierarchy, screenSystemDisabling);
        }

        [UIAction("edit-mode-toggled")]
        public void EditModeToggle(bool value)
        {
            EditModeToggled?.Invoke(value);
        }

        [UIAction("clicked-auros")]
        protected void ClickedAuros()
        {
            Application.OpenURL("https://ko-fi.com/aurosnex");
        }
    }
}