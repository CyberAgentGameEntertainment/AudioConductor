// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using System.Collections.Generic;
using AudioConductor.Core.Models;
using AudioConductor.Editor.Core.Tools.CueSheetEditor;
using AudioConductor.Editor.Core.Tools.Shared;
using AudioConductor.Editor.Core.Tools.Validation.Models;
using AudioConductor.Editor.Core.Tools.Validation.Presenters;
using AudioConductor.Editor.Core.Tools.Validation.Views;
using AudioConductor.Editor.Foundation.TinyRx;
using UnityEditor;
using UnityEngine;

namespace AudioConductor.Editor.Core.Tools.Validation
{
    internal sealed class CueSheetValidationWindow : EditorWindow
    {
        private readonly CompositeDisposable _disposable = new();
        private IEnumerable<CueSheetAsset> _assets = Array.Empty<CueSheetAsset>();
        private CueSheetValidationWindowModel? _model;
        private CueSheetValidationPresenter? _presenter;
        private ValidationScope _scope = ValidationScope.None;

        private void OnDisable()
        {
            Teardown();
        }

        private void CreateGUI()
        {
            Rebuild();
        }

        private void Rebuild()
        {
            Teardown();
            rootVisualElement.Clear();

            titleContent = new GUIContent("CueSheet Validation");
            minSize = new Vector2(400, 300);

            var model = new CueSheetValidationWindowModel(
                _assets,
                _scope,
                AudioConductorSettingsRepository.instance,
                new EditorPrefsValidationSettingsPreferences(),
                new CueSheetValidator());
            _model = model;

            var view = new CueSheetValidationView(rootVisualElement);
            _presenter = new CueSheetValidationPresenter(model, view);
            _presenter.Setup();

            model.RowSelected
                .Subscribe(row =>
                {
                    if (row.Asset == null)
                        return;

                    if (row.Issue is null)
                        CueSheetAssetEditorWindow.Open(row.Asset);
                    else
                        CueSheetAssetEditorWindow.OpenWithFocus(row.Asset, row.Issue.CueEditorId);
                })
                .DisposeWith(_disposable);
        }

        private void Teardown()
        {
            _disposable.Clear();
            _presenter?.Dispose();
            _model?.Dispose();
            _presenter = null;
            _model = null;
        }

        internal static void Open(IEnumerable<CueSheetAsset> assets, ValidationScope scope)
        {
            var window = GetWindow<CueSheetValidationWindow>();
            window._assets = assets;
            window._scope = scope;
            window.Rebuild();
            window.Show();
        }
    }
}
