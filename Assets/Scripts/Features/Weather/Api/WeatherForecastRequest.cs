using System;
using CKR.Configs;
using CKR.Core.Net;
using CKR.Core.Requests;
using CKR.Features.Weather.Model;

namespace CKR.Features.Weather.Api
{
    public sealed class WeatherForecastRequest : JsonGetRequest<WeatherSnapshot, WeatherForecastDto>
    {
        private readonly WeatherConfig _config;

        public WeatherForecastRequest(WeatherConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public override string Description => "weather:forecast";

        protected override string BuildUrl()
        {
            return _config.ForecastUrl;
        }

        protected override WeatherSnapshot Map(WeatherForecastDto dto)
        {
            var periods = dto.properties?.periods;

            if (periods == null || periods.Length == 0)
                throw new ApiException("Forecast response contains no periods.");

            var period = periods[0];

            return new WeatherSnapshot(
                period.temperature,
                period.temperatureUnit,
                period.shortForecast,
                period.icon);
        }
    }
}
