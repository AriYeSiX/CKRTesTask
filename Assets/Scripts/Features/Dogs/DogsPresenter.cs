using System.Collections.Generic;
using System.Text;
using CKR.Configs;
using CKR.Core.Requests;
using CKR.Features.Dogs.Api;
using CKR.Features.Dogs.Model;
using CKR.Features.Dogs.View;
using CKR.Features.Navigation;
using CKR.Features.Popup;
using Cysharp.Threading.Tasks;
using UniRx;

namespace CKR.Features.Dogs
{
    /// <summary>
    /// Breeds tab.
    ///
    /// Entering the tab queues the breed list; tapping a row queues the details for that breed and
    /// shows a loader inside it. Selecting another row, or leaving the tab, cancels whatever is
    /// still in flight and clears the loader with it.
    /// </summary>
    public sealed class DogsPresenter : TabPresenterBase
    {
        private readonly DogsView _view;
        private readonly IRequestQueue _queue;
        private readonly DogApiConfig _config;
        private readonly IPopupService _popups;

        private readonly CompositeDisposable _rowSubscriptions = new CompositeDisposable();

        private IRequestHandle<IReadOnlyList<DogBreed>> _listRequest;
        private IRequestHandle<DogBreedDetails> _detailsRequest;
        private BreedRowView _loadingRow;

        public DogsPresenter(ITabNavigator navigator, DogsView view, IRequestQueue queue,
            DogApiConfig config, IPopupService popups) : base(navigator)
        {
            _view = view;
            _queue = queue;
            _config = config;
            _popups = popups;
        }

        protected override TabId TabId => TabId.Dogs;

        protected override void OnInitialize()
        {
            _view.Hide(true);
        }

        protected override void OnActivate()
        {
            _view.Show();
            LoadBreeds();
        }

        protected override void OnDeactivate()
        {
            CancelListRequest();
            CancelDetailsRequest();

            _popups.Close();
            _view.Hide();
        }

        protected override void OnDispose()
        {
            CancelListRequest();
            CancelDetailsRequest();
            _rowSubscriptions.Dispose();
        }

        private void LoadBreeds()
        {
            _rowSubscriptions.Clear();
            _view.ClearRows();
            _view.HideError();
            _view.SetListLoading(true);

            _listRequest = _queue.Enqueue(new BreedsListRequest(_config));
            ProcessBreedsAsync(_listRequest).Forget();
        }

        private async UniTaskVoid ProcessBreedsAsync(IRequestHandle<IReadOnlyList<DogBreed>> handle)
        {
            var result = await handle.ResultAsync;

            if (result.IsCanceled || !IsActive)
                return;

            _view.SetListLoading(false);

            if (!result.IsSuccess)
            {
                _view.ShowError(_config.ErrorMessage);
                return;
            }

            RenderBreeds(result.Value);
        }

        private void RenderBreeds(IReadOnlyList<DogBreed> breeds)
        {
            for (var i = 0; i < breeds.Count; i++)
            {
                var row = _view.AddRow(new BreedRowData(i + 1, breeds[i], _config.RowFormat));

                row.Clicked
                    .Subscribe(OnRowClicked)
                    .AddTo(_rowSubscriptions);
            }
        }

        private void OnRowClicked(BreedRowView row)
        {
            if (row == null || row.Breed == null)
                return;

            // The previous details request is dropped before a new one is queued, and its row
            // loader goes away with it.
            CancelDetailsRequest();

            _loadingRow = row;
            row.SetLoading(true);

            _detailsRequest = _queue.Enqueue(new BreedDetailsRequest(_config, row.Breed.Id));
            ProcessDetailsAsync(_detailsRequest).Forget();
        }

        private async UniTaskVoid ProcessDetailsAsync(IRequestHandle<DogBreedDetails> handle)
        {
            var result = await handle.ResultAsync;

            if (result.IsCanceled || !IsActive)
                return;

            StopRowLoader();

            if (!result.IsSuccess)
            {
                _view.ShowError(_config.ErrorMessage);
                return;
            }

            _popups.Show(new PopupData(result.Value.Name, BuildDescription(result.Value), _config.ConfirmText));
        }

        private string BuildDescription(DogBreedDetails details)
        {
            var builder = new StringBuilder(details.Description);

            if (details.LifeMin > 0 || details.LifeMax > 0)
            {
                builder.AppendLine();
                builder.AppendLine();
                builder.AppendFormat(_config.LifeSpanFormat, details.LifeMin, details.LifeMax);
            }

            if (details.Hypoallergenic)
            {
                builder.AppendLine();
                builder.Append(_config.HypoallergenicLabel);
            }

            return builder.ToString();
        }

        private void CancelListRequest()
        {
            _listRequest?.Cancel();
            _listRequest = null;

            _view.SetListLoading(false);
        }

        private void CancelDetailsRequest()
        {
            _detailsRequest?.Cancel();
            _detailsRequest = null;

            StopRowLoader();
        }

        private void StopRowLoader()
        {
            _loadingRow?.SetLoading(false);
            _loadingRow = null;
        }
    }
}
