using System.Threading;
using CKR.Core.Json;
using CKR.Core.Net;
using Cysharp.Threading.Tasks;

namespace CKR.Core.Requests
{
    /// <summary>
    /// A unit of work for <see cref="IRequestQueue"/>. Requests are plain data objects: they receive
    /// their collaborators on execution rather than capturing them, so they stay cheap to create
    /// and free of any container knowledge.
    /// </summary>
    public interface IApiRequest<TResponse>
    {
        string Description { get; }

        UniTask<TResponse> ExecuteAsync(IWebClient client, IJsonSerializer json,
            CancellationToken cancellationToken);
    }
}
