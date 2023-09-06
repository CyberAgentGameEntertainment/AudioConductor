// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System;
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

        [SerializeField]
        private CueSheetAssetEditorWindowModel _target;

        private readonly CompositeDisposable _disposable = new();

        private CueSheetEditorPresenter _cueSheetEditorPresenter;

        private void OnDisable() => Cleanup();

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
            _target.Setup();

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
        }

        private void OnFocus()
        {
            CategoryListRepository.instance.Update();
            ColorDefineListRepository.instance.Update();
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
