using HMUI;
using System;
using Zenject;
using UnityEngine;
using System.Linq;
using Actions.Dashboard;
using System.ComponentModel;
using Actions.UI.Dashboards;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;

namespace Actions.UI
{
    [ViewDefinition("Actions.Views.macro-view.bsml")]
    [HotReload(RelativePathToLayout = @"..\Views\macro-view.bsml")]
    internal class ActionMacroView : BSMLAutomaticViewController
    {
        [Inject]
        protected readonly Config _config = null!;

        [Inject]
        protected readonly MacroDash _macroDash = null!;

        [UIComponent("editor-modal")]
        protected readonly ModalView _editorModal = null!;

        [UIParams]
        protected readonly BSMLParserParams parserParams = null!;

        [UIComponent("macro-list")]
        protected readonly CustomCellListTableData macroTable = null!;

        [UIValue("keyboard-text")]
        protected string keyboardText = "";

        private string _editorTitle = "Edit Macro";
        [UIValue("editor-title")]
        public string EditorTitle
        {
            get => _editorTitle;
            set
            {
                _editorTitle = value;
                NotifyPropertyChanged();
            }
        }

        private string _nameText = "";
        [UIValue("name-text")]
        public string NameText
        {
            get => _nameText;
            set
            {
                _nameText = value;
                NotifyPropertyChanged();
            }
        }

        private string _contentText = "";
        [UIValue("content-text")]
        public string ContentText
        {
            get => _contentText;
            set
            {
                _contentText = value;
                NotifyPropertyChanged();
            }
        }

        [UIValue("is-command-value")]
        public bool IsCommandValue { get; set; }

        [UIValue("within-limit")]
        protected bool WithinLimit => macroTable.data.Count < 21;

        protected Action<string>? _keyboardEntered;
        protected Action<bool>? _onCommand;
        protected Action? _onContent;
        protected Action? _onName;

        protected override async void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            if (firstActivation)
            {
                macroTable.data.Clear();
                foreach (Transform t in macroTable.tableView.contentTransform)
                {
                    Destroy(t.gameObject);
                }
                foreach (var macro in _config.Macros.Take(21))
                {
                    macroTable.data.Add(new Host(macro, EditMacro, DeleteMacro));
                }
            }
            NotifyPropertyChanged(nameof(WithinLimit));
            await SiraUtil.Utilities.PauseChamp;
            macroTable.tableView.ReloadData();
        }

        [UIAction("new-macro")]
        protected void NewMacro()
        {
            Macro macro = new Macro
            {
                Name = "",
                Content = "",
                IsCommand = false,
            };
            var host = new Host(macro, EditMacro, DeleteMacro);
            SetEditorDetails(host, true);

            parserParams?.EmitEvent("show-editor");
            _editorModal.blockerClickedEvent += Dismissed;
            void Dismissed()
            {
                _editorModal.blockerClickedEvent -= Dismissed;
                if (!string.IsNullOrWhiteSpace(macro.Name) && !string.IsNullOrWhiteSpace(macro.Content))
                {
                    _config.Macros.Add(macro);
                    macroTable.data.Add(host);
                    macroTable.tableView.ReloadData();
                    NotifyPropertyChanged(nameof(WithinLimit));
                    _macroDash.MacroCreated(macro);
                }
            }
        }

        private void EditMacro(Host host)
        {
            SetEditorDetails(host);
            parserParams?.EmitEvent("show-editor");
            _editorModal.blockerClickedEvent += Dismissed;
            void Dismissed()
            {
                _editorModal.blockerClickedEvent -= Dismissed;
                if (string.IsNullOrWhiteSpace(host.macro.Name))
                    host.Name = "NOT SET";
                if (string.IsNullOrWhiteSpace(host.macro.Content))
                    host.Content = "NOT SET";

                _macroDash.MacroEdited(host.macro);
            }
        }

        private void DeleteMacro(Host host)
        {
            macroTable.data.Remove(host);
            _config.Macros.Remove(host.macro);
            macroTable.tableView.ReloadData();
            _macroDash.MacroDeleted(host.macro);
        }

        // SELF CONTAINED AS HELL
        private void SetEditorDetails(Host macro, bool isNew = false)
        {
            _onCommand = null;
            _onContent = null;
            _onName = null;

            EditorTitle = isNew ? "New Macro" : "Edit Macro";
            ContentText = string.IsNullOrWhiteSpace(macro.Content) ? "Click To Edit" : macro.Content;
            NameText = string.IsNullOrWhiteSpace(macro.Name) ? "Click To Edit" : macro.Name;
            IsCommandValue = macro.IsCommand;
            parserParams?.EmitEvent("cmd");

            _onCommand = delegate (bool value) { macro.IsCommand = value; };
            _onContent = delegate ()
            {
                parserParams?.EmitEvent("hide");
                parserParams?.EmitEvent("show-keyboard");
                _keyboardEntered = delegate (string value)
                {
                    parserParams?.EmitEvent("hide");
                    parserParams?.EmitEvent("show-editor");
                    macro.Content = value;
                    ContentText = value;
                };
            };
            _onName = delegate ()
            {
                parserParams?.EmitEvent("hide");
                parserParams?.EmitEvent("show-keyboard");
                _keyboardEntered = delegate (string value)
                {
                    parserParams?.EmitEvent("hide");
                    parserParams?.EmitEvent("show-editor");
                    macro.Name = value;
                    NameText = value;
                };
            };
        }

        [UIAction("keyboard-entered")]
        protected void Entered(string value) => _keyboardEntered?.Invoke(value);

        [UIAction("clicked-name")]
        protected void ClickedName() => _onName?.Invoke();

        [UIAction("clicked-content")]
        protected void ClickedContent() => _onContent?.Invoke();

        [UIAction("command-value-changed")]
        protected void CommandValueChanged(bool value) => _onCommand?.Invoke(value); 

        public class Host : INotifyPropertyChanged
        {
            public readonly Macro macro;
            private readonly Action<Host> _edit;
            private readonly Action<Host> _delete;
            public event PropertyChangedEventHandler? PropertyChanged;

            [UIValue("name")]
            public string Name
            {
                get => macro.Name;
                set
                {
                    macro.Name = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
                }
            }

            [UIValue("rich-content")]
            protected string RichContent => $"<color=white>{(IsCommand ? "executes" : "says" )}</color> <color=#9c9c9c>{Content}</color>";

            [UIValue("content")]
            public string Content
            {
                get => macro.Content;
                set
                {
                    macro.Content = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Content)));
                }
            }

            [UIValue("is-command")]
            public bool IsCommand
            {
                get => macro.IsCommand;
                set
                {
                    macro.IsCommand = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsCommand)));
                }
            }

            public Host(Macro macro, Action<Host> edit, Action<Host> delete)
            {
                _edit = edit;
                _delete = delete;
                this.macro = macro;
            }

            [UIAction("edit")]
            protected void Edit() => _edit.Invoke(this);

            [UIAction("delete")]
            protected void Delete() => _delete.Invoke(this);
        }
    }
}