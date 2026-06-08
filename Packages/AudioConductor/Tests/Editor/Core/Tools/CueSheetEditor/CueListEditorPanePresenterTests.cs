// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using System.Collections.Generic;
using AudioConductor.Editor.Core.Tools.CueSheetEditor.Models.Interfaces;
using AudioConductor.Editor.Core.Tools.CueSheetEditor.Presenters;
using AudioConductor.Editor.Core.Tools.CueSheetEditor.Views;
using AudioConductor.Editor.Foundation.TinyRx;
using AudioConductor.Editor.Foundation.TinyRx.ObservableProperty;
using NUnit.Framework;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Tests
{
    internal sealed class CueListEditorPanePresenterTests
    {
        [Test]
        public void FocusCue_WhenItemFound_ClearsSearchAndFocusesItem()
        {
            var model = new FakeModel(5, "cue-abc");
            var view = new FakeView();
            var cueListPresenter = new FakeCueListPresenter();
            using var presenter = new CueListEditorPanePresenter(model, view, cueListPresenter);
            presenter.Setup();

            presenter.FocusCue("cue-abc");

            Assert.That(view.ClearSearchCalled, Is.True);
            Assert.That(cueListPresenter.OnSearchFieldChangedArgs, Contains.Item(string.Empty));
            Assert.That(cueListPresenter.FocusItemByIdArgs, Is.EqualTo(new[] { 5 }));
        }

        [Test]
        public void FocusCue_WhenItemNotFound_DoesNotClearSearchOrFocusItem()
        {
            var model = new FakeModel(-1, "cue-abc");
            var view = new FakeView();
            var cueListPresenter = new FakeCueListPresenter();
            using var presenter = new CueListEditorPanePresenter(model, view, cueListPresenter);
            presenter.Setup();

            presenter.FocusCue("unknown-id");

            Assert.That(view.ClearSearchCalled, Is.False);
            Assert.That(cueListPresenter.FocusItemByIdArgs, Is.Empty);
        }

        [Test]
        public void FocusCue_WhenSearchIsActive_StillFocusesCorrectItem()
        {
            var model = new FakeModel(42, "cue-xyz");
            var view = new FakeView();
            view.SimulateSearchActive("active-filter");
            var cueListPresenter = new FakeCueListPresenter();
            using var presenter = new CueListEditorPanePresenter(model, view, cueListPresenter);
            presenter.Setup();

            presenter.FocusCue("cue-xyz");

            Assert.That(view.ClearSearchCalled, Is.True);
            Assert.That(cueListPresenter.FocusItemByIdArgs, Is.EqualTo(new[] { 42 }));
        }

        private sealed class FakeModel : ICueListEditorPaneModel
        {
            private readonly string _cueEditorId;
            private readonly int _itemId;

            internal FakeModel(int itemId, string cueEditorId)
            {
                _itemId = itemId;
                _cueEditorId = cueEditorId;
                ObservableInspectorUnCollapsed = new ObservableProperty<bool>(false);
            }

            public ICueListModel CueListModel => null!;
            public IObservableProperty<bool> ObservableInspectorUnCollapsed { get; }
            public IReadOnlyCollection<int> VisibleColumns => Array.Empty<int>();
            public string SearchString => string.Empty;

            public int FindItemIdByCueEditorId(string cueEditorId)
            {
                return cueEditorId == _cueEditorId ? _itemId : -1;
            }
        }

        private sealed class FakeView : ICueListEditorPaneView
        {
            private readonly Subject<bool> _inspectorToggleChanged = new();
            private readonly Subject<bool> _memoToggleChanged = new();
            private readonly Subject<bool> _playInfoToggleChanged = new();
            private readonly Subject<string> _searchFieldChanged = new();
            private readonly Subject<bool> _throttleToggleChanged = new();
            private readonly Subject<bool> _volumeToggleChanged = new();

            internal bool ClearSearchCalled { get; private set; }

            public IObservable<bool> InspectorToggleChangedAsObservable => _inspectorToggleChanged;
            public IObservable<bool> VolumeToggleChangedAsObservable => _volumeToggleChanged;
            public IObservable<bool> PlayInfoToggleChangedAsObservable => _playInfoToggleChanged;
            public IObservable<bool> ThrottleToggleChangedAsObservable => _throttleToggleChanged;
            public IObservable<bool> MemoToggleChangedAsObservable => _memoToggleChanged;
            public IObservable<string> SearchFieldChangedAsObservable => _searchFieldChanged;

            public void Dispose()
            {
            }

            public void Setup()
            {
            }

            public void Open()
            {
            }

            public void Close()
            {
            }

            public void SetButtonState(IReadOnlyCollection<int> visibleColumns)
            {
            }

            public void SetSearchString(string searchString)
            {
            }

            public void SetInspector(bool unCollapsed)
            {
            }

            public void ClearSearch()
            {
                ClearSearchCalled = true;
                _searchFieldChanged.OnNext(string.Empty);
            }

            internal void SimulateSearchActive(string searchText)
            {
                _searchFieldChanged.OnNext(searchText);
            }
        }

        private sealed class FakeCueListPresenter : ICueListPresenter
        {
            internal List<int> FocusItemByIdArgs { get; } = new();
            internal List<string> OnSearchFieldChangedArgs { get; } = new();

            public event Action<IInspectorModel> OnSelectionItemChanged
            {
                add { }
                remove { }
            }

            public void Dispose()
            {
            }

            public void Setup()
            {
            }

            public void FocusItemById(int itemId)
            {
                FocusItemByIdArgs.Add(itemId);
            }

            public void OnVolumeToggleChanged(bool active)
            {
            }

            public void OnPlayInfoToggleChanged(bool active)
            {
            }

            public void OnThrottleToggleChanged(bool active)
            {
            }

            public void OnMemoToggleChanged(bool active)
            {
            }

            public void OnSearchFieldChanged(string text)
            {
                OnSearchFieldChangedArgs.Add(text);
            }
        }
    }
}
