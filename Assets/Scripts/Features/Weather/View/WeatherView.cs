using CKR.Infrastructure.Ui;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CKR.Features.Weather.View
{
    /// <summary>
    /// Weather screen: an icon with a single forecast line, plus loading and error states.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class WeatherView : MonoBehaviour
    {
        [SerializeField] private TabScreenView _screen;
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _forecastLabel;
        [SerializeField] private TMP_Text _detailsLabel;
        [SerializeField] private TMP_Text _errorLabel;
        [SerializeField] private LoaderSpinnerView _loader;
        [SerializeField] private CanvasGroup _contentGroup;
        [SerializeField, Min(0f)] private float _refreshFadeDuration = 0.2f;

        private Tween _refreshTween;

        public bool HasData { get; private set; }

        private void Awake()
        {
            // Nothing to show until the first icon is downloaded.
            if (_icon && !_icon.sprite)
                _icon.enabled = false;
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

            SetLoading(false);
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

        /// <summary>The icon is a separate download and arrives after the forecast line.</summary>
        public void SetIcon(Sprite iconSprite)
        {
            if (!_icon)
                return;

            _icon.sprite = iconSprite;
            _icon.enabled = iconSprite;
        }

        public void Render(string forecastText, string detailsText)
        {
            HasData = true;

            if (_errorLabel)
                _errorLabel.gameObject.SetActive(false);

            if (_forecastLabel)
                _forecastLabel.SetText(forecastText);

            if (_detailsLabel)
                _detailsLabel.SetText(detailsText ?? string.Empty);

            PlayRefreshPulse();
        }

        public void ShowError(string message)
        {
            if (!_errorLabel)
                return;

            _errorLabel.gameObject.SetActive(true);
            _errorLabel.SetText(message);
        }

        private void PlayRefreshPulse()
        {
            if (!_contentGroup || _refreshFadeDuration <= 0f)
                return;

            _refreshTween?.Kill();
            _contentGroup.alpha = 0.35f;

            _refreshTween = DOTween
                .To(() => _contentGroup.alpha, value => _contentGroup.alpha = value, 1f, _refreshFadeDuration)
                .SetEase(Ease.OutQuad)
                .SetLink(gameObject);
        }

        private void OnDestroy()
        {
            _refreshTween?.Kill();
        }
    }
}
