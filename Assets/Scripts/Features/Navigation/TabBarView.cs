using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

namespace CKR.Features.Navigation
{
    /// <summary>
    /// Bottom navigation bar. Pure view: it reports clicks and renders the selected state,
    /// the decision of what is selected belongs to <see cref="TabBarPresenter"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TabBarView : MonoBehaviour
    {
        [SerializeField] private List<TabButtonView> _buttons = new List<TabButtonView>();

        private IObservable<TabId> _tabClicked;

        public IObservable<TabId> TabClicked =>
            _tabClicked ??= _buttons
                .Where(button => button != null)
                .Select(button => button.Clicked.Select(_ => button.TabId))
                .Merge()
                .Share();

        public void SetSelected(TabId tabId, bool instant = false)
        {
            for (var i = 0; i < _buttons.Count; i++)
            {
                var button = _buttons[i];
                if (button)
                    button.SetSelected(button.TabId == tabId, instant);
            }
        }
    }
}
