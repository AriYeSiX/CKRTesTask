using System.Threading;
using Cysharp.Threading.Tasks;

namespace CKR.Core.Net
{
    /// <summary>
    /// Transport abstraction. Implementations must abort the underlying connection on cancellation,
    /// not merely drop the result.
    /// </summary>
    public interface IWebClient
    {
        UniTask<HttpResponseData> SendAsync(HttpRequestData request, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Transport tuning, consumed through an interface so the core layer stays independent
    /// of the config assets that provide it.
    /// </summary>
    public interface IWebClientSettings
    {
        string UserAgent { get; }
        int TimeoutSeconds { get; }
        int MaxRetries { get; }
        float RetryBaseDelaySeconds { get; }
    }
}
