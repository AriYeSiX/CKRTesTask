using System;
using CKR.Configs;
using CKR.Features.Clicker.Model;
using UniRx;

namespace CKR.Features.Clicker.Services
{
    public sealed class ClickerService : IClickerService, IDisposable
    {
        private readonly IWallet _wallet;
        private readonly IEnergyModel _energy;
        private readonly EconomyConfig _config;
        private readonly Subject<ClickResult> _clicks = new Subject<ClickResult>();

        public ClickerService(IWallet wallet, IEnergyModel energy, EconomyConfig config)
        {
            _wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
            _energy = energy ?? throw new ArgumentNullException(nameof(energy));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public IObservable<ClickResult> Clicks => _clicks;

        public void Click(ClickSource source)
        {
            var cost = source == ClickSource.Manual
                ? _config.EnergyPerManualClick
                : _config.EnergyPerAutoClick;

            if (!_energy.TrySpend(cost))
            {
                _clicks.OnNext(ClickResult.Rejected(source));
                return;
            }

            var reward = _config.RewardPerClick;
            _wallet.Add(reward);

            _clicks.OnNext(ClickResult.Rewarded(source, reward, cost));
        }

        public void Dispose()
        {
            _clicks.OnCompleted();
            _clicks.Dispose();
        }
    }
}
