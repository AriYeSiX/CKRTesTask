using UnityEngine;

namespace CKR.Configs
{
    [CreateAssetMenu(fileName = "WeatherConfig", menuName = "CKR/Configs/Weather", order = 3)]
    public sealed class WeatherConfig : ScriptableObject
    {
        [SerializeField] private string _forecastUrl = "https://api.weather.gov/gridpoints/TOP/32,81/forecast";

        [Tooltip("A new forecast request is enqueued with this period while the weather tab is open.")]
        [SerializeField, Min(0.5f)] private float _pollIntervalSeconds = 5f;

        [Header("Presentation")]
        [SerializeField] private string _todayLabel = "Сегодня";
        [Tooltip("{0} - label, {1} - temperature, {2} - unit. Example: 'Сегодня - 61F'.")]
        [SerializeField] private string _displayFormat = "{0} - {1}{2}";
        [SerializeField] private string _errorMessage = "Не удалось загрузить погоду";

        public string ForecastUrl => _forecastUrl;
        public float PollIntervalSeconds => _pollIntervalSeconds;
        public string TodayLabel => _todayLabel;
        public string DisplayFormat => _displayFormat;
        public string ErrorMessage => _errorMessage;
    }
}
