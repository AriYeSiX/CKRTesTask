namespace CKR.Features.Weather.Model
{
    /// <summary>
    /// Domain model of the nearest forecast period. Independent from the wire format,
    /// so switching the weather provider does not touch the UI.
    /// </summary>
    public sealed class WeatherSnapshot
    {
        public WeatherSnapshot(int temperature, string temperatureUnit, string shortForecast,
            string iconUrl)
        {
            Temperature = temperature;
            TemperatureUnit = string.IsNullOrEmpty(temperatureUnit) ? "F" : temperatureUnit;
            ShortForecast = shortForecast;
            IconUrl = iconUrl;
        }

        public int Temperature { get; }
        public string TemperatureUnit { get; }
        public string ShortForecast { get; }
        public string IconUrl { get; }
    }
}
