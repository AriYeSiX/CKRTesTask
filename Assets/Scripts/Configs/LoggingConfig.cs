using CKR.Core.Logging;
using UnityEngine;

namespace CKR.Configs
{
    [CreateAssetMenu(fileName = "LoggingConfig", menuName = "CKR/Configs/Logging", order = 5)]
    public sealed class LoggingConfig : ScriptableObject, ILogSettings
    {
        [Header("Visible in the console")]
        [Tooltip("Request queue traffic and other routine diagnostics.")]
        [SerializeField] private bool _info = true;

        [Tooltip("Recoverable problems, such as an icon that failed to download.")]
        [SerializeField] private bool _warnings = true;

        [Tooltip("Failures and exceptions. Keep this on outside of demo builds.")]
        [SerializeField] private bool _errors = true;

        public bool InfoEnabled => _info;
        public bool WarningsEnabled => _warnings;
        public bool ErrorsEnabled => _errors;
    }
}
