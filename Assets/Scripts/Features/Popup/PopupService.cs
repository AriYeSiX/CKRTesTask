using System;

namespace CKR.Features.Popup
{
    public sealed class PopupService : IPopupService
    {
        private readonly InfoPopupView _view;

        public PopupService(InfoPopupView view)
        {
            _view = view ? view : throw new ArgumentNullException(nameof(view));
        }

        public void Show(PopupData data)
        {
            _view.Show(data);
        }

        public void Close()
        {
            _view.Close();
        }
    }
}
