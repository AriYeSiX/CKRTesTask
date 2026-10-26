using UniRx;

namespace CKR.Features.Clicker.Model
{
    public interface IEnergyModel
    {
        IReadOnlyReactiveProperty<int> Current { get; }

        int Max { get; }

        /// <summary>Atomic check-and-spend. Returns false when there is not enough energy.</summary>
        bool TrySpend(int amount);

        void Restore(int amount);
    }
}
