// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

using System.Linq;
using AudioConductor.Editor.Core.Tools.CueSheetEditor.Models;
using AudioConductor.Editor.Core.Tools.CueSheetEditor.Presenters;
using AudioConductor.Editor.Core.Tools.CueSheetEditor.Views;
using AudioConductor.Editor.Core.Tools.Shared;
using AudioConductor.Editor.Foundation.TinyRx;
using AudioConductor.Runtime.Core.Models;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor
{
    internal sealed class CueSheetAssetEditorWindow : EditorWindow
    {
        private const KeyCode UndoKey = KeyCode.Z;
        private const KeyCode RedoKey = KeyCode.Y;
        private const string SelectedSettingsGuidPrefKey = "AudioConductor.SelectedSettingsGuid";

        [SerializeField] private CueSheetAssetEditorWindowModel _target;
        [SerializeField] private string _selectedSettingsGuid;

        private readonly CompositeDisposable _disposable = new();

        private CueSheetEditorPresenter _cueSheetEditorPresenter;
        private DropdownField _settingsDropdown;

        private void OnEnable()
        {
            RefreshSettingsDropdown();
        }

        private void OnDisable()
        {
            Cleanup();
        }

        private void OnGUI()
        {
            var e = Event.current;

            if (GetEventAction(e) && e.type == EventType.KeyDown && e.keyCode == UndoKey)
            {
                _target.Undo();
                e.Use();
            }

            if (GetEventAction(e) && e.type == EventType.KeyDown && e.keyCode == RedoKey)
            {
                _target.Redo();
                e.Use();
            }
        }

        private void CreateGUI()
        {
            _target.Setup(GetSelectedSettings);

            _target.CueSheetEditorModel
                .CueSheetParameterPaneModel
                .NameObservable
                .Subscribe(cueSheetName =>
                {
                    titleContent =
                        new GUIContent(string.IsNullOrWhiteSpace(cueSheetName)
                            ? "No Title"
                            : cueSheetName);
                })
                .DisposeWith(_disposable);

            rootVisualElement.RegisterCallback<KeyDownEvent>(HandleKeyDownEvent);

            var view = new CueSheetEditorView(rootVisualElement);
            _cueSheetEditorPresenter = new CueSheetEditorPresenter(_target.CueSheetEditorModel, view);
            _cueSheetEditorPresenter.Setup();

            _settingsDropdown = rootVisualElement.Q<DropdownField>("SettingsDropdown");
            _settingsDropdown.RegisterValueChangedCallback(_ => ApplySelectedSettings());

            RefreshSettingsDropdown();
        }

        private void OnFocus()
        {
            RefreshSettingsDropdown();
            CategoryListRepository.instance.Refresh(GetSelectedSettings());
            ColorDefineListRepository.instance.Update();
        }

        private AudioConductorSettings GetSelectedSettings()
        {
            return AudioConductorSettingsRepository.instance.GetByGuid(_selectedSettingsGuid);
        }

        public static void Open(CueSheetAsset cueSheetAsset)
        {
            var openedWindows = Resources.FindObjectsOfTypeAll<CueSheetAssetEditorWindow>();
            var sameCueSheetWindow =
                openedWindows.FirstOrDefault(window => window._target.CueSheetId == cueSheetAsset.cueSheet.Id);
            if (sameCueSheetWindow != null)
            {
                sameCueSheetWindow.Focus();
                return;
            }

            var window = CreateInstance<CueSheetAssetEditorWindow>();
            window._target = new CueSheetAssetEditorWindowModel(cueSheetAsset);
            window.minSize = new Vector2(1340, 700);
            window.Show();
        }

        private void Cleanup()
        {
            rootVisualElement.UnregisterCallback<KeyDownEvent>(HandleKeyDownEvent);

            _disposable.Clear();

            _cueSheetEditorPresenter?.Dispose();
        }

        private void RefreshSettingsDropdown()
        {
            _settingsDropdown ??= rootVisualElement?.Q<DropdownField>("SettingsDropdown");

            var allSettings = AudioConductorSettingsRepository.instance.AllSettings;
            if (allSettings == null || allSettings.Length == 0)
            {
                if (_settingsDropdown != null)
                    _settingsDropdown.style.display = DisplayStyle.None;
                return;
            }

            if (allSettings.Length == 1)
            {
                if (_settingsDropdown != null)
                    _settingsDropdown.style.display = DisplayStyle.None;
                _selectedSettingsGuid =
                    AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(allSettings[0]));
                return;
            }

            var choices = allSettings.Select(s => s != null ? s.name : "(Missing)").ToList();

            // Restore per-window selection first, then fall back to EditorPrefs default.
            var index = FindSettingsIndex(allSettings, _selectedSettingsGuid);
            if (index < 0)
                index = FindSettingsIndex(allSettings,
                    EditorPrefs.GetString(SelectedSettingsGuidPrefKey, string.Empty));
            if (index < 0)
                index = 0;

            _selectedSettingsGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(allSettings[index]));

            if (_settingsDropdown != null)
            {
                _settingsDropdown.choices = choices;
                _settingsDropdown.SetValueWithoutNotify(choices[index]);
                _settingsDropdown.style.display = DisplayStyle.Flex;
            }
        }

        private static int FindSettingsIndex(AudioConductorSettings[] allSettings, string guid)
        {
            if (string.IsNullOrEmpty(guid))
                return -1;

            var path = AssetDatabase.GUIDToAssetPath(guid);
            for (var i = 0; i < allSettings.Length; i++)
                if (AssetDatabase.GetAssetPath(allSettings[i]) == path)
                    return i;

            return -1;
        }

        private void ApplySelectedSettings()
        {
            var allSettings = AudioConductorSettingsRepository.instance.AllSettings;
            if (_settingsDropdown == null || allSettings == null)
                return;

            var index = _settingsDropdown.index;
            if (index < 0 || index >= allSettings.Length)
                return;

            var selected = allSettings[index];

            // Persist per-window selection and global default.
            if (selected != null)
            {
                var assetPath = AssetDatabase.GetAssetPath(selected);
                var guid = AssetDatabase.AssetPathToGUID(assetPath);
                _selectedSettingsGuid = guid;
                EditorPrefs.SetString(SelectedSettingsGuidPrefKey, guid);
            }

            CategoryListRepository.instance.Refresh(selected);
        }

        private static bool GetEventAction(Event e)
        {
#if UNITY_EDITOR_WIN
            return e.control;
#else
            return e.command;
#endif
        }

        private void HandleKeyDownEvent(KeyDownEvent e)
        {
            if (GetEventAction(e) && e.keyCode == UndoKey)
            {
                _target.Undo();
                e.StopPropagation();
            }

            if (GetEventAction(e) && e.keyCode == RedoKey)
            {
                _target.Redo();
                e.StopPropagation();
            }
        }

        private static bool GetEventAction(IKeyboardEvent e)
        {
#if UNITY_EDITOR_WIN
            return e.ctrlKey;
#else
            return e.commandKey;
#endif
        }
    }
}
