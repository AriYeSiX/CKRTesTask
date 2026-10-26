using Cysharp.Threading.Tasks;

namespace CKR.Core.Requests
{
    /// <summary>
    /// Ticket returned by the queue. Owning the ticket is the only way to influence a request:
    /// cancelling it removes the request from the queue if it has not started yet, or aborts the
    /// live connection if it has.
    /// </summary>
    public interface IRequestHandle
    {
        bool IsCompleted { get; }

        void Cancel();
    }

    public interface IRequestHandle<T> : IRequestHandle
    {
        /// <summary>
        /// Completes when the request reaches a terminal state. Never throws on cancellation —
        /// inspect <see cref="RequestResult{T}.Status"/> instead. Can be awaited more than once.
        /// </summary>
        UniTask<RequestResult<T>> ResultAsync { get; }
    }
}
