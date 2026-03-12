// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.Collections.Generic;
using AudioConductor.Core.Models;
using AudioConductor.Editor.Core.Models;
using AudioConductor.Editor.Core.Tools.CueSheetEditor;
using AudioConductor.Editor.Core.Tools.Shared;
using AudioConductor.Editor.Foundation.CommandBasedUndo;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using L = AudioConductor.Editor.Localization.Localization;

namespace AudioConductor.Editor.Core.Tools.CodeGen
{
    /// <summary>
    ///     EditorWindow for managing CueEnumDefinition.
    /// </summary>
    internal sealed class CueEnumDefinitionEditorWindow : EditorWindow
    {
        private const string WindowTitle = "Cue Enum Definition";
        private const KeyCode UndoKey = KeyCode.Z;
        private const KeyCode RedoKey = KeyCode.Y;

        [SerializeField] private CueEnumDefinitionTreeView.State? treeViewState;

        private readonly AutoIncrementHistory _history = new();

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
        private Button? _generateButton;
        private TextField? _namespaceField;
        private Button? _openCueSheetEditorButton;
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
            L.LanguageChanged += OnLanguageChanged;
            EditorApplication.projectChanged += OnProjectChanged;
        }

        private void OnDisable()
        {
            L.LanguageChanged -= OnLanguageChanged;
            EditorApplication.projectChanged -= OnProjectChanged;
            rootVisualElement.UnregisterCallback<KeyDownEvent>(HandleKeyDownEvent);
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

            ApplyTooltips();
            UpdateDefaultSettingsVisibility();
            UpdateDefaultSettingsValues();
            UpdateInspector();

            rootVisualElement.RegisterCallback<KeyDownEvent>(HandleKeyDownEvent);
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

            OpenWindow();
        }

        internal static void Open(CueEnumDefinition definition)
        {
            var window = OpenWindow();
            if (window._definition != definition)
            {
                window._definition = definition;
                window._history.Clear();
                CueEnumDefinitionRepository.instance.SetDefinition(definition);
                window._treeView?.SetDefinition(definition);
                window._definitionField?.SetValueWithoutNotify(definition);
                window.UpdateDefaultSettingsVisibility();
                window.UpdateDefaultSettingsValues();
                window.UpdateInspector();
            }
        }

