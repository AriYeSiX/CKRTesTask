using System;

namespace CKR.Core.Logging
{
    /// <summary>
    /// Single entry point for runtime diagnostics. Injected like any other dependency, so a build
    /// can silence whole categories, a test can capture the output and no class reaches for a
    /// global logger of its own.
    /// </summary>
    public interface ILogService
    {
        void Info(string tag, string message);

        void Warning(string tag, string message);

        void Error(string tag, string message);

        void Exception(string tag, Exception exception);
    }
}
