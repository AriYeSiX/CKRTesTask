using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;

namespace CKR.Core.Net
{
    /// <summary>
    /// <see cref="UnityWebRequest"/> based transport with exponential backoff for transient failures.
    /// The request object is always disposed, including on cancellation, so no native handle leaks.
    /// </summary>
    public sealed class UnityWebClient : IWebClient
    {
        private readonly IWebClientSettings _settings;

        public UnityWebClient(IWebClientSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public async UniTask<HttpResponseData> SendAsync(HttpRequestData request, CancellationToken cancellationToken)
        {
            var attempt = 0;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var response = await SendOnceAsync(request, cancellationToken);
                if (!response.IsRetryable || attempt >= _settings.MaxRetries)
                    return response;

                attempt++;

                var delaySeconds = _settings.RetryBaseDelaySeconds * Math.Pow(2d, attempt - 1);
                await UniTask.Delay(TimeSpan.FromSeconds(delaySeconds), DelayType.Realtime,
                    cancellationToken: cancellationToken);
            }
        }

        private async UniTask<HttpResponseData> SendOnceAsync(HttpRequestData request,
            CancellationToken cancellationToken)
        {
            using var unityRequest = CreateRequest(request);
            try
            {
                // UniTask aborts the UnityWebRequest itself when the token is cancelled.
                await unityRequest.SendWebRequest().WithCancellation(cancellationToken);
            }
            catch (UnityWebRequestException exception)
            {
                // Protocol/connection errors are a normal outcome here, not an exceptional one.
                return HttpResponseData.Failed(exception.ResponseCode, exception.Text, exception.Error);
            }

            var payload = unityRequest.downloadHandler?.data;
            return HttpResponseData.Completed(unityRequest.responseCode, payload);
        }

        private UnityWebRequest CreateRequest(HttpRequestData request)
        {
            var unityRequest = new UnityWebRequest(request.Url, ToVerb(request.Method))
            {
                downloadHandler = new DownloadHandlerBuffer(),
                timeout = Math.Max(1, _settings.TimeoutSeconds)
            };

            if (!string.IsNullOrEmpty(request.Body))
            {
                var payload = System.Text.Encoding.UTF8.GetBytes(request.Body);
                unityRequest.uploadHandler = new UploadHandlerRaw(payload);
                unityRequest.SetRequestHeader("Content-Type", "application/json");
            }

            // No default Accept header: both JSON and images are fetched from the same host,
            // so the expected content type belongs to the request.

            // api.weather.gov rejects anonymous clients, a descriptive User-Agent is mandatory.
            if (!string.IsNullOrEmpty(_settings.UserAgent))
                unityRequest.SetRequestHeader("User-Agent", _settings.UserAgent);

            if (request.Headers != null)
            {
                for (var i = 0; i < request.Headers.Count; i++)
                {
                    var header = request.Headers[i];
                    if (!string.IsNullOrEmpty(header.Name))
                        unityRequest.SetRequestHeader(header.Name, header.Value);
                }
            }

            return unityRequest;
        }

        private static string ToVerb(HttpMethodType method)
        {
            switch (method)
            {
                case HttpMethodType.Get: return UnityWebRequest.kHttpVerbGET;
                case HttpMethodType.Post: return UnityWebRequest.kHttpVerbPOST;
                case HttpMethodType.Put: return UnityWebRequest.kHttpVerbPUT;
                case HttpMethodType.Delete: return UnityWebRequest.kHttpVerbDELETE;
                default: throw new ArgumentOutOfRangeException(nameof(method), method, null);
            }
        }
    }
}
