// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.Collections.Generic;
using System.Linq;
using AudioConductor.Core.Models;
using AudioConductor.Editor.Core.Tools.Shared;
using AudioConductor.Editor.Localization;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AudioConductor.Editor.Core.CustomEditors
{
    [CustomEditor(typeof(AudioConductorSettings))]
    internal sealed class AudioConductorSettingsEditor : UnityEditor.Editor
    {
        private HashSet<int> _existIds = null!;
        private IntegerField? _managedPoolCapacityField;
        private FloatField? _masterVolumeField;
        private IntegerField? _oneShotPoolCapacityField;
        private AudioConductorSettings _settings = null!;
        private IntegerField? _throttleLimitField;

        private ThrottleTypeField? _throttleTypeField;

        private void OnEnable()
        {
            _settings = (AudioConductorSettings)target;
            _existIds = new HashSet<int>(_settings.categoryList.Select(category => category.id));
            Localization.Localization.LanguageChanged += OnLanguageChanged;
        }

        private void OnDisable()
        {
            Localization.Localization.LanguageChanged -= OnLanguageChanged;
        }

        public override VisualElement CreateInspectorGUI()
        {
            var treeAsset = AssetLoader.LoadUxml("AudioConductorSettings");
            var container = treeAsset.CloneTree();

            _throttleTypeField = container.Q<ThrottleTypeField>();
            _throttleTypeField.bindingPath = nameof(AudioConductorSettings.throttleType);

            _throttleLimitField = container.Q<IntegerField>("ThrottleLimit");
            _throttleLimitField.bindingPath = nameof(AudioConductorSettings.throttleLimit);

            _masterVolumeField = container.Q<FloatField>("MasterVolume");
            _masterVolumeField.bindingPath = nameof(AudioConductorSettings.masterVolume);

            _managedPoolCapacityField = container.Q<IntegerField>("ManagedPoolCapacity");
            _managedPoolCapacityField.bindingPath = nameof(AudioConductorSettings.managedPoolCapacity);

            _oneShotPoolCapacityField = container.Q<IntegerField>("OneShotPoolCapacity");
            _oneShotPoolCapacityField.bindingPath = nameof(AudioConductorSettings.oneShotPoolCapacity);

            ApplyTooltips();

            var categoryListView = container.Q<ListView>();
            categoryListView.bindingPath = nameof(AudioConductorSettings.categoryList);
            categoryListView.makeItem = () => new CategoryView();
            categoryListView.itemsAdded += OnCategoryListItemsAdded;
            categoryListView.itemsRemoved += OnCategoryListItemsRemoved;

            var sizeField = categoryListView.Q<TextField>("unity-list-view__size-field");
            sizeField.SetVisible(false);

            container.schedule
                .Execute(() => AssetDatabase.SaveAssetIfDirty(target))
                .Every(1000);

            return container;
        }

        private void ApplyTooltips()
        {
            if (_throttleTypeField != null)
                _throttleTypeField.tooltip = Localization.Localization.Tr("settings.throttle_type");
            if (_throttleLimitField != null)
                _throttleLimitField.tooltip = Localization.Localization.Tr("settings.throttle_limit");
            if (_masterVolumeField != null)
                _masterVolumeField.tooltip = Localization.Localization.Tr("settings.master_volume");
            if (_managedPoolCapacityField != null)
                _managedPoolCapacityField.tooltip = Localization.Localization.Tr("settings.managed_pool_size");
            if (_oneShotPoolCapacityField != null)
                _oneShotPoolCapacityField.tooltip = Localization.Localization.Tr("settings.oneshot_pool_size");
        }

        private void OnLanguageChanged()
        {
            ApplyTooltips();
        }

        private void OnCategoryListItemsAdded(IEnumerable<int> indices)
        {
            var list = _settings.categoryList;
            foreach (var index in indices)
            {
                var id = CreateUniqueId();
                // Use default values instead of trailing copy.
                list[index] = new Category
                {
                    id = id
                };
                _existIds.Add(id);
            }
        }

        private void OnCategoryListItemsRemoved(IEnumerable<int> indices)
        {
            _existIds.Clear();
            var indicesSet = new HashSet<int>(indices);
            // It may be called before changes to the SerializedProperty are applied.
            var list = _settings.categoryList;
            for (var i = 0; i < list.Count; i++)
            {
                if (indicesSet.Contains(i))
                    continue;
                _existIds.Add(list[i].id);
            }
        }

        private int CreateUniqueId()
        {
            int id;
            do
            {
                id = Random.Range(0, int.MaxValue);
            } while (_existIds.Contains(id));

            return id;
        }

        private class CategoryView : BindableElement
        {
            public CategoryView()
            {
                var element = AssetLoader.LoadUxml("Category");
                element.CloneTree(this);

                var idField = this.Q<IntegerField>("Id");
                idField.bindingPath = nameof(Category.id);
                idField.SetEnabled(false);
                idField.SetDisplay(false); // for developer

                var nameField = this.Q<TextField>("Name");
                nameField.bindingPath = nameof(Category.name);

                var throttleTypeField = this.Q<ThrottleTypeField>();
                throttleTypeField.bindingPath = nameof(Category.throttleType);

                var throttleLimitField = this.Q<IntegerField>("ThrottleLimit");
                throttleLimitField.bindingPath = nameof(Category.throttleLimit);

                var audioMixerGroupField = this.Q<ObjectField>();
                audioMixerGroupField.bindingPath = nameof(Category.audioMixerGroup);
            }
        }
    }
}
