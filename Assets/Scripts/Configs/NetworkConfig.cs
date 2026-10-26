using CKR.Core.Net;
using UnityEngine;

namespace CKR.Configs
{
    [CreateAssetMenu(fileName = "NetworkConfig", menuName = "CKR/Configs/Network", order = 0)]
    public sealed class NetworkConfig : ScriptableObject, IWebClientSettings
    {
        [Header("Transport")]
        [Tooltip("api.weather.gov requires a descriptive User-Agent, anonymous requests are rejected.")]
        [SerializeField] private string _userAgent = "CKRTestTask/1.0 (contact: dev@example.com)";

        [SerializeField, Min(1)] private int _timeoutSeconds = 15;

        [Header("Retry policy")]
        [Tooltip("Applied to transport errors, 408/429 and 5xx only.")]
        [SerializeField, Min(0)] private int _maxRetries = 2;

        [SerializeField, Min(0.05f)] private float _retryBaseDelaySeconds = 0.5f;

        public string UserAgent => _userAgent;
        public int TimeoutSeconds => _timeoutSeconds;
        public int MaxRetries => _maxRetries;
        public float RetryBaseDelaySeconds => _retryBaseDelaySeconds;
    }
}
