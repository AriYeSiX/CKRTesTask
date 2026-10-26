using System;
using System.Globalization;
using CKR.Configs;
using CKR.Core.Logging;
using CKR.Core.Requests;
using CKR.Features.Navigation;
using CKR.Features.Weather.Api;
using CKR.Features.Weather.Model;
using CKR.Features.Weather.View;
using CKR.Infrastructure.Ui;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;

namespace CKR.Features.Weather
{
    /// <summary>
    /// Weather tab.
    ///
    /// Polls the forecast while the tab is open and stops the moment it is left: a single
    /// <see cref="IRequestHandle.Cancel"/> aborts a running request or drops a queued one.
    /// A new poll is skipped while the previous one is still pending, so a slow network cannot
    /// flood the single lane queue.
    ///
    /// The condition icon is not a local asset: its url comes from the forecast and the image is
    /// downloaded through the same queue, then cached per url so repeated polls do not re-download it.
    /// </summary>
    public sealed class WeatherPresenter : TabPresenterBase
    {
        private readonly WeatherView _view;
        private readonly IRequestQueue _queue;
        private readonly WeatherConfig _config;
        private readonly ISpriteCache _iconCache;
        private readonly ILogService _log;

        private IRequestHandle<WeatherSnapshot> _forecastRequest;
        private IRequestHandle<Sprite> _iconRequest;
        private string _displayedIconUrl;
        private string _pendingIconUrl;

        public WeatherPresenter(ITabNavigator navigator, WeatherView view, IRequestQueue queue,
            WeatherConfig config, ISpriteCache iconCache, ILogService log) : base(navigator)
        {
            _view = view;
            _queue = queue;
            _config = config;
            _iconCache = iconCache;
            _log = log;
        }

        protected override TabId TabId => TabId.Weather;

        protected override void OnInitialize()
        {
            _view.Hide(true);
        }

        protected override void OnActivate()
        {
            _view.Show();

            // Zero due time: the first request goes out immediately, then every PollInterval seconds.
            Observable
                .Timer(TimeSpan.Zero, TimeSpan.FromSeconds(_config.PollIntervalSeconds))
                .Subscribe(_ => EnqueueForecastRequest())
                .AddTo(ActiveScope);
        }

        protected override void OnDeactivate()
        {
            CancelRequests();
            _view.Hide();
        }

        protected override void OnDispose()
        {
            CancelRequests();
        }

        private void EnqueueForecastRequest()
        {
            if (!IsActive)
                return;

            if (_forecastRequest is { IsCompleted: false })
                return;

            if (!_view.HasData)
                _view.SetLoading(true);

            _forecastRequest = _queue.Enqueue(new WeatherForecastRequest(_config));
            ProcessForecastAsync(_forecastRequest).Forget();
        }

        private async UniTaskVoid ProcessForecastAsync(IRequestHandle<WeatherSnapshot> handle)
        {
            var result = await handle.ResultAsync;

            // Cancelled means the tab was left: the view is hidden, there is nothing to update.
            if (result.IsCanceled || !IsActive)
                return;

            _view.SetLoading(false);

            if (!result.IsSuccess)
            {
                _view.ShowError(_config.ErrorMessage);
                return;
            }

            RenderSnapshot(result.Value);
            RequestIcon(result.Value.IconUrl);
        }

        private void RenderSnapshot(WeatherSnapshot snapshot)
        {
            var text = string.Format(
                _config.DisplayFormat,
                _config.TodayLabel,
                snapshot.Temperature.ToString(CultureInfo.InvariantCulture),
                snapshot.TemperatureUnit);

            _view.Render(text, snapshot.ShortForecast);
        }

        private void RequestIcon(string iconUrl)
        {
            if (string.IsNullOrWhiteSpace(iconUrl))
                return;

            // Asking the cache rather than trusting _displayedIconUrl also covers the case where the
            // sprite was evicted while the url stayed the same.
            if (_iconCache.TryGet(iconUrl, out var cached))
            {
                if (_displayedIconUrl != iconUrl)
                {
                    _displayedIconUrl = iconUrl;
                    _view.SetIcon(cached);
                }

                return;
            }

            // Without this guard every poll would cancel the download started by the previous one,
            // and an icon slower than the poll interval would never finish loading.
            if (_pendingIconUrl == iconUrl && _iconRequest != null && !_iconRequest.IsCompleted)
                return;

            // The condition changed while the previous icon was still loading.
            _iconRequest?.Cancel();

            _pendingIconUrl = iconUrl;
            _iconRequest = _queue.Enqueue(new WeatherIconRequest(iconUrl));
            ProcessIconAsync(_iconRequest, iconUrl).Forget();
        }

        private async UniTaskVoid ProcessIconAsync(IRequestHandle<Sprite> handle, string iconUrl)
        {
            var result = await handle.ResultAsync;

            if (_pendingIconUrl == iconUrl)
                _pendingIconUrl = null;

            if (!result.IsSuccess)
            {
                if (result.IsFailed)
                    _log.Warning(LogTags.Weather,
                        $"icon '{iconUrl}' could not be loaded: {result.Error?.Message}");

                return;
            }

            // Hand the sprite to the cache even when the tab is already closed, otherwise the texture
            // it owns would never be destroyed.
            var sprite = _iconCache.Put(iconUrl, result.Value);

            if (!IsActive || !sprite)
                return;

            _displayedIconUrl = iconUrl;
            _view.SetIcon(sprite);
        }

        private void CancelRequests()
        {
            _forecastRequest?.Cancel();
            _forecastRequest = null;

            _iconRequest?.Cancel();
            _iconRequest = null;
            _pendingIconUrl = null;
        }
    }
}
