// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Core.Models;
using AudioConductor.Editor.Core.Models;
using AudioConductor.Editor.Core.Tools.Shared;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AudioConductor.Editor.Core.Tools.CodeGen
{
    /// <summary>
    ///     EditorWindow for managing CueEnumDefinition.
    /// </summary>
    internal sealed class CueEnumDefinitionEditorWindow : EditorWindow
    {
        private const string WindowTitle = "Cue Enum Definition";

        [SerializeField] private CueEnumDefinitionTreeView.State? treeViewState;

        private ObjectField? _assetField;
        private VisualElement? _assetInspector;
        private TextField? _classSuffixField;
        private IntegerField? _cueCountField;
        private TextField? _cueSheetNameField;
        private TextField? _defaultClassSuffixField;
        private TextField? _defaultNamespaceField;
        private TextField? _defaultOutputPathField;
        private VisualElement? _defaultSettings;

        private CueEnumDefinition? _definition;

        // UI elements
        private ObjectField? _definitionField;
        private VisualElement? _emptyInspectorHelpBox;
        private VisualElement? _fileEntryInspector;
        private TextField? _fileNameField;
        private TextField? _namespaceField;
        private TextField? _outputPathField;
        private TextField? _pathRuleField;
        private CueEnumDefinitionTreeItem? _selectedItem;
        private Label? _statusLabel;
        private string _statusMessage = "Ready";
        private CueEnumDefinitionTreeView? _treeView;
        private IMGUIContainer? _treeViewContainer;
        private Toggle? _useDefaultClassSuffixToggle;
        private Toggle? _useDefaultNamespaceToggle;
        private Toggle? _useDefaultOutputPathToggle;

        private void OnEnable()
        {
            _definition = CueEnumDefinitionRepository.instance.Definition;
            InitTreeView();
        }

        private void CreateGUI()
        {
            var tree = AssetLoader.LoadUxml("CueEnumDefinitionEditor");
            tree.CloneTree(rootVisualElement);

            QueryElements();
            SetupDefinitionField();
            SetupDefaultSettings();
            SetupTreeView();
            SetupTreeViewButtons();
            SetupInspector();
            SetupFooter();

            UpdateDefaultSettingsVisibility();
            UpdateDefaultSettingsValues();
            UpdateInspector();
        }

        [MenuItem("Tools/Audio Conductor/Cue Enum Definition")]
        private static void ShowWindow()
        {
            if (CueEnumDefinitionRepository.instance.Definition == null)
            {
                var ok = EditorUtility.DisplayDialog(
                    WindowTitle,
                    "CueEnumDefinition asset does not exist. A new one will be created.",
                    "OK",
                    "Cancel");

                if (!ok)
                    return;

                CueEnumDefinitionRepository.instance.GetOrCreate();
            }

            var window = GetWindow<CueEnumDefinitionEditorWindow>();
            window.titleContent = new GUIContent(WindowTitle);
            window.minSize = new Vector2(700, 400);
            window.Show();
        }

        private void QueryElements()
        {
            _definitionField = rootVisualElement.Q<ObjectField>("DefinitionField");
            _defaultSettings = rootVisualElement.Q<VisualElement>("DefaultSettings");
            _defaultOutputPathField = rootVisualElement.Q<TextField>("DefaultOutputPath");
            _defaultNamespaceField = rootVisualElement.Q<TextField>("DefaultNamespace");
            _defaultClassSuffixField = rootVisualElement.Q<TextField>("DefaultClassSuffix");
            _treeViewContainer = rootVisualElement.Q<IMGUIContainer>("TreeViewContainer");
            _emptyInspectorHelpBox = rootVisualElement.Q<VisualElement>("EmptyInspectorHelpBox");
            _fileEntryInspector = rootVisualElement.Q<VisualElement>("FileEntryInspector");
            _assetInspector = rootVisualElement.Q<VisualElement>("AssetInspector");
            _fileNameField = rootVisualElement.Q<TextField>("FileName");
            _useDefaultOutputPathToggle = rootVisualElement.Q<Toggle>("UseDefaultOutputPath");
            _outputPathField = rootVisualElement.Q<TextField>("OutputPath");
            _useDefaultNamespaceToggle = rootVisualElement.Q<Toggle>("UseDefaultNamespace");
            _namespaceField = rootVisualElement.Q<TextField>("Namespace");
            _useDefaultClassSuffixToggle = rootVisualElement.Q<Toggle>("UseDefaultClassSuffix");
            _classSuffixField = rootVisualElement.Q<TextField>("ClassSuffix");
            _pathRuleField = rootVisualElement.Q<TextField>("PathRule");
            _assetField = rootVisualElement.Q<ObjectField>("AssetField");
            _cueSheetNameField = rootVisualElement.Q<TextField>("CueSheetName");
            _cueCountField = rootVisualElement.Q<IntegerField>("CueCount");
            _statusLabel = rootVisualElement.Q<Label>("StatusLabel");
        }

        private void SetupDefinitionField()
        {
            if (_definitionField == null)
                return;

            _definitionField.objectType = typeof(CueEnumDefinition);
            _definitionField.value = _definition;
            _definitionField.RegisterValueChangedCallback(evt =>
            {
                _definition = evt.newValue as CueEnumDefinition;
                _treeView?.SetDefinition(_definition);
                UpdateDefaultSettingsVisibility();
                UpdateDefaultSettingsValues();
                UpdateInspector();
            });
        }

        private void SetupDefaultSettings()
        {
            _defaultOutputPathField?.RegisterValueChangedCallback(evt =>
            {
                if (_definition == null)
                    return;
                _definition.defaultOutputPath = evt.newValue;
                MarkDirtyAndSave();
                _treeView?.Reload();
            });

            _defaultNamespaceField?.RegisterValueChangedCallback(evt =>
            {
                if (_definition == null)
                    return;
                _definition.defaultNamespace = evt.newValue;
                MarkDirtyAndSave();
            });

            _defaultClassSuffixField?.RegisterValueChangedCallback(evt =>
            {
                if (_definition == null)
                    return;
                _definition.defaultClassSuffix = evt.newValue;
                MarkDirtyAndSave();
                _treeView?.Reload();
            });
        }

        private void InitTreeView()
        {
            treeViewState ??= new CueEnumDefinitionTreeView.State();
            _treeView = new CueEnumDefinitionTreeView(treeViewState);
            _treeView.OnSelectionChanged += item =>
            {
                _selectedItem = item;
                UpdateInspector();
            };
            _treeView.OnStructureChanged += () =>
            {
                if (_definition != null)
                    MarkDirtyAndSave();
            };
            _treeView.SetDefinition(_definition);
        }

        private void SetupTreeView()
        {
            if (_treeViewContainer == null)
                return;

            _treeViewContainer.onGUIHandler = OnTreeViewGUI;
        }

        private void OnTreeViewGUI()
        {
            if (_treeView == null)
                return;

            var rect = _treeViewContainer!.contentRect;
            _treeView.OnGUI(rect);
        }

        private void SetupTreeViewButtons()
        {
            var addButton = rootVisualElement.Q<Button>("AddFileGroupButton");
            addButton?.RegisterCallback<ClickEvent>(_ =>
            {
                if (_definition == null)
                    return;

                _definition.fileEntries.Add(new FileEntry { fileName = "NewFileGroup" });
                MarkDirtyAndSave();
                _treeView?.Reload();
            });

            var removeButton = rootVisualElement.Q<Button>("RemoveButton");
            removeButton?.RegisterCallback<ClickEvent>(_ => RemoveSelected());
        }

        private void SetupInspector()
        {
            // FileEntry inspector
            _fileNameField?.RegisterValueChangedCallback(evt =>
            {
                if (_selectedItem is FileEntryTreeItem feItem)
                {
                    feItem.FileEntry.fileName = evt.newValue;
                    MarkDirtyAndSave();
                    _treeView?.Reload();
                }
            });

            _useDefaultOutputPathToggle?.RegisterValueChangedCallback(evt =>
            {
                if (_selectedItem is FileEntryTreeItem feItem)
                {
                    feItem.FileEntry.useDefaultOutputPath = evt.newValue;
                    _outputPathField?.SetEnabled(!evt.newValue);
                    MarkDirtyAndSave();
                }
            });

            _outputPathField?.RegisterValueChangedCallback(evt =>
            {
                if (_selectedItem is FileEntryTreeItem feItem)
                {
                    feItem.FileEntry.outputPath = evt.newValue;
                    MarkDirtyAndSave();
                }
            });

            _useDefaultNamespaceToggle?.RegisterValueChangedCallback(evt =>
            {
                if (_selectedItem is FileEntryTreeItem feItem)
                {
                    feItem.FileEntry.useDefaultNamespace = evt.newValue;
                    _namespaceField?.SetEnabled(!evt.newValue);
                    MarkDirtyAndSave();
                }
            });

            _namespaceField?.RegisterValueChangedCallback(evt =>
            {
                if (_selectedItem is FileEntryTreeItem feItem)
                {
                    feItem.FileEntry.@namespace = evt.newValue;
                    MarkDirtyAndSave();
                }
            });

            _useDefaultClassSuffixToggle?.RegisterValueChangedCallback(evt =>
            {
                if (_selectedItem is FileEntryTreeItem feItem)
                {
                    feItem.FileEntry.useDefaultClassSuffix = evt.newValue;
                    _classSuffixField?.SetEnabled(!evt.newValue);
                    MarkDirtyAndSave();
                }
            });

            _classSuffixField?.RegisterValueChangedCallback(evt =>
            {
                if (_selectedItem is FileEntryTreeItem feItem)
                {
                    feItem.FileEntry.classSuffix = evt.newValue;
                    MarkDirtyAndSave();
                }
            });

            _pathRuleField?.RegisterValueChangedCallback(evt =>
            {
                if (_selectedItem is FileEntryTreeItem feItem)
                {
                    feItem.FileEntry.pathRule = evt.newValue;
                    MarkDirtyAndSave();
                }
            });
        }

        private void SetupFooter()
        {
            var generateButton = rootVisualElement.Q<Button>("GenerateButton");
            generateButton?.RegisterCallback<ClickEvent>(_ =>
            {
                if (_definition == null)
                    return;

                var result = CueEnumPipeline.Execute(_definition);
                _statusMessage = result.Success
                    ? $"Generated: {result.GeneratedCount}, Written: {result.WrittenCount}, Up to date: {result.UpToDateCount}"
                    : $"Failed with {result.Errors.Count} error(s). See console.";

                if (!result.Success)
                    foreach (var error in result.Errors)
                        Debug.LogError(error);

                UpdateStatusLabel();
            });
        }

        private void UpdateDefaultSettingsVisibility()
        {
            if (_defaultSettings == null)
                return;

            _defaultSettings.style.display = _definition != null ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void UpdateDefaultSettingsValues()
        {
            if (_definition == null)
                return;

            _defaultOutputPathField?.SetValueWithoutNotify(_definition.defaultOutputPath);
            _defaultNamespaceField?.SetValueWithoutNotify(_definition.defaultNamespace);
            _defaultClassSuffixField?.SetValueWithoutNotify(_definition.defaultClassSuffix);
        }

        private void UpdateInspector()
        {
            var showEmpty = _selectedItem == null;
            var showFileEntry = _selectedItem is FileEntryTreeItem;
            var showAsset = _selectedItem is CueSheetAssetTreeItem;

            SetDisplay(_emptyInspectorHelpBox, showEmpty);
            SetDisplay(_fileEntryInspector, showFileEntry);
            SetDisplay(_assetInspector, showAsset);

            if (showFileEntry)
                PopulateFileEntryInspector((FileEntryTreeItem)_selectedItem!);
            else if (showAsset)
                PopulateAssetInspector((CueSheetAssetTreeItem)_selectedItem!);
        }

        private void PopulateFileEntryInspector(FileEntryTreeItem feItem)
        {
            var fe = feItem.FileEntry;
            _fileNameField?.SetValueWithoutNotify(fe.fileName);
            _useDefaultOutputPathToggle?.SetValueWithoutNotify(fe.useDefaultOutputPath);
            _outputPathField?.SetValueWithoutNotify(fe.outputPath);
            _outputPathField?.SetEnabled(!fe.useDefaultOutputPath);
            _useDefaultNamespaceToggle?.SetValueWithoutNotify(fe.useDefaultNamespace);
            _namespaceField?.SetValueWithoutNotify(fe.@namespace);
            _namespaceField?.SetEnabled(!fe.useDefaultNamespace);
            _useDefaultClassSuffixToggle?.SetValueWithoutNotify(fe.useDefaultClassSuffix);
            _classSuffixField?.SetValueWithoutNotify(fe.classSuffix);
            _classSuffixField?.SetEnabled(!fe.useDefaultClassSuffix);
            _pathRuleField?.SetValueWithoutNotify(fe.pathRule);
        }

        private void PopulateAssetInspector(CueSheetAssetTreeItem assetItem)
        {
            if (assetItem.Asset == null)
                return;

            if (_assetField != null)
            {
                _assetField.objectType = typeof(CueSheetAsset);
                _assetField.SetEnabled(false);
                _assetField.SetValueWithoutNotify(assetItem.Asset);
            }

            _cueSheetNameField?.SetValueWithoutNotify(assetItem.Asset.cueSheet.name);
            _cueSheetNameField?.SetEnabled(false);
            _cueCountField?.SetValueWithoutNotify(assetItem.Asset.cueSheet.cueList.Count);
            _cueCountField?.SetEnabled(false);
        }

        private void UpdateStatusLabel()
        {
            if (_statusLabel != null)
                _statusLabel.text = $"Status: {_statusMessage}";
        }

        private void RemoveSelected()
        {
            if (_definition == null || _treeView == null)
                return;

            var selection = _treeView.GetSelection();
            if (selection == null || selection.Count == 0)
                return;

            foreach (var id in selection)
            {
                var item = _treeView.FindItemById(id);
                if (item is CueSheetAssetTreeItem assetItem && assetItem.Asset != null)
                {
                    _definition.rootEntries.Remove(assetItem.Asset);
                    foreach (var fe in _definition.fileEntries)
                        fe.assets.Remove(assetItem.Asset);
                }
                else if (item is FileEntryTreeItem feItem)
                {
                    _definition.fileEntries.Remove(feItem.FileEntry);
                }
            }

            MarkDirtyAndSave();
            _treeView.Reload();
            _selectedItem = null;
            UpdateInspector();
        }

        private void MarkDirtyAndSave()
        {
            if (_definition == null)
                return;

            EditorUtility.SetDirty(_definition);
            AssetDatabase.SaveAssets();
        }

        private static void SetDisplay(VisualElement? element, bool visible)
        {
            if (element != null)
                element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
