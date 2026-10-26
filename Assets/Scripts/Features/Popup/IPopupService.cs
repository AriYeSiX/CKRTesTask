namespace CKR.Features.Popup
{
    public readonly struct PopupData
    {
        public readonly string Title;
        public readonly string Message;
        public readonly string ConfirmText;

        public PopupData(string title, string message, string confirmText = null)
        {
            Title = title;
            Message = message;
            ConfirmText = confirmText;
        }
    }

    /// <summary>
    /// Access point to the single shared popup. Features never touch the popup view directly.
    /// </summary>
    public interface IPopupService
    {
        void Show(PopupData data);

        void Close();
    }
}
