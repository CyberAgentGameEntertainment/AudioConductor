// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using System.Collections.Generic;
using AudioConductor.Core.Models;
using AudioConductor.Editor.Core.Tools.CueSheetList.Models.Interfaces;
using AudioConductor.Editor.Core.Tools.Shared;
using AudioConductor.Editor.Foundation.TinyRx;
using AudioConductor.Editor.Foundation.TinyRx.ObservableProperty;

namespace AudioConductor.Editor.Core.Tools.CueSheetList.Models
{
    internal sealed class CueSheetListModel : ICueSheetListModel, IDisposable
    {
        private readonly CompositeDisposable _disposable = new();
        private readonly ObservableProperty<CueSheetListItem[]> _items = new(Array.Empty<CueSheetListItem>());
        private readonly Subject<CueSheetAsset> _openRequested = new();
        private readonly ICueSheetAssetRepository _repository;
        private readonly StringObservableProperty _searchFilter = new(string.Empty);
        private readonly Subject<IReadOnlyList<CueSheetAsset>> _validateAllRequested = new();

        public CueSheetListModel(ICueSheetAssetRepository repository)
        {
            _repository = repository;
            _repository.Changed += OnRepositoryChanged;
            _searchFilter.Subscribe(_ => Refresh()).DisposeWith(_disposable);
        }

        public IReadOnlyObservableProperty<CueSheetListItem[]> Items => _items;
        public IObservableProperty<string> SearchFilter => _searchFilter;
        public IObservable<CueSheetAsset> OpenRequested => _openRequested;
        public IObservable<IReadOnlyList<CueSheetAsset>> ValidateAllRequested => _validateAllRequested;

        public void RequestOpen(CueSheetAsset asset)
        {
            _openRequested.OnNext(asset);
        }

        public void RequestValidateAll()
        {
            _validateAllRequested.OnNext(_repository.GetAll());
        }

        public void Dispose()
        {
            _repository.Changed -= OnRepositoryChanged;
            _disposable.Dispose();
            _items.Dispose();
            _searchFilter.Dispose();
            _openRequested.Dispose();
            _validateAllRequested.Dispose();
        }

        private void OnRepositoryChanged()
        {
            Refresh();
        }

        private void Refresh()
        {
            var all = _repository.GetAll();
            var filter = _searchFilter.Value;
            _items.SetValueAndNotify(BuildItems(all, filter));
        }

        private static CueSheetListItem[] BuildItems(CueSheetAsset[] all, string filter)
        {
            var result = new List<CueSheetListItem>(all.Length);
            foreach (var asset in all)
            {
                if (asset == null)
                    continue;
                if (!string.IsNullOrEmpty(filter) &&
                    asset.name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) < 0)
                    continue;
                result.Add(new CueSheetListItem(asset, asset.name, asset.cueSheet.cueList?.Count ?? 0));
            }

            return result.ToArray();
        }
    }
}
