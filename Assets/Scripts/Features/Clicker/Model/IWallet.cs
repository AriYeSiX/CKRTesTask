using UniRx;

namespace CKR.Features.Clicker.Model
{
    public interface IWallet
    {
        IReadOnlyReactiveProperty<long> Balance { get; }

        void Add(long amount);
    }
}
