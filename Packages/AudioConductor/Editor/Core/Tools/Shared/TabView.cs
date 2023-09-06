// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System;
using UnityEngine.UIElements;

namespace AudioConductor.Editor.Core.Tools.Shared
{
    internal sealed class TabView : IDisposable
    {
        private const string SelectedClassName = "selected";
        private const string TabClassName = "tab";

        private readonly VisualElement _tabContainer;

        public TabView(VisualElement tabContainer)
        {
            _tabContainer = tabContainer;
        }

        public void Dispose()
        {
            CleanupEventHandlers();
        }

        /// <summary>
        ///     Callback for when the tab selected;
        /// </summary>
        public event Action<int> OnTabSelected;

        public void Setup()
        {
            SetupEventHandlers();
        }

        private void OnClick(ClickEvent evt)
            => OnClick(evt.currentTarget as VisualElement);

        private void OnClick(VisualElement clickedTab)
        {
            if (TabIsCurrentlySelected(clickedTab))
                return;

            GetAllTabs().Where(tab => tab != clickedTab && TabIsCurrentlySelected(tab))
                        .ForEach(tab => tab.RemoveFromClassList(SelectedClassName));
            clickedTab.AddToClassList(SelectedClassName);
            OnTabSelected?.Invoke(clickedTab.tabIndex);
        }

        private static bool TabIsCurrentlySelected(VisualElement tab) => tab.ClassListContains(SelectedClassName);

        private UQueryBuilder<VisualElement> GetAllTabs() => _tabContainer.Query(className: TabClassName);

        public void SelectTab(int tabIndex)
        {
            GetAllTabs().Where(tab => tab.tabIndex == tabIndex).ForEach(OnClick);
        }

        private void SetupEventHandlers()
        {
            GetAllTabs().ForEach(tab => { tab.RegisterCallback<ClickEvent>(OnClick); });
        }

        private void CleanupEventHandlers()
        {
            GetAllTabs().ForEach(tab => { tab.UnregisterCallback<ClickEvent>(OnClick); });
        }
    }
}
