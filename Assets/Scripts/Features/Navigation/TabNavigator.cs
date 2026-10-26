using System;
using UniRx;

namespace CKR.Features.Navigation
{
    public sealed class TabNavigator : ITabNavigator, IDisposable
    {
        private readonly ReactiveProperty<TabId> _activeTab = new(TabId.Clicker);

        public IReadOnlyReactiveProperty<TabId> ActiveTab => _activeTab;

        public void Switch(TabId tab)
        {
            if (_activeTab.Value == tab)
                return;

            _activeTab.Value = tab;
        }

        public void Dispose()
        {
            _activeTab.Dispose();
        }
    }
}
