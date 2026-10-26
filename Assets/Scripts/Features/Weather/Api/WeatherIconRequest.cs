using System;
using System.Threading;
using CKR.Core.Json;
using CKR.Core.Net;
using CKR.Core.Requests;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CKR.Features.Weather.Api
{
    /// <summary>
    /// Downloads the condition icon the forecast points at (the "icon" field of a period, e.g.
    /// https://api.weather.gov/icons/land/day/few?size=medium) and turns it into a sprite.
    ///
    /// A regular queued request: it runs after the forecast that produced the url, can be dropped
    /// from the queue and is aborted when the tab is left.
    /// </summary>
    public sealed class WeatherIconRequest : IApiRequest<Sprite>
    {
        private static readonly HttpHeader[] ImageHeaders =
        {
            new HttpHeader("Accept", "image/png,image/*;q=0.8")
        };

        private readonly string _iconUrl;

        public WeatherIconRequest(string iconUrl)
        {
            if (string.IsNullOrWhiteSpace(iconUrl))
                throw new ArgumentException("Icon url must not be empty.", nameof(iconUrl));

            _iconUrl = iconUrl;
        }

        public string Description => "weather:icon";

        public async UniTask<Sprite> ExecuteAsync(IWebClient client, IJsonSerializer json,
            CancellationToken cancellationToken)
        {
            var response = await client.SendAsync(HttpRequestData.Get(_iconUrl, ImageHeaders),
                cancellationToken);

            // Nothing is allocated before this point, so a cancellation here cannot leak a texture.
            cancellationToken.ThrowIfCancellationRequested();

            if (!response.IsSuccess)
                throw ApiException.FromResponse(_iconUrl, response);

            if (response.RawData == null || response.RawData.Length == 0)
                throw new ApiException($"Icon '{_iconUrl}' returned an empty image.", response.StatusCode);

            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };

            // markNonReadable: the pixels are never read back, so the CPU copy is dropped.
            if (!texture.LoadImage(response.RawData, true))
            {
                UnityEngine.Object.Destroy(texture);
                throw new ApiException($"Icon '{_iconUrl}' is not a readable image.", response.StatusCode);
            }

            return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f));
        }
    }
}
