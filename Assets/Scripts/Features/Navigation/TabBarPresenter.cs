using System;
using UniRx;
using Zenject;

namespace CKR.Features.Navigation
{
    /// <summary>
    /// Two-way binding between the bottom bar and the navigator: clicks go up, selection comes down.
    /// </summary>
    public sealed class TabBarPresenter : IInitializable, IDisposable
    {
        private readonly ITabNavigator _navigator;
        private readonly TabBarView _view;
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        public TabBarPresenter(ITabNavigator navigator, TabBarView view)
        {
            _navigator = navigator;
            _view = view;
        }

        public void Initialize()
        {
            _view.TabClicked
                .Subscribe(_navigator.Switch)
                .AddTo(_disposables);

            var isFirst = true;
            _navigator.ActiveTab
                .Subscribe(tab =>
                {
                    _view.SetSelected(tab, isFirst);
                    isFirst = false;
                })
                .AddTo(_disposables);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}
