using System;
using Zenject;
using Tweening;
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
            for (int i = 0; i < 21; i++)
                macroHosts.Add(new MacroHost(MacroClicked));

            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            macroContainerCanvas = macroContainer.gameObject.AddComponent<CanvasGroup>();
            macroContainerCanvas.alpha = 0f;

            Position = new Vector3(0f, 2.5f, 3f);

            var h = (macroHosts[0] as MacroHost)!;

            h.Name = "hello";
            h.Macro = "hello";
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

        private void MacroClicked(string macro)
        {
            _platformManager.SendMessage(macro);
        }

        public class MacroHost : INotifyPropertyChanged
        {
            [UIValue("name")] public string Name { get; set; } = "[MACRO]";
            [UIValue("has-content")] protected bool HasContent => !(Macro is null);

            private string? _macroText;
            public string? Macro
            {
                get => _macroText;
                set
                {
                    _macroText = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasContent)));
                }
            }

            private readonly Action<string>? _macro;
            public event PropertyChangedEventHandler? PropertyChanged;

            public MacroHost(Action<string>? macro = null)
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