// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using System.Collections.Generic;
using AudioConductor.Editor.Core.Models;
using AudioConductor.Editor.Core.Tools.Shared;
using AudioConductor.Editor.Localization;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace AudioConductor.Editor.Core.CustomEditors
{
    [CustomEditor(typeof(AudioConductorEditorSettings))]
    internal sealed class AudioConductorEditorSettingsEditor : UnityEditor.Editor
    {
        private TextField? _defaultCodeGenClassSuffixField;
        private TextField? _defaultCodeGenNamespaceField;
        private TextField? _defaultCodeGenOutputPathField;
        private EnumField? _languageField;
        private AudioConductorEditorSettings _settings = null!;

        private void OnEnable()
        {
            _settings = (AudioConductorEditorSettings)target;
            Localization.Localization.LanguageChanged += OnLanguageChanged;
        }

        private void OnDisable()
        {
            Localization.Localization.LanguageChanged -= OnLanguageChanged;
        }

        public override VisualElement CreateInspectorGUI()
        {
            var treeAsset = AssetLoader.LoadUxml("AudioConductorEditorSettings");
            var container = treeAsset.CloneTree();

            _languageField = container.Q<EnumField>("Language");
            _languageField.Init(Localization.Localization.Language);
            _languageField.RegisterValueChangedCallback(OnLanguageFieldChanged);

            _defaultCodeGenOutputPathField = container.Q<TextField>("DefaultCodeGenOutputPath");
            _defaultCodeGenOutputPathField.bindingPath = nameof(AudioConductorEditorSettings.defaultCodeGenOutputPath);

            _defaultCodeGenNamespaceField = container.Q<TextField>("DefaultCodeGenNamespace");
            _defaultCodeGenNamespaceField.bindingPath = nameof(AudioConductorEditorSettings.defaultCodeGenNamespace);

            _defaultCodeGenClassSuffixField = container.Q<TextField>("DefaultCodeGenClassSuffix");
            _defaultCodeGenClassSuffixField.bindingPath =
                nameof(AudioConductorEditorSettings.defaultCodeGenClassSuffix);
            ApplyTooltips();

            var colorDefineListView = container.Q<ListView>();
            colorDefineListView.bindingPath = nameof(AudioConductorEditorSettings.colorDefineList);
            colorDefineListView.makeItem = () => new ColorDefineView();
            colorDefineListView.itemsAdded += OnColorDefineListItemsAdded;

            var sizeField = colorDefineListView.Q<TextField>("unity-list-view__size-field");
            sizeField.SetVisible(false);

            container.schedule
                .Execute(() => AssetDatabase.SaveAssetIfDirty(target))
                .Every(1000);

            return container;
        }

        private void ApplyTooltips()
        {
            if (_defaultCodeGenOutputPathField != null)
                _defaultCodeGenOutputPathField.tooltip =
                    Localization.Localization.Tr("editor_settings.codegen_default_output_path");
            if (_defaultCodeGenNamespaceField != null)
                _defaultCodeGenNamespaceField.tooltip =
                    Localization.Localization.Tr("editor_settings.codegen_default_namespace");
            if (_defaultCodeGenClassSuffixField != null)
                _defaultCodeGenClassSuffixField.tooltip =
                    Localization.Localization.Tr("editor_settings.codegen_default_class_suffix");
        }

        private void OnLanguageFieldChanged(ChangeEvent<Enum> evt)
        {
            Localization.Localization.Language = (EditorLanguage)evt.newValue;
        }

        private void OnLanguageChanged()
        {
            if (_languageField == null)
                return;
            _languageField.SetValueWithoutNotify(Localization.Localization.Language);
            ApplyTooltips();
        }

        private void OnColorDefineListItemsAdded(IEnumerable<int> indices)
        {
            var list = _settings.colorDefineList;
            foreach (var index in indices)
                // Use default values instead of trailing copy.
                list[index] = new ColorDefine();
        }

        private class ColorDefineView : BindableElement
        {
            public ColorDefineView()
            {
                var element = AssetLoader.LoadUxml("ColorDefine");
                element.CloneTree(this);

                var idField = this.Q<TextField>("Id");
                idField.bindingPath = nameof(ColorDefine.id);
                idField.SetEnabled(false);
                idField.SetDisplay(false); // for developer

                var nameField = this.Q<TextField>("Name");
                nameField.bindingPath = nameof(ColorDefine.name);

                var colorField = this.Q<ColorField>();
                colorField.bindingPath = nameof(ColorDefine.color);
                colorField.showAlpha = false;
            }
        }
    }
}
