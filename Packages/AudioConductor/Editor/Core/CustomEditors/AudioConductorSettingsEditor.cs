// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using AudioConductor.Editor.Core.Tools.Shared;
using AudioConductor.Runtime.Core.Models;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AudioConductor.Editor.Core.CustomEditors
{
    [CustomEditor(typeof(AudioConductorSettings))]
    internal sealed class AudioConductorSettingsEditor : UnityEditor.Editor
    {
        private HashSet<int> _existIds;
        private AudioConductorSettings _settings;

        private void OnEnable()
        {
            _settings = (AudioConductorSettings)target;
            _existIds = new HashSet<int>(_settings.categoryList.Select(category => category.id));
        }

        public override VisualElement CreateInspectorGUI()
        {
            var treeAsset = AssetLoader.LoadUxml("AudioConductorSettings");
            var container = treeAsset.CloneTree();

            var throttleTypeField = container.Q<ThrottleTypeField>();
            throttleTypeField.bindingPath = nameof(AudioConductorSettings.throttleType);

            var throttleLimitField = container.Q<IntegerField>("ThrottleLimit");
            throttleLimitField.bindingPath = nameof(AudioConductorSettings.throttleLimit);

            var categoryListView = container.Q<ListView>();
            categoryListView.bindingPath = nameof(AudioConductorSettings.categoryList);
            categoryListView.makeItem = () => new CategoryView();
            categoryListView.itemsAdded += indices =>
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
            };
            categoryListView.itemsRemoved += indices =>
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
            };

            var sizeField = categoryListView.Q<TextField>("unity-list-view__size-field");
            sizeField.SetVisible(false);

            container.schedule
                     .Execute(() => AssetDatabase.SaveAssetIfDirty(target))
                     .Every(1000);

            return container;
        }

        private int CreateUniqueId()
        {
            int id;
            do
                id = Random.Range(0, int.MaxValue);
            while (_existIds.Contains(id));

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
