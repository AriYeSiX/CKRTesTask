using System;
using UniRx;
using Zenject;

namespace CKR.Features.Navigation
{
    /// <summary>
    /// Base presenter for a tab.
    ///
    /// Gives every feature two lifetimes:
    /// * <see cref="Lifetime"/> — the whole session;
    /// * <see cref="ActiveScope"/> — only while the tab is open.
    ///
    /// Subscriptions added to <see cref="ActiveScope"/> are disposed on tab switch, so timers and
    /// bindings that belong to an open tab stop on their own.
    /// </summary>
    public abstract class TabPresenterBase : IInitializable, IDisposable
    {
        private readonly ITabNavigator _navigator;
        private bool _isDisposed;

        protected TabPresenterBase(ITabNavigator navigator)
        {
            _navigator = navigator ?? throw new ArgumentNullException(nameof(navigator));
        }

        protected abstract TabId TabId { get; }

        protected CompositeDisposable Lifetime { get; } = new CompositeDisposable();

        protected CompositeDisposable ActiveScope { get; } = new CompositeDisposable();

        protected bool IsActive { get; private set; }

        public void Initialize()
        {
            OnInitialize();

            // Fires immediately with the current tab, so the starting tab activates on launch.
            _navigator.ActiveTab
                .Subscribe(OnActiveTabChanged)
                .AddTo(Lifetime);
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            Deactivate();
            Lifetime.Dispose();
            ActiveScope.Dispose();

            OnDispose();
        }

        /// <summary>Called once, before the tab starts reacting to navigation.</summary>
        protected abstract void OnInitialize();

        /// <summary>The tab became visible: show the screen and start its work.</summary>
        protected abstract void OnActivate();

        /// <summary>
        /// The tab is no longer visible. <see cref="ActiveScope"/> is already disposed by this
        /// point; anything held outside it must be released here.
        /// </summary>
        protected abstract void OnDeactivate();

        /// <summary>
        /// Optional: the base class already deactivates the tab and disposes both scopes,
        /// so only state kept outside them needs cleaning up.
        /// </summary>
        protected virtual void OnDispose()
        {
        }

        private void OnActiveTabChanged(TabId tab)
        {
            if (tab == TabId)
                Activate();
            else
                Deactivate();
        }

        private void Activate()
        {
            if (IsActive || _isDisposed)
                return;

            IsActive = true;

            OnActivate();
        }

        private void Deactivate()
        {
            if (!IsActive)
                return;

            IsActive = false;

            ActiveScope.Clear();

            OnDeactivate();
        }
    }
}
