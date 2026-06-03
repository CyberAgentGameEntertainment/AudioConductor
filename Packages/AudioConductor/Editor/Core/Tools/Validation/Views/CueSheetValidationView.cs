// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using System.Collections.Generic;
using AudioConductor.Core.Models;
using AudioConductor.Editor.Core.Tools.Shared;
using AudioConductor.Editor.Core.Tools.Validation.Models;
using AudioConductor.Editor.Foundation.TinyRx;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace AudioConductor.Editor.Core.Tools.Validation.Views
{
    internal sealed class CueSheetValidationView : ICueSheetValidationView
    {
        private readonly VisualElement _root;

        private readonly Subject<ValidationResultRow> _rowSelected = new();
        private readonly Subject<AudioConductorSettings?> _settingsChanged = new();
        private readonly Subject<Empty> _validateClicked = new();

        private AudioConductorSettings[] _allSettings = Array.Empty<AudioConductorSettings>();

        private List<ValidationResultRow> _currentRows = new();
        private VisualElement? _emptyHelpBox;
        private VisualElement? _noResultHelpBox;
        private ListView? _resultListView;
        private DropdownField? _settingsDropdown;
        private string[] _settingsLabels = Array.Empty<string>();
        private ToolbarButton? _validateButton;

        public CueSheetValidationView(VisualElement root)
        {
            _root = root;
            var tree = AssetLoader.LoadUxml("CueSheetValidation");
            tree.CloneTree(root);
        }

        public IObservable<ValidationResultRow> RowSelectedAsObservable => _rowSelected;
        public IObservable<AudioConductorSettings?> SettingsChangedAsObservable => _settingsChanged;
        public IObservable<Empty> ValidateClickedAsObservable => _validateClicked;

        public void Dispose()
        {
            CleanupEventHandlers();
            _rowSelected.Dispose();
            _settingsChanged.Dispose();
            _validateClicked.Dispose();
        }

        public void Setup(AudioConductorSettings[] allSettings, bool showSettingsDropdown)
        {
            _allSettings = allSettings;
            _settingsLabels = BuildChoiceLabels(allSettings);

            _settingsDropdown = _root.Q<DropdownField>("SettingsDropdown");
            _validateButton = _root.Q<ToolbarButton>("ValidateButton");
            _resultListView = _root.Q<ListView>("ResultListView");
            _emptyHelpBox = _root.Q<VisualElement>("EmptyHelpBox");
            _noResultHelpBox = _root.Q<VisualElement>("NoResultHelpBox");

            if (_settingsDropdown is not null)
            {
                _settingsDropdown.SetDisplay(showSettingsDropdown);
                _settingsDropdown.choices = new List<string>(_settingsLabels);
            }

            if (_resultListView is not null)
            {
                _resultListView.makeItem = MakeItem;
                _resultListView.bindItem = BindItem;
                _resultListView.itemsSource = _currentRows;
            }

            SetupEventHandlers();
        }

        public void RenderResults(IReadOnlyList<ValidationResultRow> rows)
        {
            _currentRows = new List<ValidationResultRow>(rows);

            if (_resultListView is not null)
            {
                _resultListView.itemsSource = _currentRows;
                _resultListView.Rebuild();
            }

            var hasIssues = false;
            foreach (var row in _currentRows)
                if (row.Issue is not null)
                {
                    hasIssues = true;
                    break;
                }

            ApplyDisplayState(hasIssues ? ValidationDisplayState.HasIssues : ValidationDisplayState.NoIssues);
        }

        public void SetSelectedSettings(AudioConductorSettings? settings)
        {
            if (_settingsDropdown is null)
                return;

            if (settings == null)
            {
                _settingsDropdown.SetValueWithoutNotify(string.Empty);
                return;
            }

            var index = Array.IndexOf(_allSettings, settings);
            if (index >= 0)
                _settingsDropdown.SetValueWithoutNotify(_settingsLabels[index]);
        }

        public void ShowEmpty()
        {
            ApplyDisplayState(ValidationDisplayState.Empty);
        }

        private void ApplyDisplayState(ValidationDisplayState state)
        {
            _resultListView.SetDisplay(state == ValidationDisplayState.HasIssues);
            _emptyHelpBox.SetDisplay(state == ValidationDisplayState.Empty);
            _noResultHelpBox.SetDisplay(state == ValidationDisplayState.NoIssues);
        }

        private static VisualElement MakeItem()
        {
            var row = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    paddingLeft = 4,
                    paddingRight = 4
                }
            };

            var icon = new Image
            {
                name = "Icon",
                style =
                {
                    width = 16,
                    height = 16,
                    marginRight = 4
                }
            };
            row.Add(icon);

            var label = new Label
            {
                name = "Label",
                style =
                {
                    flexGrow = 1
                }
            };
            row.Add(label);

            return row;
        }

        private void BindItem(VisualElement element, int index)
        {
            if (index < 0 || index >= _currentRows.Count)
                return;

            var row = _currentRows[index];
            var icon = element.Q<Image>("Icon");
            var label = element.Q<Label>("Label");

            const float indentWidth = 16f;
            element.style.paddingLeft = 4 + (row.IsIssueRow ? indentWidth : 0f);

            if (row.Issue is not null)
            {
                var iconName = row.Issue.Severity == ValidationSeverity.Error
                    ? "console.erroricon.sml"
                    : "console.warnicon.sml";
                icon.image = EditorGUIUtility.IconContent(iconName).image;
                icon.SetDisplay(true);
            }
            else
            {
                icon.image = null;
                icon.SetDisplay(false);
            }

            label.text = row.DisplayText;
        }

        private void SetupEventHandlers()
        {
            if (_settingsDropdown is not null)
                _settingsDropdown.RegisterValueChangedCallback(OnSettingsChanged);

            if (_validateButton is not null)
                _validateButton.clicked += OnValidateClicked;

            if (_resultListView is not null)
                _resultListView.selectionChanged += OnSelectionChanged;
        }

        private void CleanupEventHandlers()
        {
            if (_settingsDropdown is not null)
                _settingsDropdown.UnregisterValueChangedCallback(OnSettingsChanged);

            if (_validateButton is not null)
                _validateButton.clicked -= OnValidateClicked;

            if (_resultListView is not null)
                _resultListView.selectionChanged -= OnSelectionChanged;
        }

        private void OnSettingsChanged(ChangeEvent<string> e)
        {
            if (_settingsDropdown is null)
                return;

            var index = _settingsDropdown.index;
            if (index >= 0 && index < _allSettings.Length)
                _settingsChanged.OnNext(_allSettings[index]);
            else
                _settingsChanged.OnNext(null);
        }

        private void OnValidateClicked()
        {
            _validateClicked.OnNext(Empty.Default);
        }

        private void OnSelectionChanged(IEnumerable<object> selected)
        {
            foreach (var obj in selected)
                if (obj is ValidationResultRow row)
                    _rowSelected.OnNext(row);
        }

        private static string[] BuildChoiceLabels(AudioConductorSettings[] settings)
        {
            var seen = new HashSet<string>();
            var hasDuplicateName = false;
            foreach (var s in settings)
            {
                if (s == null)
                    continue;
                if (!seen.Add(s.name))
                {
                    hasDuplicateName = true;
                    break;
                }
            }

            var labels = new string[settings.Length];
            for (var i = 0; i < settings.Length; i++)
            {
                var s = settings[i];
                labels[i] = s != null
                    ? hasDuplicateName ? $"{s.name} ({AssetDatabase.GetAssetPath(s)})" : s.name
                    : string.Empty;
            }

            return labels;
        }

        private enum ValidationDisplayState
        {
            Empty,
            NoIssues,
            HasIssues
        }
    }
}
