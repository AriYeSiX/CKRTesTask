using System;
using CKR.Configs;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Zenject;

namespace CKR.Features.Clicker.View
{
    public readonly struct FlyingCurrencyRequest
    {
        public readonly Vector2 StartPosition;
        public readonly Vector2 TargetPosition;
        public readonly string Text;

        public FlyingCurrencyRequest(Vector2 startPosition, Vector2 targetPosition, string text)
        {
            StartPosition = startPosition;
            TargetPosition = targetPosition;
            Text = text;
        }
    }

    /// <summary>
    /// Reward label that flies from the button to the currency counter.
    /// Pooled through a Zenject memory pool: nothing is instantiated after the pool warm up,
    /// and the object returns itself once its tween completes.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class FlyingCurrencyView : MonoBehaviour, IPoolable<FlyingCurrencyRequest, IMemoryPool>, IDisposable
    {
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TMP_Text _label;

        private ClickerFeedbackConfig _config;
        private IMemoryPool _pool;
        private Sequence _sequence;

        [Inject]
        public void Construct(ClickerFeedbackConfig config)
        {
            _config = config;
        }

        private void Awake()
        {
            if (_rectTransform == null)
                _rectTransform = transform as RectTransform;
        }

        public void OnSpawned(FlyingCurrencyRequest request, IMemoryPool pool)
        {
            _pool = pool;

            KillSequence();

            _rectTransform.anchoredPosition = request.StartPosition;
            _rectTransform.localScale = Vector3.one;
            _canvasGroup.alpha = 1f;
            _label.SetText(request.Text);

            var duration = _config.FlyDuration;
            var fadeStart = _config.FlyFadeStartNormalized;
            var fadeDuration = Mathf.Max(0.01f, duration * (1f - fadeStart));

            _sequence = DOTween.Sequence()
                .Append(DOTween
                    .To(() => _rectTransform.anchoredPosition,
                        value => _rectTransform.anchoredPosition = value,
                        request.TargetPosition, duration)
                    .SetEase(_config.FlyEase))
                .Join(DOTween
                    .To(() => _canvasGroup.alpha, value => _canvasGroup.alpha = value, 0f, fadeDuration)
                    .SetDelay(duration * fadeStart))
                .Join(_rectTransform.DOPunchScale(Vector3.one * _config.FlyPunchScale, 0.3f))
                .SetLink(gameObject)
                .OnComplete(Dispose);
        }

        public void OnDespawned()
        {
            KillSequence();
            _pool = null;
        }

        /// <summary>Returns itself to the pool. Called by the tween, never by the caller.</summary>
        public void Dispose()
        {
            _pool?.Despawn(this);
        }

        private void KillSequence()
        {
            var sequence = _sequence;
            _sequence = null;

            // IsActive() guards the "despawn from the tween callback" path: the sequence is already
            // being auto-killed there and must not be killed twice.
            if (sequence != null && sequence.IsActive())
                sequence.Kill();
        }

        private void OnDestroy()
        {
            KillSequence();
        }

        public class Pool : MonoPoolableMemoryPool<FlyingCurrencyRequest, IMemoryPool, FlyingCurrencyView>
        {
        }
    }
}
