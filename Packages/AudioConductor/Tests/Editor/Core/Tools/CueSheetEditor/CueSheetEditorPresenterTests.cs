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
            var parameter = new FakePanePresenter();
            var cueList = new FakePanePresenter();
            var other = new FakePanePresenter();

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
            var parameter = new FakePanePresenter();
            var cueList = new FakePanePresenter();
            var other = new FakePanePresenter();

            using var presenter = new CueSheetEditorPresenter(model, view, parameter, cueList, other);
            presenter.Setup();

            model.ObservablePaneState.Value = CueSheetEditorPresenter.Pane.OtherOperation;

            Assert.That(view.SelectedTabs[^1], Is.EqualTo((int)CueSheetEditorPresenter.Pane.OtherOperation));
            Assert.That(parameter.CloseCount, Is.EqualTo(2));
            Assert.That(cueList.CloseCount, Is.EqualTo(1));
            Assert.That(other.OpenCount, Is.EqualTo(1));
        }

        [Test]
        public void TabSelectedByView_UpdatesObservablePaneState()
        {
            var model = new FakeModel(CueSheetEditorPresenter.Pane.CueList);
            var view = new FakeView();

            using var presenter = new CueSheetEditorPresenter(
                model,
                view,
                new FakePanePresenter(),
                new FakePanePresenter(),
                new FakePanePresenter());
            presenter.Setup();

            view.EmitTabSelected((int)CueSheetEditorPresenter.Pane.CueSheetParameter);

            Assert.That(model.ObservablePaneState.Value, Is.EqualTo(CueSheetEditorPresenter.Pane.CueSheetParameter));
        }

        [Test]
        public void Dispose_UnsubscribesViewEvents()
        {
            var model = new FakeModel(CueSheetEditorPresenter.Pane.CueList);
            var view = new FakeView();
            var presenter = new CueSheetEditorPresenter(
                model,
                view,
                new FakePanePresenter(),
                new FakePanePresenter(),
                new FakePanePresenter());
            presenter.Setup();
            presenter.Dispose();

            view.EmitTabSelected((int)CueSheetEditorPresenter.Pane.OtherOperation);

            Assert.That(model.ObservablePaneState.Value, Is.EqualTo(CueSheetEditorPresenter.Pane.CueList));
            Assert.That(view.DisposeCount, Is.EqualTo(1));
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

        private sealed class FakePanePresenter : ICueSheetEditorPanePresenter
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
    }
}
