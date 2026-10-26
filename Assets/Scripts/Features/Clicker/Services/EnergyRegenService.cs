using System;
using CKR.Configs;
using CKR.Features.Clicker.Model;
using UniRx;
using Zenject;

namespace CKR.Features.Clicker.Services
{
    /// <summary>
    /// Refills the energy pool at a fixed rate, clamped to its maximum.
    /// </summary>
    public sealed class EnergyRegenService : IInitializable, IDisposable
    {
        private readonly IEnergyModel _energy;
        private readonly EconomyConfig _config;
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        public EnergyRegenService(IEnergyModel energy, EconomyConfig config)
        {
            _energy = energy;
            _config = config;
        }

        public void Initialize()
        {
            if (_config.EnergyRegenAmount <= 0)
                return;

            var interval = TimeSpan.FromSeconds(_config.EnergyRegenIntervalSeconds);

            Observable.Interval(interval)
                .Subscribe(_ => _energy.Restore(_config.EnergyRegenAmount))
                .AddTo(_disposables);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}
