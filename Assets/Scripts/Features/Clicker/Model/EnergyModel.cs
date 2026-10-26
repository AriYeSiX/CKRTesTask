using System;
using CKR.Configs;
using UniRx;
using UnityEngine;

namespace CKR.Features.Clicker.Model
{
    /// <summary>
    /// Energy pool: starts full, is consumed by manual and automatic clicks and refilled by
    /// <see cref="Services.EnergyRegenService"/>.
    /// </summary>
    public sealed class EnergyModel : IEnergyModel, IDisposable
    {
        private readonly ReactiveProperty<int> _current;

        public EnergyModel(EconomyConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            Max = Mathf.Max(1, config.MaxEnergy);
            _current = new ReactiveProperty<int>(Mathf.Clamp(config.StartEnergy, 0, Max));
        }

        public IReadOnlyReactiveProperty<int> Current => _current;

        public int Max { get; }

        public bool TrySpend(int amount)
        {
            if (amount <= 0)
                return true;

            if (_current.Value < amount)
                return false;

            _current.Value -= amount;
            return true;
        }

        public void Restore(int amount)
        {
            if (amount <= 0 || _current.Value >= Max)
                return;

            _current.Value = Mathf.Min(Max, _current.Value + amount);
        }

        public void Dispose()
        {
            _current.Dispose();
        }
    }
}
