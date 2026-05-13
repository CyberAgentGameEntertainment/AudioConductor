// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using AudioConductor.Core.Models;
using AudioConductor.Editor.Core.Tools.CueSheetList.Models;
using AudioConductor.Editor.Core.Tools.CueSheetList.Models.Interfaces;
using AudioConductor.Editor.Core.Tools.CueSheetList.Presenters;
using AudioConductor.Editor.Core.Tools.CueSheetList.Views;
using AudioConductor.Editor.Foundation.TinyRx;
using AudioConductor.Editor.Foundation.TinyRx.ObservableProperty;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AudioConductor.Editor.Core.Tools.CueSheetList.Tests
{
    internal sealed class CueSheetListPresenterTests
    {
        [Test]
        public void Setup_RendersCurrentItems()
        {
            var items = new[] { new CueSheetListItem(ScriptableObject.CreateInstance<CueSheetAsset>(), "Test", 3) };
            var model = new FakeModel();
            model.SetItems(items);
            var view = new FakeView();

            using var presenter = new CueSheetListPresenter(model, view);
            presenter.Setup();

            Assert.That(view.SetupCount, Is.EqualTo(1));
            Assert.That(view.LastRenderedItems, Is.EqualTo(items));

            foreach (var item in items)
                Object.DestroyImmediate(item.Asset);
        }

        [Test]
        public void ModelItems_WhenChanged_UpdatesView()
        {
            var model = new FakeModel();
            var view = new FakeView();

            using var presenter = new CueSheetListPresenter(model, view);
            presenter.Setup();

            var asset = ScriptableObject.CreateInstance<CueSheetAsset>();
            var newItems = new[] { new CueSheetListItem(asset, "NewSheet", 5) };
            model.SetItems(newItems);

            Assert.That(view.LastRenderedItems, Is.EqualTo(newItems));

            Object.DestroyImmediate(asset);
        }

        [Test]
        public void ViewSearchText_UpdatesModelSearchFilter()
        {
            var model = new FakeModel();
            var view = new FakeView();

            using var presenter = new CueSheetListPresenter(model, view);
            presenter.Setup();

            view.EmitSearchText("BGM");

            Assert.That(model.SearchFilter.Value, Is.EqualTo("BGM"));
        }

        [Test]
        public void ViewOpenRequest_CallsModelRequestOpen()
        {
            var model = new FakeModel();
            var view = new FakeView();

            using var presenter = new CueSheetListPresenter(model, view);
            presenter.Setup();

            var asset = ScriptableObject.CreateInstance<CueSheetAsset>();
            view.EmitOpenRequest(asset);

            Assert.That(model.LastOpened, Is.SameAs(asset));

            Object.DestroyImmediate(asset);
        }

        [Test]
        public void Dispose_UnsubscribesViewEvents()
        {
            var model = new FakeModel();
            var view = new FakeView();
            var presenter = new CueSheetListPresenter(model, view);
            presenter.Setup();
            presenter.Dispose();

            view.EmitSearchText("SE");

            Assert.That(model.SearchFilter.Value, Is.EqualTo(string.Empty));
            Assert.That(view.DisposeCount, Is.EqualTo(1));
        }

        [Test]
        public void Dispose_UnsubscribesModelBinding()
        {
            var model = new FakeModel();
            var view = new FakeView();
            var presenter = new CueSheetListPresenter(model, view);
            presenter.Setup();
            presenter.Dispose();

            var asset = ScriptableObject.CreateInstance<CueSheetAsset>();
            var items = new[] { new CueSheetListItem(asset, "After", 0) };
            model.SetItems(items);

            Assert.That(view.LastRenderedItems, Is.Not.EqualTo(items));

            Object.DestroyImmediate(asset);
        }

        private sealed class FakeModel : ICueSheetListModel
        {
            private readonly ObservableProperty<CueSheetListItem[]> _items =
                new(Array.Empty<CueSheetListItem>());

            private readonly Subject<CueSheetAsset> _openRequested = new();
            private readonly StringObservableProperty _searchFilter = new(string.Empty);

            internal CueSheetAsset? LastOpened { get; private set; }

            public IReadOnlyObservableProperty<CueSheetListItem[]> Items => _items;
            public IObservableProperty<string> SearchFilter => _searchFilter;
            public IObservable<CueSheetAsset> OpenRequested => _openRequested;

            public void RequestOpen(CueSheetAsset asset)
            {
                LastOpened = asset;
                _openRequested.OnNext(asset);
            }

            internal void SetItems(CueSheetListItem[] items)
            {
                _items.SetValueAndNotify(items);
            }
        }

        private sealed class FakeView : ICueSheetListView
        {
            private readonly Subject<CueSheetAsset> _openRequested = new();
            private readonly Subject<string> _searchTextChanged = new();

            internal int DisposeCount { get; private set; }
            internal CueSheetListItem[]? LastRenderedItems { get; private set; }
            internal int SetupCount { get; private set; }

            public IObservable<string> SearchTextChangedAsObservable => _searchTextChanged;
            public IObservable<CueSheetAsset> OpenRequestedAsObservable => _openRequested;

            public void Dispose()
            {
                DisposeCount++;
            }

            public void Setup()
            {
                SetupCount++;
            }

            public void RenderItems(CueSheetListItem[] items)
            {
                LastRenderedItems = items;
            }

            public void SetSearchText(string text)
            {
            }

            internal void EmitSearchText(string text)
            {
                _searchTextChanged.OnNext(text);
            }

            internal void EmitOpenRequest(CueSheetAsset asset)
            {
                _openRequested.OnNext(asset);
            }
        }
    }
}
