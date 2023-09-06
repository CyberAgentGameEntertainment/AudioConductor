// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using AudioConductor.Editor.Core.Models;
using AudioConductor.Editor.Core.Tools.Shared;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace AudioConductor.Editor.Core.CustomEditors
{
    [CustomEditor(typeof(AudioConductorEditorSettings))]
    internal sealed class AudioConductorEditorSettingsEditor : UnityEditor.Editor
    {
        private AudioConductorEditorSettings _settings;

        private void OnEnable()
        {
            _settings = (AudioConductorEditorSettings)target;
        }

        public override VisualElement CreateInspectorGUI()
        {
            var treeAsset = AssetLoader.LoadUxml("AudioConductorEditorSettings");
            var container = treeAsset.CloneTree();

            var colorDefineListView = container.Q<ListView>();
            colorDefineListView.bindingPath = nameof(AudioConductorEditorSettings.colorDefineList);
            colorDefineListView.makeItem = () => new ColorDefineView();
            colorDefineListView.itemsAdded += indices =>
            {
                var list = _settings.colorDefineList;
                foreach (var index in indices)
                    // Use default values instead of trailing copy.
                    list[index] = new ColorDefine();
            };

            var sizeField = colorDefineListView.Q<TextField>("unity-list-view__size-field");
            sizeField.SetVisible(false);

            container.schedule
                     .Execute(() => AssetDatabase.SaveAssetIfDirty(target))
                     .Every(1000);

            return container;
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
