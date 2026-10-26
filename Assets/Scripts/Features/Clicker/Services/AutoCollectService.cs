using System;
using CKR.Configs;
using CKR.Features.Clicker.Model;
using UniRx;
using Zenject;

namespace CKR.Features.Clicker.Services
{
    /// <summary>
    /// Presses the button on its own at a fixed interval.
    ///
    /// It belongs to the model layer and keeps running while the player is on another tab;
    /// the clicker view is simply not there to show the feedback.
    /// </summary>
    public sealed class AutoCollectService : IInitializable, IDisposable
    {
        private readonly IClickerService _clicker;
        private readonly EconomyConfig _config;
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        public AutoCollectService(IClickerService clicker, EconomyConfig config)
        {
            _clicker = clicker;
            _config = config;
        }

        public void Initialize()
        {
            if (!_config.AutoCollectEnabled)
                return;

            var interval = TimeSpan.FromSeconds(_config.AutoCollectIntervalSeconds);

            Observable.Interval(interval)
                .Subscribe(_ => _clicker.Click(ClickSource.Auto))
                .AddTo(_disposables);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}
