using CKR.Features.Clicker.Model;
using CKR.Features.Clicker.Services;
using CKR.Features.Clicker.View;
using CKR.Features.Navigation;
using UniRx;

namespace CKR.Features.Clicker
{
    /// <summary>
    /// Connects the clicker model to its view.
    ///
    /// The model keeps running while the tab is closed (auto collect never stops), the presenter
    /// only decides whether the feedback is worth playing and keeps the counters in sync.
    /// </summary>
    public sealed class ClickerPresenter : TabPresenterBase
    {
        private readonly ClickerView _view;
        private readonly IClickerService _clicker;
        private readonly IWallet _wallet;
        private readonly IEnergyModel _energy;

        public ClickerPresenter(ITabNavigator navigator, ClickerView view, IClickerService clicker,
            IWallet wallet, IEnergyModel energy) : base(navigator)
        {
            _view = view;
            _clicker = clicker;
            _wallet = wallet;
            _energy = energy;
        }

        protected override TabId TabId => TabId.Clicker;

        protected override void OnInitialize()
        {
            _view.Hide(true);

            _wallet.Balance
                .Subscribe(balance => _view.SetCurrency(balance, !IsActive))
                .AddTo(Lifetime);

            _energy.Current
                .Subscribe(energy => _view.SetEnergy(energy, _energy.Max, !IsActive))
                .AddTo(Lifetime);

            _view.Clicked
                .Subscribe(_ => _clicker.Click(ClickSource.Manual))
                .AddTo(Lifetime);

            // Clicks that happen while another tab is open still earn currency,
            // but there is no point in playing particles and sounds for an invisible screen.
            _clicker.Clicks
                .Where(_ => IsActive)
                .Subscribe(OnClick)
                .AddTo(Lifetime);
        }

        protected override void OnActivate()
        {
            _view.Show();

            // Snap the counters to the state accumulated while the tab was closed.
            _view.SetCurrency(_wallet.Balance.Value, true);
            _view.SetEnergy(_energy.Current.Value, _energy.Max, true);
        }

        protected override void OnDeactivate()
        {
            _view.Hide();
        }

        private void OnClick(ClickResult result)
        {
            if (result.IsRewarded)
                _view.PlayRewardFeedback(result.Source, result.Reward);
            else
                _view.PlayRejectedFeedback();
        }
    }
}
