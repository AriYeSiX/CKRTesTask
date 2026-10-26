using DG.Tweening;
using UnityEngine;

namespace CKR.Infrastructure.Ui
{
    /// <summary>
    /// Reusable spinner. The tween is created once and paused instead of being rebuilt on every
    /// show, so toggling it while scrolling a list does not allocate.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class LoaderSpinnerView : MonoBehaviour
    {
        [SerializeField] private RectTransform _target;
        [SerializeField] private float _degreesPerSecond = -360f;
        [SerializeField] private bool _hideOnAwake = true;

        private Tween _tween;

        private void Awake()
        {
            if (!_target)
                _target = transform as RectTransform;

            if (_hideOnAwake)
                Hide();
        }

        public void Show()
        {
            if (!_target)
                return;

            _target.gameObject.SetActive(true);
            EnsureTween().Play();
        }

        public void Hide()
        {
            _tween?.Pause();

            if (_target)
            {
                _target.localRotation = Quaternion.identity;
                _target.gameObject.SetActive(false);
            }
        }

        private Tween EnsureTween()
        {
            if (_tween != null)
                return _tween;

            var duration = Mathf.Abs(_degreesPerSecond) > 0.01f ? 360f / Mathf.Abs(_degreesPerSecond) : 1f;
            var direction = Mathf.Sign(_degreesPerSecond);

            _tween = _target
                .DOLocalRotate(new Vector3(0f, 0f, 360f * direction), duration, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart)
                .SetLink(gameObject)
                .SetUpdate(true);

            return _tween;
        }

        private void OnDestroy()
        {
            _tween?.Kill();
            _tween = null;
        }
    }
}
