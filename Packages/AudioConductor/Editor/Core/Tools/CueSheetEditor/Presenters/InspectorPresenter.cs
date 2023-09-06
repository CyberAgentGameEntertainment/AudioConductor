// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System;
using AudioConductor.Core.Tools.CueSheetEditor.Enums;
using AudioConductor.Editor.Core.Tools.CueSheetEditor.Models.Interfaces;
using AudioConductor.Editor.Core.Tools.CueSheetEditor.Views;
using UnityEngine.UIElements;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Presenters
{
    internal sealed class InspectorPresenter : IDisposable
    {
        private readonly InspectorView _view;

        private readonly CueTrackInspectorPresenter _cueTrackInspectorPresenter;
        private readonly CueInspectorPresenter _cueInspectorPresenter;
        private readonly TrackInspectorPresenter _trackInspectorPresenter;

        public InspectorPresenter(InspectorView view)
        {
            _view = view;

            _cueTrackInspectorPresenter = new CueTrackInspectorPresenter(_view.Q<CueTrackInspectorView>());
            _cueInspectorPresenter = new CueInspectorPresenter(_view.Q<CueInspectorView>());
            _trackInspectorPresenter = new TrackInspectorPresenter(_view.Q<TrackInspectorView>());
        }

        public void Dispose()
        {
            _trackInspectorPresenter.Dispose();
            _cueInspectorPresenter.Dispose();
            _view.Dispose();
        }

        public void Setup()
        {
            _view.Setup();
            _cueInspectorPresenter.Setup();
            _trackInspectorPresenter.Setup();
        }

        public void SetModel(IInspectorModel model)
        {
            switch (model.InspectorType)
            {
                case InspectorType.None:
                    CloseAllInspector();
                    break;
                case InspectorType.CueAndTrack:
                    OpenCueTrackInspector();
                    break;
                case InspectorType.Cue:
                    OpenCueInspector(model.CueInspectorModel);
                    break;
                case InspectorType.Track:
                    OpenTrackInspector(model.TrackInspectorModel);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(model.InspectorType), model.InspectorType, null);
            }
        }

        private void CloseAllInspector()
        {
            _cueTrackInspectorPresenter.Close();
            _cueInspectorPresenter.Close();
            _trackInspectorPresenter.Close();
        }

        private void OpenCueTrackInspector()
        {
            _cueInspectorPresenter.Close();
            _trackInspectorPresenter.Close();

            _cueTrackInspectorPresenter.Open();
        }

        private void OpenCueInspector(ICueInspectorModel model)
        {
            _cueTrackInspectorPresenter.Close();
            _trackInspectorPresenter.Close();

            _cueInspectorPresenter.Open(model);
        }

        private void OpenTrackInspector(ITrackInspectorModel model)
        {
            _cueTrackInspectorPresenter.Close();
            _cueInspectorPresenter.Close();

            _trackInspectorPresenter.Open(model);
        }
    }
}
