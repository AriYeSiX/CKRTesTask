using System.Collections.Generic;
using CKR.Infrastructure.Ui;
using TMPro;
using UnityEngine;
using Zenject;

namespace CKR.Features.Dogs.View
{
    /// <summary>
    /// Breeds screen. Owns the pooled rows and the list level loading/error state.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class DogsView : MonoBehaviour
    {
        [SerializeField] private TabScreenView _screen;
        [SerializeField] private RectTransform _rowsRoot;
        [SerializeField] private LoaderSpinnerView _listLoader;
        [SerializeField] private TMP_Text _errorLabel;

        private readonly List<BreedRowView> _rows = new List<BreedRowView>();
        private BreedRowView.Pool _rowPool;

        [Inject]
        public void Construct(BreedRowView.Pool rowPool)
        {
            _rowPool = rowPool;
        }

        public void Show()
        {
            if (_screen)
                _screen.Show();
        }

        public void Hide(bool instant = false)
        {
            if (_screen)
                _screen.Hide(instant);

            SetListLoading(false);
        }

        public void SetListLoading(bool isLoading)
        {
            if (!_listLoader)
                return;

            if (isLoading)
                _listLoader.Show();
            else
                _listLoader.Hide();
        }

        public void ShowError(string message)
        {
            if (!_errorLabel)
                return;

            _errorLabel.gameObject.SetActive(true);
            _errorLabel.SetText(message);
        }

        public void HideError()
        {
            if (_errorLabel)
                _errorLabel.gameObject.SetActive(false);
        }

        public BreedRowView AddRow(BreedRowData data)
        {
            var row = _rowPool.Spawn(data, _rowPool);

            // Rows are parented to the list root by the pool binding (UnderTransform).
            if (_rowsRoot && row.transform.parent != _rowsRoot)
                row.transform.SetParent(_rowsRoot, false);

            row.transform.SetAsLastSibling();
            _rows.Add(row);

            return row;
        }

        public void ClearRows()
        {
            for (var i = 0; i < _rows.Count; i++)
                _rows[i].Dispose();

            _rows.Clear();
        }
    }
}
