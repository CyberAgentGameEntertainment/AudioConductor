// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System;
using System.IO;
using AudioConductor.Editor.Core.Tools.CueSheetEditor.Models.Interfaces;
using AudioConductor.Editor.Core.Tools.CueSheetEditor.Views;
using AudioConductor.Editor.Foundation.TinyRx;
using UnityEditor;
using UnityEngine;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Presenters
{
    internal sealed class OtherOperationPanePresenter : IDisposable
    {
        private readonly IOtherOperationPaneModel _model;

        private readonly OtherOperationPaneView _view;
        private readonly CompositeDisposable _viewEventDisposable = new();

        public OtherOperationPanePresenter(IOtherOperationPaneModel model, OtherOperationPaneView view)
        {
            _model = model;
            _view = view;
        }

        public void Dispose()
        {
            CleanupViewEventHandlers();
            _view.Dispose();
        }

        public void Setup()
        {
            _view.Setup();
            SetupViewEventHandlers();
        }

        private void SetupViewEventHandlers()
        {
            _view.ExportClickedAsObservable
                 .Subscribe(_ => Export())
                 .DisposeWith(_viewEventDisposable);

            _view.ImportClickedAsObservable
                 .Subscribe(_ => Import())
                 .DisposeWith(_viewEventDisposable);
        }

        private void CleanupViewEventHandlers()
        {
            _viewEventDisposable.Clear();
        }

        public void Open()
        {
            _view.Open();
        }

        public void Close()
        {
            _view.Close();
        }

        private void Export()
        {
            var csvText = _model.CreateCsvText();
            if (string.IsNullOrEmpty(csvText))
                return;

            var defaultName = _model.CueSheetName;
            var path = EditorUtility.SaveFilePanel("Export CueSheet", "", defaultName, "csv");
            if (string.IsNullOrEmpty(path))
                return;

            try
            {
                File.WriteAllText(path, csvText);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return;
            }

            EditorUtility.DisplayDialog("CueSheet export success.", path, "OK");
        }

        private void Import()
        {
            var path = EditorUtility.OpenFilePanel("Import CueSheet", "", "csv");
            if (string.IsNullOrEmpty(path))
                return;

            string[] lines;
            try
            {
                lines = File.ReadAllLines(path);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return;
            }

            if (_model.ImportCsv(lines))
                EditorUtility.DisplayDialog("CueSheet import success.", path, "OK");
        }
    }
}
