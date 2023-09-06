// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System;
using AudioConductor.Editor.Core.Tools.CueSheetEditor.Views;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Presenters
{
    internal sealed class CueTrackInspectorPresenter : IDisposable
    {
        private readonly CueTrackInspectorView _view;

        public CueTrackInspectorPresenter(CueTrackInspectorView view)
        {
            _view = view;
        }

        public void Dispose()
        {
            _view.Dispose();
        }

        internal void Open()
        {
            _view.Open();
        }

        internal void Close()
        {
            _view.Close();
        }
    }
}