        private static CueEnumDefinitionEditorWindow OpenWindow()
        {
            var window = GetWindow<CueEnumDefinitionEditorWindow>();
            window.titleContent = new GUIContent(WindowTitle);
            window.minSize = new Vector2(700, 400);
            window.Show();
            return window;
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
            _openCueSheetEditorButton = rootVisualElement.Q<Button>("OpenCueSheetEditorButton");
            _generateButton = rootVisualElement.Q<Button>("GenerateButton");
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
                _history.Clear();
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
                var oldValue = evt.previousValue;
                var newValue = evt.newValue;
                _history.Register(
                    $"Set Default OutputPath {newValue}",
                    () =>
                    {
                        _definition.defaultOutputPath = newValue;
                        _defaultOutputPathField?.SetValueWithoutNotify(newValue);
                        MarkDirtyAndSave();
                        _treeView?.Reload();
                    },
                    () =>
                    {
                        _definition.defaultOutputPath = oldValue;
                        _defaultOutputPathField?.SetValueWithoutNotify(oldValue);
                        MarkDirtyAndSave();
                        _treeView?.Reload();
                    });
            });

            _defaultNamespaceField?.RegisterValueChangedCallback(evt =>
            {
                if (_definition == null)
                    return;
                var oldValue = evt.previousValue;
                var newValue = evt.newValue;
                _history.Register(
                    $"Set Default Namespace {newValue}",
                    () =>
                    {
                        _definition.defaultNamespace = newValue;
                        _defaultNamespaceField?.SetValueWithoutNotify(newValue);
                        MarkDirtyAndSave();
                    },
                    () =>
                    {
                        _definition.defaultNamespace = oldValue;
                        _defaultNamespaceField?.SetValueWithoutNotify(oldValue);
                        MarkDirtyAndSave();
                    });
            });

            _defaultClassSuffixField?.RegisterValueChangedCallback(evt =>
            {
                if (_definition == null)
                    return;
                var oldValue = evt.previousValue;
                var newValue = evt.newValue;
                _history.Register(
                    $"Set Default ClassSuffix {newValue}",
                    () =>
                    {
                        _definition.defaultClassSuffix = newValue;
                        _defaultClassSuffixField?.SetValueWithoutNotify(newValue);
                        MarkDirtyAndSave();
                        _treeView?.Reload();
                    },
                    () =>
                    {
                        _definition.defaultClassSuffix = oldValue;
                        _defaultClassSuffixField?.SetValueWithoutNotify(oldValue);
                        MarkDirtyAndSave();
                        _treeView?.Reload();
                    });
            });
        }

        private void InitTreeView()
        {
            treeViewState ??= new CueEnumDefinitionTreeView.State();
            _treeView = new CueEnumDefinitionTreeView(treeViewState);
            _treeView.SetHistory(_history);
            _treeView.OnSelectionChanged += item =>
            {
                _selectedItem = item;
                UpdateInspector();
            };
            _treeView.OnStructureChanged += () =>
            {
                if (_definition != null)
                {
                    MarkDirtyAndSave();
                    _treeView?.Reload();
                    UpdateInspector();
                }
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

            var e = Event.current;
            if (GetEventAction(e) && e.type == EventType.KeyDown)
            {
                if (e.keyCode == UndoKey)
                {
                    PerformUndo();
                    e.Use();
                }
                else if (e.keyCode == RedoKey)
                {
                    PerformRedo();
                    e.Use();
                }
            }
        }

        private void SetupTreeViewButtons()
        {
            var addButton = rootVisualElement.Q<Button>("AddFileGroupButton");
            addButton?.RegisterCallback<ClickEvent>(_ =>
            {
                if (_definition == null)
                    return;

                var newEntry = new FileEntry { fileName = "NewFileGroup" };
                _history.Register(
                    "Add FileGroup",
                    () =>
                    {
                        _definition.fileEntries.Add(newEntry);
                        MarkDirtyAndSave();
                        _treeView?.Reload();
                    },
                    () =>
                    {
                        _definition.fileEntries.Remove(newEntry);
                        MarkDirtyAndSave();
                        _treeView?.Reload();
                    });
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
                    var oldValue = evt.previousValue;
                    var newValue = evt.newValue;
                    var fe = feItem.FileEntry;
                    _history.Register(
                        $"Set FileName {newValue}",
                        () =>
                        {
                            fe.fileName = newValue;
                            _fileNameField?.SetValueWithoutNotify(newValue);
                            MarkDirtyAndSave();
                            _treeView?.Reload();
                        },
                        () =>
                        {
                            fe.fileName = oldValue;
                            _fileNameField?.SetValueWithoutNotify(oldValue);
                            MarkDirtyAndSave();
                            _treeView?.Reload();
                        });
                }
            });

            _useDefaultOutputPathToggle?.RegisterValueChangedCallback(evt =>
            {
                if (_selectedItem is FileEntryTreeItem feItem)
                {
                    var oldValue = evt.previousValue;
                    var newValue = evt.newValue;
                    var fe = feItem.FileEntry;
                    _history.Register(
                        $"Set UseDefaultOutputPath {newValue}",
                        () =>
                        {
                            fe.useDefaultOutputPath = newValue;
                            _useDefaultOutputPathToggle?.SetValueWithoutNotify(newValue);
                            _outputPathField?.SetEnabled(!newValue);
                            MarkDirtyAndSave();
                        },
                        () =>
                        {
                            fe.useDefaultOutputPath = oldValue;
                            _useDefaultOutputPathToggle?.SetValueWithoutNotify(oldValue);
                            _outputPathField?.SetEnabled(!oldValue);
                            MarkDirtyAndSave();
                        });
                }
            });

            _outputPathField?.RegisterValueChangedCallback(evt =>
            {
                if (_selectedItem is FileEntryTreeItem feItem)
                {
                    var oldValue = evt.previousValue;
                    var newValue = evt.newValue;
                    var fe = feItem.FileEntry;
                    _history.Register(
                        $"Set OutputPath {newValue}",
                        () =>
                        {
                            fe.outputPath = newValue;
                            _outputPathField?.SetValueWithoutNotify(newValue);
                            MarkDirtyAndSave();
                        },
                        () =>
                        {
                            fe.outputPath = oldValue;
                            _outputPathField?.SetValueWithoutNotify(oldValue);
                            MarkDirtyAndSave();
                        });
                }
            });

            _useDefaultNamespaceToggle?.RegisterValueChangedCallback(evt =>
            {
                if (_selectedItem is FileEntryTreeItem feItem)
                {
                    var oldValue = evt.previousValue;
                    var newValue = evt.newValue;
                    var fe = feItem.FileEntry;
                    _history.Register(
                        $"Set UseDefaultNamespace {newValue}",
                        () =>
                        {
                            fe.useDefaultNamespace = newValue;
                            _useDefaultNamespaceToggle?.SetValueWithoutNotify(newValue);
                            _namespaceField?.SetEnabled(!newValue);
                            MarkDirtyAndSave();
                        },
                        () =>
                        {
                            fe.useDefaultNamespace = oldValue;
                            _useDefaultNamespaceToggle?.SetValueWithoutNotify(oldValue);
                            _namespaceField?.SetEnabled(!oldValue);
                            MarkDirtyAndSave();
                        });
                }
            });

            _namespaceField?.RegisterValueChangedCallback(evt =>
            {
                if (_selectedItem is FileEntryTreeItem feItem)
                {
                    var oldValue = evt.previousValue;
                    var newValue = evt.newValue;
                    var fe = feItem.FileEntry;
                    _history.Register(
                        $"Set Namespace {newValue}",
                        () =>
                        {
                            fe.@namespace = newValue;
                            _namespaceField?.SetValueWithoutNotify(newValue);
                            MarkDirtyAndSave();
                        },
                        () =>
                        {
                            fe.@namespace = oldValue;
                            _namespaceField?.SetValueWithoutNotify(oldValue);
                            MarkDirtyAndSave();
                        });
                }
            });

            _useDefaultClassSuffixToggle?.RegisterValueChangedCallback(evt =>
            {
                if (_selectedItem is FileEntryTreeItem feItem)
                {
                    var oldValue = evt.previousValue;
                    var newValue = evt.newValue;
                    var fe = feItem.FileEntry;
                    _history.Register(
                        $"Set UseDefaultClassSuffix {newValue}",
                        () =>
                        {
                            fe.useDefaultClassSuffix = newValue;
                            _useDefaultClassSuffixToggle?.SetValueWithoutNotify(newValue);
                            _classSuffixField?.SetEnabled(!newValue);
                            MarkDirtyAndSave();
                        },
                        () =>
                        {
                            fe.useDefaultClassSuffix = oldValue;
                            _useDefaultClassSuffixToggle?.SetValueWithoutNotify(oldValue);
                            _classSuffixField?.SetEnabled(!oldValue);
                            MarkDirtyAndSave();
                        });
                }
            });

            _classSuffixField?.RegisterValueChangedCallback(evt =>
            {
                if (_selectedItem is FileEntryTreeItem feItem)
                {
                    var oldValue = evt.previousValue;
                    var newValue = evt.newValue;
                    var fe = feItem.FileEntry;
                    _history.Register(
                        $"Set ClassSuffix {newValue}",
                        () =>
                        {
                            fe.classSuffix = newValue;
                            _classSuffixField?.SetValueWithoutNotify(newValue);
                            MarkDirtyAndSave();
                        },
                        () =>
                        {
                            fe.classSuffix = oldValue;
                            _classSuffixField?.SetValueWithoutNotify(oldValue);
                            MarkDirtyAndSave();
                        });
                }
            });

            _pathRuleField?.RegisterValueChangedCallback(evt =>
            {
                if (_selectedItem is FileEntryTreeItem feItem)
                {
                    var oldValue = evt.previousValue;
                    var newValue = evt.newValue;
                    var fe = feItem.FileEntry;
                    _history.Register(
                        $"Set PathRule {newValue}",
                        () =>
                        {
                            fe.pathRule = newValue;
                            _pathRuleField?.SetValueWithoutNotify(newValue);
                            MarkDirtyAndSave();
                        },
                        () =>
                        {
                            fe.pathRule = oldValue;
                            _pathRuleField?.SetValueWithoutNotify(oldValue);
                            MarkDirtyAndSave();
                        });
                }
            });
        }

        private void SetupFooter()
        {
            _generateButton?.RegisterCallback<ClickEvent>(_ =>
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

            if (_openCueSheetEditorButton != null)
                _openCueSheetEditorButton.clickable = new Clickable(() =>
                {
                    if (assetItem.Asset != null)
                        CueSheetAssetEditorWindow.Open(assetItem.Asset);
                });
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

            // Capture removal targets
            var removedAssets = new List<(CueSheetAsset asset, int rootIndex, int feIndex, int assetIndex)>();
            var removedFileEntries = new List<(FileEntry fe, int index)>();

            foreach (var id in selection)
            {
                var item = _treeView.FindItemById(id);
                if (item is CueSheetAssetTreeItem assetItem && assetItem.Asset != null)
                {
                    var rootIdx = _definition.rootEntries.IndexOf(assetItem.Asset);
                    if (rootIdx >= 0)
                        removedAssets.Add((assetItem.Asset, rootIdx, -1, -1));
                    else
                        for (var fi = 0; fi < _definition.fileEntries.Count; fi++)
                        {
                            var ai = _definition.fileEntries[fi].assets.IndexOf(assetItem.Asset);
                            if (ai >= 0)
                            {
                                removedAssets.Add((assetItem.Asset, -1, fi, ai));
                                break;
                            }
                        }
                }
                else if (item is FileEntryTreeItem feItem)
                {
                    var idx = _definition.fileEntries.IndexOf(feItem.FileEntry);
                    if (idx >= 0)
                        removedFileEntries.Add((feItem.FileEntry, idx));
                }
            }

            _history.Register(
                "Remove Selected",
                () =>
                {
                    foreach (var (asset, rootIdx, feIdx, _) in removedAssets)
                        if (rootIdx >= 0)
                            _definition.rootEntries.Remove(asset);
                        else if (feIdx >= 0 && feIdx < _definition.fileEntries.Count)
                            _definition.fileEntries[feIdx].assets.Remove(asset);

                    foreach (var (fe, _) in removedFileEntries)
                        _definition.fileEntries.Remove(fe);

                    MarkDirtyAndSave();
                    _treeView?.Reload();
                    _selectedItem = null;
                    UpdateInspector();
                },
                () =>
                {
                    // Restore in reverse order
                    for (var i = removedFileEntries.Count - 1; i >= 0; i--)
                    {
                        var (fe, idx) = removedFileEntries[i];
                        if (idx <= _definition.fileEntries.Count)
                            _definition.fileEntries.Insert(idx, fe);
                        else
                            _definition.fileEntries.Add(fe);
                    }

                    for (var i = removedAssets.Count - 1; i >= 0; i--)
                    {
                        var (asset, rootIdx, feIdx, assetIdx) = removedAssets[i];
                        if (rootIdx >= 0)
                        {
                            if (rootIdx <= _definition.rootEntries.Count)
                                _definition.rootEntries.Insert(rootIdx, asset);
                            else
                                _definition.rootEntries.Add(asset);
                        }
                        else if (feIdx >= 0 && feIdx < _definition.fileEntries.Count)
                        {
                            if (assetIdx <= _definition.fileEntries[feIdx].assets.Count)
                                _definition.fileEntries[feIdx].assets.Insert(assetIdx, asset);
                            else
                                _definition.fileEntries[feIdx].assets.Add(asset);
                        }
                    }

                    MarkDirtyAndSave();
                    _treeView?.Reload();
                    _selectedItem = null;
                    UpdateInspector();
                });
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

        private void ApplyTooltips()
        {
            // Default Settings
            if (_defaultOutputPathField != null)
                _defaultOutputPathField.tooltip = L.Tr("cue_enum_definition.default_output_path");
            if (_defaultNamespaceField != null)
                _defaultNamespaceField.tooltip = L.Tr("cue_enum_definition.default_namespace");
            if (_defaultClassSuffixField != null)
                _defaultClassSuffixField.tooltip = L.Tr("cue_enum_definition.default_class_suffix");

            // FileEntry Inspector
            if (_fileNameField != null)
                _fileNameField.tooltip = L.Tr("cue_enum_definition.file_entry.file_name");
            if (_useDefaultOutputPathToggle != null)
                _useDefaultOutputPathToggle.tooltip = L.Tr("cue_enum_definition.file_entry.use_default_output_path");
            if (_outputPathField != null)
                _outputPathField.tooltip = L.Tr("cue_enum_definition.file_entry.output_path");
            if (_useDefaultNamespaceToggle != null)
                _useDefaultNamespaceToggle.tooltip = L.Tr("cue_enum_definition.file_entry.use_default_namespace");
            if (_namespaceField != null)
                _namespaceField.tooltip = L.Tr("cue_enum_definition.file_entry.namespace");
            if (_useDefaultClassSuffixToggle != null)
                _useDefaultClassSuffixToggle.tooltip = L.Tr("cue_enum_definition.file_entry.use_default_class_suffix");
            if (_classSuffixField != null)
                _classSuffixField.tooltip = L.Tr("cue_enum_definition.file_entry.class_suffix");
            if (_pathRuleField != null)
                _pathRuleField.tooltip = L.Tr("cue_enum_definition.file_entry.path_rule");

            // Asset Inspector
            if (_assetField != null)
                _assetField.tooltip = L.Tr("cue_enum_definition.asset.asset");
            if (_cueSheetNameField != null)
                _cueSheetNameField.tooltip = L.Tr("cue_enum_definition.asset.cue_sheet_name");
            if (_cueCountField != null)
                _cueCountField.tooltip = L.Tr("cue_enum_definition.asset.cue_count");

            // Footer
            if (_generateButton != null)
                _generateButton.tooltip = L.Tr("cue_enum_definition.generate");
        }

        private void HandleKeyDownEvent(KeyDownEvent e)
        {
            if (GetEventAction(e) && e.keyCode == UndoKey)
            {
                PerformUndo();
                e.StopPropagation();
            }

            if (GetEventAction(e) && e.keyCode == RedoKey)
            {
                PerformRedo();
                e.StopPropagation();
            }
        }

        private void PerformUndo()
        {
            _history.Undo();
        }

        private void PerformRedo()
        {
            _history.Redo();
        }

        private static bool GetEventAction(Event e)
        {
#if UNITY_EDITOR_WIN
            return e.control;
#else
            return e.command;
#endif
        }

        private static bool GetEventAction(IKeyboardEvent e)
        {
#if UNITY_EDITOR_WIN
            return e.ctrlKey;
#else
            return e.commandKey;
#endif
        }

        private void OnLanguageChanged()
        {
            ApplyTooltips();
        }

        private void OnProjectChanged()
        {
            _treeView?.Reload();
            UpdateInspector();
        }
    }
}
