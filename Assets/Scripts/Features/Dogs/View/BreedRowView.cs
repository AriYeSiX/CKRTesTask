using System;
using CKR.Features.Dogs.Model;
using CKR.Infrastructure.Ui;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace CKR.Features.Dogs.View
{
    public readonly struct BreedRowData
    {
        public readonly int Number;
        public readonly DogBreed Breed;
        public readonly string Format;

        public BreedRowData(int number, DogBreed breed, string format)
        {
            Number = number;
            Breed = breed;
            Format = format;
        }
    }

    /// <summary>
    /// One row of the breeds list, with a loader shown while the details for it are being fetched.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BreedRowView : MonoBehaviour, IPoolable<BreedRowData, IMemoryPool>, IDisposable
    {
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _label;
        [SerializeField] private LoaderSpinnerView _loader;

        private IMemoryPool _pool;

        public DogBreed Breed { get; private set; }

        /// <summary>Emits the row itself, so the presenter can read the breed and drive its loader.</summary>
        public IObservable<BreedRowView> Clicked =>
            _button ? _button.OnClickAsObservable().Select(_ => this)
                : Observable.Never<BreedRowView>();

        public void OnSpawned(BreedRowData data, IMemoryPool pool)
        {
            _pool = pool;
            Breed = data.Breed;

            if (_label)
                _label.SetText(string.Format(data.Format, data.Number, data.Breed?.Name));

            SetLoading(false);

            // Pooled objects keep the sibling index of their previous life, so order is restored here.
            transform.SetAsLastSibling();
        }

        public void OnDespawned()
        {
            SetLoading(false);
            Breed = null;
            _pool = null;
        }

        public void SetLoading(bool isLoading)
        {
            if (!_loader)
                return;

            if (isLoading)
                _loader.Show();
            else
                _loader.Hide();
        }

        public void Dispose()
        {
            _pool?.Despawn(this);
        }

        public class Pool : MonoPoolableMemoryPool<BreedRowData, IMemoryPool, BreedRowView>
        {
        }
    }
}
