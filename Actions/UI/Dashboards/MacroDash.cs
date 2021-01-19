using System;
using Zenject;
using Tweening;
using System.Linq;
using UnityEngine;
using Actions.Dashboard;
using Actions.Components;
using System.ComponentModel;
using System.Collections.Generic;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.FloatingScreen;

namespace Actions.UI.Dashboards
{
    [ViewDefinition("Actions.Views.macro-dash.bsml")]
    [HotReload(RelativePathToLayout = @"..\..\Views\macro-dash.bsml")]
    internal class MacroDash : FloatingViewController<MacroDash>, IInitializable, IDisposable
    {
        [Inject]
        private readonly Config _config = null!;

        [Inject]
        private readonly TweeningManager _tweeningManager = null!;

        [Inject]
        private readonly PlatformManager _platformManager = null!;

        [UIValue("macro-hosts")]
        protected readonly List<object> macroHosts = new List<object>();

        [UIComponent("macro-container")]
        protected readonly RectTransform macroContainer = null!;
        protected CanvasGroup macroContainerCanvas = null!;
        private bool opened = false;

        public void Initialize()
        {
            gameObject.SetActive(true);

            _floatingScreen!.HandleReleased += HandleReleased;
            _floatingScreen!.HandleSide = FloatingScreen.Side.Right;
            _floatingScreen!.ScreenPosition = _config.MacroDashboardPosition;
            _floatingScreen!.ScreenRotation = Quaternion.Euler(_config.MacroDashboardRotation);
        }

        private void HandleReleased(object _, FloatingScreenHandleEventArgs e)
        {
            _config.MacroDashboardPosition = e.Position;
            _config.MacroDashboardRotation = e.Rotation.eulerAngles;
        }

        public void Dispose()
        {
            _floatingScreen!.HandleReleased -= HandleReleased;
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            if (firstActivation)
            {
                var macros = _config.Macros.Take(21);
                for (int i = 0; i < 21; i++)
                {
                    var macro = macros.ElementAtOrDefault(i);
                    var host = new MacroHost(MacroClicked);
                    macroHosts.Add(host);
                    if (macro is null)
                        continue;
                    host.Macro = macro;
                }
            }
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            if (firstActivation)
            {
                macroContainerCanvas = macroContainer.gameObject.AddComponent<CanvasGroup>();
                macroContainerCanvas.alpha = 0f;
            }
        }

        public void MacroCreated(Macro macro)
        {
            var host = macroHosts.Cast<MacroHost>().FirstOrDefault(mh => mh.Macro == null);
            if (host is null)
                return;
            host.Macro = macro;
        }

        public void MacroEdited(Macro macro)
        {
            var host = macroHosts.Cast<MacroHost>().FirstOrDefault(mh => mh.Macro != null && mh.Macro == macro);
            if (host is null)
                return;
            host.Macro = macro;
        }

        public void MacroDeleted(Macro macro)
        {
            var host = macroHosts.Cast<MacroHost>().FirstOrDefault(mh => mh.Macro != null && mh.Macro == macro);
            if (host is null)
                return;
            host.Macro = null;
        }

        [UIAction("toggle")]
        protected void Clicked()
        {
            _tweeningManager.KillAllTweens(this);
            var currentAlpha = macroContainerCanvas.alpha;
            if (opened)
            {
                _tweeningManager.AddTween(new FloatTween(currentAlpha, 0f, UpdateCanvasAlpha, 0.5f, EaseType.InOutQuad), this);
            }
            else
            {
                _tweeningManager.AddTween(new FloatTween(currentAlpha, 1f, UpdateCanvasAlpha, 0.5f, EaseType.InOutQuad), this);
            }
            opened = !opened;
        }

        private void UpdateCanvasAlpha(float val)
        {
            macroContainerCanvas.alpha = val;
        }

        private void MacroClicked(Macro macro)
        {
            if (!macro.IsCommand)
            {
                _platformManager.SendMessage(macro.Content);
                return;
            }
            _platformManager.SendCommand(macro.Content);
        }

        public class MacroHost : INotifyPropertyChanged
        {
            [UIValue("name")] public string Name => _macroValue?.Name ?? "[MACRO]";
            [UIValue("has-content")] protected bool HasContent => !(Macro is null);

            private Macro? _macroValue;
            public Macro? Macro
            {
                get => _macroValue;
                set
                {
                    _macroValue = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasContent)));
                }
            }

            private readonly Action<Macro>? _macro;
            public event PropertyChangedEventHandler? PropertyChanged;

            public MacroHost(Action<Macro>? macro = null)
            {
                _macro = macro;
            }

            [UIAction("clicked")]
            protected void Clicked()
            {
                if (Macro is null)
                    return;
                _macro?.Invoke(Macro);
            }
        }
    }
}