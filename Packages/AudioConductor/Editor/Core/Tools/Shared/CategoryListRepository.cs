// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using AudioConductor.Editor.Foundation.TinyRx.ObservableProperty;
using AudioConductor.Runtime.Core.Models;
using UnityEditor;

namespace AudioConductor.Editor.Core.Tools.Shared
{
    internal sealed class CategoryListRepository : ScriptableSingleton<CategoryListRepository>
    {
        public const int InvalidId = -1;

        private static readonly Category Invalid = new()
        {
            id = InvalidId,
            name = "( None )"
        };

        private readonly ObservableProperty<List<int>> _categoryIdList = new();

        private Category[] _categories;

        public string[] CategoryNames { get; private set; }

        public IReadOnlyObservableProperty<List<int>> CategoryIdList => _categoryIdList;

        public void Update()
        {
            var settings = AudioConductorSettingsRepository.instance.Settings;
            var enumerable = settings == null ? Enumerable.Empty<Category>() : settings.categoryList;

            _categories = enumerable.Prepend(Invalid).ToArray();
            CategoryNames = _categories.Select(category => category.name).ToArray();
            _categoryIdList.Value = _categories.Select(category => category.id).ToList();
        }

        public int ToCategoryId(int index)
        {
            if (index < 0 || _categories == null || _categories.Length <= index)
                return InvalidId;

            return _categories[index].id;
        }

        public int ToIndex(int categoryId)
        {
            if (_categories == null)
                return 0;

            for (var i = 0; i < _categories.Length; i++)
                if (categoryId == _categories[i].id)
                    return i;

            return 0;
        }

        public Category Find(int categoryId)
            => _categories?.FirstOrDefault(category => category.id == categoryId) ?? Invalid;

        public string GetName(int categoryId)
            => _categories?.FirstOrDefault(category => category.id == categoryId)?.name ?? Invalid.name;
    }
}
