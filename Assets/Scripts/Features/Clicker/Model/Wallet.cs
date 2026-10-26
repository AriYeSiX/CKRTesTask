using System;
using CKR.Configs;
using UniRx;

namespace CKR.Features.Clicker.Model
{
    /// <summary>
    /// Soft currency storage. Model layer: knows nothing about views, tweens or the network.
    /// </summary>
    public sealed class Wallet : IWallet, IDisposable
    {
        private readonly ReactiveProperty<long> _balance;

        public Wallet(EconomyConfig config)
        {
            if (!config)
                throw new ArgumentNullException(nameof(config));

            _balance = new ReactiveProperty<long>(config.StartBalance);
        }

        public IReadOnlyReactiveProperty<long> Balance => _balance;

        public void Add(long amount)
        {
            if (amount <= 0)
                return;

            _balance.Value += amount;
        }

        public void Dispose()
        {
            _balance.Dispose();
        }
    }
}
