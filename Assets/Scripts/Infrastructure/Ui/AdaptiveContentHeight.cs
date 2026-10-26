using UnityEngine;
using UnityEngine.UI;

namespace CKR.Infrastructure.Ui
{
    /// <summary>
    /// Resizes a window to its content height, clamps it to a fraction of the available space
    /// and switches the content to scrolling once it no longer fits.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AdaptiveContentHeight : MonoBehaviour
    {
        [SerializeField] private RectTransform _window;
        [SerializeField] private RectTransform _content;
        [SerializeField] private ScrollRect _scrollRect;

        [Tooltip("Reference rect used as the available space. Defaults to the parent of the window.")]
        [SerializeField] private RectTransform _availableSpace;

        [SerializeField, Min(0f)] private float _minHeight = 280f;
        [SerializeField, Range(0.3f, 1f)] private float _maxHeightFraction = 0.8f;

        [Tooltip("Header, footer and paddings that are not part of the measured content.")]
        [SerializeField, Min(0f)] private float _chromeHeight = 220f;

        private void Reset()
        {
            _window = transform as RectTransform;
        }

        /// <summary>Call after the content has been filled.</summary>
        public void Refresh()
        {
            if (!_window || !_content)
                return;

            LayoutRebuilder.ForceRebuildLayoutImmediate(_content);

            var contentHeight = LayoutUtility.GetPreferredHeight(_content);
            var space = _availableSpace ? _availableSpace : _window.parent as RectTransform;
            var availableHeight = space ? space.rect.height : Screen.height;

            var maxHeight = Mathf.Max(_minHeight, availableHeight * _maxHeightFraction);
            var desiredHeight = contentHeight + _chromeHeight;
            var targetHeight = Mathf.Clamp(desiredHeight, _minHeight, maxHeight);

            _window.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetHeight);

            if (_scrollRect)
            {
                var needsScroll = desiredHeight > maxHeight + 0.5f;
                _scrollRect.vertical = needsScroll;
                _scrollRect.enabled = needsScroll;

                if (_scrollRect.content)
                    _scrollRect.content.anchoredPosition = Vector2.zero;

                if (_scrollRect.verticalScrollbar)
                    _scrollRect.verticalScrollbar.gameObject.SetActive(needsScroll);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(_window);
        }
    }
}
