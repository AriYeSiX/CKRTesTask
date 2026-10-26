using System.Globalization;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace CKR.Infrastructure.Ui
{
    /// <summary>
    /// Numeric counter that tweens between values instead of snapping.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AnimatedCounterView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _label;

        [Tooltip("{0} - current value, {1} - optional capacity (energy counter).")]
        [SerializeField] private string _format = "{0}";

        [SerializeField] private bool _useThousandsSeparator = true;

        private Tween _tween;
        private float _displayedValue;
        private long _capacity;

        public void SetCapacity(long capacity)
        {
            _capacity = capacity;
        }

        public void SetImmediate(long value)
        {
            KillTween();
            _displayedValue = value;
            Render();
        }

        public void AnimateTo(long value, float duration)
        {
            if (duration <= 0f)
            {
                SetImmediate(value);
                return;
            }

            KillTween();

            _tween = DOTween.To(() => _displayedValue, current =>
                {
                    _displayedValue = current;
                    Render();
                }, value, duration)
                .SetEase(Ease.OutQuad)
                .SetLink(gameObject);
        }

        private void Render()
        {
            if (!_label)
                return;

            var rounded = (long)Mathf.Round(_displayedValue);
            var value = _useThousandsSeparator
                ? rounded.ToString("N0", CultureInfo.InvariantCulture)
                : rounded.ToString(CultureInfo.InvariantCulture);

            var capacity = _useThousandsSeparator
                ? _capacity.ToString("N0", CultureInfo.InvariantCulture)
                : _capacity.ToString(CultureInfo.InvariantCulture);

            _label.SetText(string.Format(_format, value, capacity));
        }

        private void KillTween()
        {
            _tween?.Kill();
            _tween = null;
        }

        private void OnDestroy()
        {
            KillTween();
        }
    }
}
