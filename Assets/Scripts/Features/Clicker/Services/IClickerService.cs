using System;
using CKR.Features.Clicker.Model;

namespace CKR.Features.Clicker.Services
{
    /// <summary>
    /// The only entry point that turns a press into currency. Manual and automatic presses go
    /// through the same rules, so auto collect behaves exactly like a real tap.
    /// The outcome is reported through <see cref="Clicks"/>, including a refused press.
    /// </summary>
    public interface IClickerService
    {
        IObservable<ClickResult> Clicks { get; }

        void Click(ClickSource source);
    }
}
