using CKR.Infrastructure.Ui;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CKR.Features.Popup
{
    /// <summary>
    /// The single popup of the application. Its height follows the content length through
    /// <see cref="AdaptiveContentHeight"/> and falls back to scrolling for long descriptions.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class InfoPopupView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _window;
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _message;
        [SerializeField] private Button _confirmButton;
        [SerializeField] private TMP_Text _confirmLabel;
        [SerializeField] private Button _backdropButton;
        [SerializeField] private AdaptiveContentHeight _adaptiveHeight;

        [Header("Animation")]
        [SerializeField, Min(0.01f)] private float _showDuration = 0.25f;
        [SerializeField, Min(0.01f)] private float _hideDuration = 0.15f;
        [SerializeField] private float _startScale = 0.85f;

        private Sequence _animation;

        public bool IsOpen { get; private set; }

        private void Awake()
        {
            if (_confirmButton != null)
                _confirmButton.onClick.AddListener(Close);

            if (_backdropButton != null)
                _backdropButton.onClick.AddListener(Close);

            // If the popup starts inactive in the scene, Awake runs on the SetActive(true) inside
            // Show — hiding here would immediately undo the show that is in progress.
            if (!IsOpen)
                HideInstant();
        }

        public void Show(PopupData data)
        {
            IsOpen = true;
            gameObject.SetActive(true);

            if (_title)
                _title.SetText(data.Title ?? string.Empty);

            if (_message)
                _message.SetText(data.Message ?? string.Empty);

            if (_confirmLabel && !string.IsNullOrEmpty(data.ConfirmText))
                _confirmLabel.SetText(data.ConfirmText);

            // Height must be recalculated after the texts are assigned, before the show animation.
            if (_adaptiveHeight)
                _adaptiveHeight.Refresh();

            PlayShowAnimation();
        }

        public void Close()
        {
            if (!IsOpen)
                return;

            IsOpen = false;
            SetInteractable(false);

            KillAnimation();

            _animation = DOTween.Sequence()
                .Join(DOTween.To(() => _canvasGroup.alpha, value => _canvasGroup.alpha = value, 0f, _hideDuration))
                .Join(_window.DOScale(_startScale, _hideDuration).SetEase(Ease.InBack))
                .SetLink(gameObject)
                .OnComplete(() => gameObject.SetActive(false));
        }

        private void PlayShowAnimation()
        {
            KillAnimation();

            _canvasGroup.alpha = 0f;
            _window.localScale = Vector3.one * _startScale;
            SetInteractable(true);

            _animation = DOTween.Sequence()
                .Join(DOTween.To(() => _canvasGroup.alpha, value => _canvasGroup.alpha = value, 1f, _showDuration))
                .Join(_window.DOScale(Vector3.one, _showDuration).SetEase(Ease.OutBack))
                .SetLink(gameObject);
        }

        private void HideInstant()
        {
            IsOpen = false;
            SetInteractable(false);

            if (_canvasGroup != null)
                _canvasGroup.alpha = 0f;

            gameObject.SetActive(false);
        }

        private void SetInteractable(bool value)
        {
            if (!_canvasGroup)
                return;

            _canvasGroup.interactable = value;
            _canvasGroup.blocksRaycasts = value;
        }

        private void KillAnimation()
        {
            var animation = _animation;
            _animation = null;

            if (animation != null && animation.IsActive())
                animation.Kill();
        }

        private void OnDestroy()
        {
            KillAnimation();

            if (_confirmButton != null)
                _confirmButton.onClick.RemoveListener(Close);

            if (_backdropButton != null)
                _backdropButton.onClick.RemoveListener(Close);
        }
    }
}
