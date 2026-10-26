using System;
using UnityEngine;

namespace CKR.Core.Logging
{
    /// <summary>
    /// Which kinds of diagnostics reach the console. Each kind is switched independently, so
    /// informational noise can be muted without hiding failures.
    /// </summary>
    public interface ILogSettings
    {
        bool InfoEnabled { get; }

        bool WarningsEnabled { get; }

        bool ErrorsEnabled { get; }
    }

    public sealed class UnityLogService : ILogService
    {
        private readonly ILogSettings _settings;

        public UnityLogService(ILogSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public void Info(string tag, string message)
        {
            if (_settings.InfoEnabled)
                Debug.Log(Format(tag, message));
        }

        public void Warning(string tag, string message)
        {
            if (_settings.WarningsEnabled)
                Debug.LogWarning(Format(tag, message));
        }

        public void Error(string tag, string message)
        {
            if (_settings.ErrorsEnabled)
                Debug.LogError(Format(tag, message));
        }

        public void Exception(string tag, Exception exception)
        {
            if (exception == null || !_settings.ErrorsEnabled)
                return;

            Debug.LogError(Format(tag, exception.Message));
            Debug.LogException(exception);
        }

        private static string Format(string tag, string message)
        {
            return string.IsNullOrEmpty(tag) ? message : "[" + tag + "] " + message;
        }
    }
}
