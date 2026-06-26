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
    internal sealed class CueSheetEditorPresenterTests
    {
        [Test]
        public void Setup_InitialPane_SelectsTabAndOpensMatchingPane()
        {
            var model = new FakeModel(CueSheetEditorPresenter.Pane.CueList);
            var view = new FakeView();
            var parameter = new FakeCueSheetEditorPanePresenter();
            var cueList = new FakeCueListEditorPanePresenter();
            var other = new FakeCueSheetEditorPanePresenter();

            using var presenter = new CueSheetEditorPresenter(model, view, parameter, cueList, other);
            presenter.Setup();

            Assert.That(view.SetupCount, Is.EqualTo(1));
            Assert.That(view.SelectedTabs, Is.EqualTo(new[] { (int)CueSheetEditorPresenter.Pane.CueList }));
            Assert.That(cueList.OpenCount, Is.EqualTo(1));
            Assert.That(parameter.CloseCount, Is.EqualTo(1));
            Assert.That(other.CloseCount, Is.EqualTo(1));
            Assert.That(parameter.SetupCount, Is.EqualTo(1));
            Assert.That(cueList.SetupCount, Is.EqualTo(1));
            Assert.That(other.SetupCount, Is.EqualTo(1));
        }

        [Test]
        public void ObservablePaneState_WhenChanged_SelectsTabAndSwitchesPane()
        {
            var model = new FakeModel(CueSheetEditorPresenter.Pane.CueList);
            var view = new FakeView();
            var parameter = new FakeCueSheetEditorPanePresenter();
            var cueList = new FakeCueListEditorPanePresenter();
            var other = new FakeCueSheetEditorPanePresenter();

            using var presenter = new CueSheetEditorPresenter(model, view, parameter, cueList, other);
            presenter.Setup();

            model.ObservablePaneState.Value = CueSheetEditorPresenter.Pane.OtherOperation;

            Assert.That(view.SelectedTabs[^1], Is.EqualTo((int)CueSheetEditorPresenter.Pane.OtherOperation));
            Assert.That(parameter.CloseCount, Is.EqualTo(2));
            Assert.That(cueList.CloseCount, Is.EqualTo(1));
            Assert.That(other.OpenCount, Is.EqualTo(1));
        }

        [Test]
        public void ObservablePaneState_WhenChangedToCueSheetParameter_OpensParameterPaneAndClosesOthers()
        {
            var model = new FakeModel(CueSheetEditorPresenter.Pane.CueList);
            var view = new FakeView();
            var parameter = new FakeCueSheetEditorPanePresenter();
            var cueList = new FakeCueListEditorPanePresenter();
            var other = new FakeCueSheetEditorPanePresenter();

            using var presenter = new CueSheetEditorPresenter(model, view, parameter, cueList, other);
            presenter.Setup();

            model.ObservablePaneState.Value = CueSheetEditorPresenter.Pane.CueSheetParameter;

            Assert.That(view.SelectedTabs[^1], Is.EqualTo((int)CueSheetEditorPresenter.Pane.CueSheetParameter));
            Assert.That(parameter.OpenCount, Is.EqualTo(1));
            Assert.That(cueList.CloseCount, Is.EqualTo(1));
            Assert.That(other.CloseCount, Is.EqualTo(2));
        }

        [Test]
        public void TabSelectedByView_UpdatesObservablePaneState()
        {
            var model = new FakeModel(CueSheetEditorPresenter.Pane.CueList);
            var view = new FakeView();

            using var presenter = new CueSheetEditorPresenter(
                model,
                view,
                new FakeCueSheetEditorPanePresenter(),
                new FakeCueListEditorPanePresenter(),
                new FakeCueSheetEditorPanePresenter());
            presenter.Setup();

            view.EmitTabSelected((int)CueSheetEditorPresenter.Pane.CueSheetParameter);

            Assert.That(model.ObservablePaneState.Value, Is.EqualTo(CueSheetEditorPresenter.Pane.CueSheetParameter));
        }

        [Test]
        public void FocusCue_SetsPaneStateToCueList()
        {
            var model = new FakeModel(CueSheetEditorPresenter.Pane.CueSheetParameter);
            var view = new FakeView();
            var cueList = new FakeCueListEditorPanePresenter();

            using var presenter = new CueSheetEditorPresenter(model, view, new FakeCueSheetEditorPanePresenter(),
                cueList,
                new FakeCueSheetEditorPanePresenter());
            presenter.Setup();

            presenter.FocusCue("test-id");

            Assert.That(model.ObservablePaneState.Value, Is.EqualTo(CueSheetEditorPresenter.Pane.CueList));
        }

        [Test]
        public void FocusCue_DelegatesToCueListEditorPanePresenter()
        {
            var model = new FakeModel(CueSheetEditorPresenter.Pane.CueList);
            var view = new FakeView();
            var cueList = new FakeCueListEditorPanePresenter();

            using var presenter = new CueSheetEditorPresenter(model, view, new FakeCueSheetEditorPanePresenter(),
                cueList,
                new FakeCueSheetEditorPanePresenter());
            presenter.Setup();

            presenter.FocusCue("test-id");

            Assert.That(cueList.FocusCueArgs, Is.EqualTo(new[] { "test-id" }));
        }

        [Test]
        public void Dispose_UnsubscribesViewEvents()
        {
            var model = new FakeModel(CueSheetEditorPresenter.Pane.CueList);
            var view = new FakeView();
            var presenter = new CueSheetEditorPresenter(
                model,
                view,
                new FakeCueSheetEditorPanePresenter(),
                new FakeCueListEditorPanePresenter(),
                new FakeCueSheetEditorPanePresenter());
            presenter.Setup();
            presenter.Dispose();

            view.EmitTabSelected((int)CueSheetEditorPresenter.Pane.OtherOperation);

            Assert.That(model.ObservablePaneState.Value, Is.EqualTo(CueSheetEditorPresenter.Pane.CueList));
            Assert.That(view.DisposeCount, Is.EqualTo(1));
        }

        [Test]
        public void Dispose_DisposesAllPanePresenters()
        {
            var model = new FakeModel(CueSheetEditorPresenter.Pane.CueList);
            var view = new FakeView();
            var parameter = new FakeCueSheetEditorPanePresenter();
            var cueList = new FakeCueListEditorPanePresenter();
            var other = new FakeCueSheetEditorPanePresenter();
            var presenter = new CueSheetEditorPresenter(model, view, parameter, cueList, other);
            presenter.Setup();

            presenter.Dispose();

            Assert.That(parameter.DisposeCount, Is.EqualTo(1));
            Assert.That(cueList.DisposeCount, Is.EqualTo(1));
            Assert.That(other.DisposeCount, Is.EqualTo(1));
        }

        private sealed class FakeModel : ICueSheetEditorModel
        {
            internal FakeModel(CueSheetEditorPresenter.Pane initialPane)
            {
                ObservablePaneState = new ObservableProperty<CueSheetEditorPresenter.Pane>(initialPane);
            }

            public ICueSheetParameterPaneModel CueSheetParameterPaneModel => null!;
            public ICueListEditorPaneModel CueListEditorPaneModel => null!;
            public IOtherOperationPaneModel OtherOperationPaneModel => null!;
            public IObservableProperty<CueSheetEditorPresenter.Pane> ObservablePaneState { get; }
        }

        private sealed class FakeView : ICueSheetEditorView
        {
            private readonly Subject<int> _tabSelected = new();

            internal int DisposeCount { get; private set; }
            internal int SetupCount { get; private set; }
            internal List<int> SelectedTabs { get; } = new();

            public IObservable<int> TabSelectedAsObservable => _tabSelected;

            public void Dispose()
            {
                DisposeCount++;
            }

            public void SelectTab(int tabIndex)
            {
                SelectedTabs.Add(tabIndex);
            }

            public void Setup()
            {
                SetupCount++;
            }

            internal void EmitTabSelected(int tabIndex)
            {
                _tabSelected.OnNext(tabIndex);
            }
        }

        private sealed class FakeCueSheetEditorPanePresenter : ICueSheetEditorPanePresenter
        {
            internal int CloseCount { get; private set; }
            internal int DisposeCount { get; private set; }
            internal int OpenCount { get; private set; }
            internal int SetupCount { get; private set; }

            public void Dispose()
            {
                DisposeCount++;
            }

            public void Setup()
            {
                SetupCount++;
            }

            public void Open()
            {
                OpenCount++;
            }

            public void Close()
            {
                CloseCount++;
            }
        }

        private sealed class FakeCueListEditorPanePresenter : ICueListEditorPanePresenter
        {
            internal int CloseCount { get; private set; }
            internal int DisposeCount { get; private set; }
            internal int OpenCount { get; private set; }
            internal int SetupCount { get; private set; }
            internal List<string> FocusCueArgs { get; } = new();

            public event Action? TrackClipChanged;

            public void Dispose()
            {
                DisposeCount++;
            }

            public void Setup()
            {
                SetupCount++;
            }

            public void Open()
            {
                OpenCount++;
            }

            public void Close()
            {
                CloseCount++;
            }

            public void FocusCue(string cueEditorId)
            {
                FocusCueArgs.Add(cueEditorId);
            }

            internal void RaiseTrackClipChanged()
            {
                TrackClipChanged?.Invoke();
            }
        }
    }
}
