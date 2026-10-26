using System;

namespace CKR.Features.Weather.Api
{
    /// <summary>
    /// Wire format of https://api.weather.gov/gridpoints/{office}/{x},{y}/forecast.
    /// Field names must match the JSON exactly — JsonUtility ignores everything else.
    /// </summary>
    [Serializable]
    public sealed class WeatherForecastDto
    {
        public WeatherPropertiesDto properties;
    }

    [Serializable]
    public sealed class WeatherPropertiesDto
    {
        public WeatherPeriodDto[] periods;
    }

    [Serializable]
    public sealed class WeatherPeriodDto
    {
        public int number;
        public string name;
        public bool isDaytime;
        public int temperature;
        public string temperatureUnit;
        public string shortForecast;
        public string detailedForecast;
        public string icon;
    }
}
