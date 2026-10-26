using System.Threading;
using CKR.Core.Json;
using CKR.Core.Net;
using Cysharp.Threading.Tasks;

namespace CKR.Core.Requests
{
    /// <summary>
    /// Template for "GET JSON -> domain model" requests.
    /// Keeps the wire format (<typeparamref name="TDto"/>) strictly separated from the model the
    /// presentation layer consumes (<typeparamref name="TResponse"/>).
    /// </summary>
    public abstract class JsonGetRequest<TResponse, TDto> : IApiRequest<TResponse>
    {
        private static readonly HttpHeader[] JsonHeaders = { new HttpHeader("Accept", "application/json") };

        public abstract string Description { get; }

        protected abstract string BuildUrl();

        protected abstract TResponse Map(TDto dto);

        public async UniTask<TResponse> ExecuteAsync(IWebClient client, IJsonSerializer json,
            CancellationToken cancellationToken)
        {
            var url = BuildUrl();

            var response = await client.SendAsync(HttpRequestData.Get(url, JsonHeaders), cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            if (!response.IsSuccess)
                throw ApiException.FromResponse(url, response);

            var dto = json.Deserialize<TDto>(response.Body);
            return dto == null ? throw new ApiException($"Request to '{url}' returned an unreadable payload.", response.StatusCode) : Map(dto);
        }
    }
}
