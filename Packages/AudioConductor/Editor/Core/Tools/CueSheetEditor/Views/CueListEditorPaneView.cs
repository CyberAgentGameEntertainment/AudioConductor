// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System;
using System.Collections.Generic;
using AudioConductor.Editor.Core.Tools.Shared;
using AudioConductor.Editor.Foundation.TinyRx;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using TwoPaneSplitView = AudioConductor.Editor.Core.Tools.Shared.TwoPaneSplitView;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Views
{
    internal sealed class CueListEditorPaneView : VisualElement, IDisposable
    {
        private readonly ToolbarToggle _inspectorToggle;
        private readonly ToolbarToggle _volumeToggle;
        private readonly ToolbarToggle _playInfoToggle;
        private readonly ToolbarToggle _throttleToggle;
        private readonly ToolbarToggle _memoToggle;
        private readonly ToolbarSearchField _searchField;
        private readonly TwoPaneSplitView _twoPaneSplitView;

        private readonly Subject<bool> _inspectorToggleChangedSubject = new();
        private readonly Subject<bool> _volumeToggleChangedSubject = new();
        private readonly Subject<bool> _playInfoToggleChangedSubject = new();
        private readonly Subject<bool> _throttleInfoToggleChangedSubject = new();
        private readonly Subject<bool> _memoToggleChangedSubject = new();
        private readonly Subject<string> _searchFieldChangedSubject = new();

        public CueListEditorPaneView()
        {
            var tree = AssetLoader.LoadUxml("CueListEditorPane");
            tree.CloneTree(this);

            _twoPaneSplitView = this.Q<TwoPaneSplitView>();
            _twoPaneSplitView.fixedPaneInitialDimension += 1; // force Init call
            _inspectorToggle = this.Q<ToolbarToggle>("Inspector");
            _volumeToggle = this.Q<ToolbarToggle>("Volume");
            _playInfoToggle = this.Q<ToolbarToggle>("PlayInfo");
            _throttleToggle = this.Q<ToolbarToggle>("Throttle");
            _memoToggle = this.Q<ToolbarToggle>("Memo");
            _searchField = this.Q<ToolbarSearchField>();
        }

        internal IObservable<bool> InspectorToggleChangedAsObservable => _inspectorToggleChangedSubject;
        internal IObservable<bool> VolumeToggleChangedAsObservable => _volumeToggleChangedSubject;
        internal IObservable<bool> PlayInfoToggleChangedAsObservable => _playInfoToggleChangedSubject;
        internal IObservable<bool> ThrottleToggleChangedAsObservable => _throttleInfoToggleChangedSubject;
        internal IObservable<bool> MemoToggleChangedAsObservable => _memoToggleChangedSubject;
        internal IObservable<string> SearchFieldChangedAsObservable => _searchFieldChangedSubject;

        public void Dispose()
        {
            CleanupEventHandlers();
        }

        internal void Setup()
        {
            SetupEventHandlers();
        }

        internal void SetButtonState(IReadOnlyCollection<int> visibleColumns)
        {
            SetToggleValue(_volumeToggle, CueListTreeView.VolumeColumnGroup);
            SetToggleValue(_playInfoToggle, CueListTreeView.PlayInfoColumnGroup);
            SetToggleValue(_throttleToggle, CueListTreeView.ThrottleColumnGroup);
            SetToggleValue(_memoToggle, CueListTreeView.MemoColumnGroup);

            #region LocalMethods

            void SetToggleValue(ToolbarToggle toggle, IReadOnlyCollection<int> columns)
            {
                var stateSet = new HashSet<int>(visibleColumns);
                var columnsSet = new HashSet<int>(columns);

                stateSet.IntersectWith(columnsSet);
                toggle.SetValueWithoutNotify(stateSet.Count == columns.Count);
            }

            #endregion
        }

        internal void SetSearchString(string searchString)
        {
            _searchField.SetValueWithoutNotify(searchString);
        }

        internal void Open()
        {
            this.SetDisplay(true);
        }

        internal void Close()
        {
            this.SetDisplay(false);
        }

        private void SetupEventHandlers()
        {
            _inspectorToggle.RegisterValueChangedCallback(OnInspectorToggle);
            _volumeToggle.RegisterValueChangedCallback(OnVolumeToggleChanged);
            _playInfoToggle.RegisterValueChangedCallback(OnPlayInfoToggleChanged);
            _throttleToggle.RegisterValueChangedCallback(OnThrottleToggleChanged);
            _memoToggle.RegisterValueChangedCallback(OnMemoToggleChanged);
            _searchField.RegisterValueChangedCallback(OnSearchFieldChanged);
        }

        private void CleanupEventHandlers()
        {
            _searchField.UnregisterValueChangedCallback(OnSearchFieldChanged);
            _memoToggle.UnregisterValueChangedCallback(OnMemoToggleChanged);
            _throttleToggle.UnregisterValueChangedCallback(OnThrottleToggleChanged);
            _playInfoToggle.UnregisterValueChangedCallback(OnPlayInfoToggleChanged);
            _volumeToggle.UnregisterValueChangedCallback(OnVolumeToggleChanged);
            _inspectorToggle.UnregisterValueChangedCallback(OnInspectorToggle);
        }

        internal void SetInspector(bool unCollapsed)
        {
            _inspectorToggle.SetValueWithoutNotify(unCollapsed);

            if (unCollapsed)
                _twoPaneSplitView.UnCollapse();
            else
                _twoPaneSplitView.CollapseChild(1);
        }

        #region Methods - EventHandlers

        private void OnInspectorToggle(ChangeEvent<bool> evt)
            => _inspectorToggleChangedSubject.OnNext(evt.newValue);

        private void OnVolumeToggleChanged(ChangeEvent<bool> evt)
            => _volumeToggleChangedSubject.OnNext(evt.newValue);

        private void OnPlayInfoToggleChanged(ChangeEvent<bool> evt)
            => _playInfoToggleChangedSubject.OnNext(evt.newValue);

        private void OnThrottleToggleChanged(ChangeEvent<bool> evt)
            => _throttleInfoToggleChangedSubject.OnNext(evt.newValue);

        private void OnMemoToggleChanged(ChangeEvent<bool> evt)
            => _memoToggleChangedSubject.OnNext(evt.newValue);

        private void OnSearchFieldChanged(ChangeEvent<string> evt)
            => _searchFieldChangedSubject.OnNext(evt.newValue);

        #endregion

        #region Uxml

        public new class UxmlFactory : UxmlFactory<CueListEditorPaneView, UxmlTraits>
        {
            public override string uxmlNamespace => "Unity.UI.Builder";
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
        }

        #endregion
    }
}
