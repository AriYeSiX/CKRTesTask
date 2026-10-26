using UniRx;

namespace CKR.Features.Navigation
{
    /// <summary>
    /// Single source of truth for "which tab is open".
    /// Every feature reacts to it instead of talking to other features directly.
    /// </summary>
    public interface ITabNavigator
    {
        IReadOnlyReactiveProperty<TabId> ActiveTab { get; }

        void Switch(TabId tab);
    }
}
