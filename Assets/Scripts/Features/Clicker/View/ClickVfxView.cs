using System;
using DG.Tweening;
using UnityEngine;
using Zenject;

namespace CKR.Features.Clicker.View
{
    /// <summary>
    /// Particle burst played on every press. Pooled and self despawning.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ClickVfxView : MonoBehaviour, IPoolable<Vector2, IMemoryPool>, IDisposable
    {
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private ParticleSystem _particles;

        [Tooltip("Extra time before the object returns to the pool, on top of the particle duration.")]
        [SerializeField, Min(0f)] private float _despawnPadding = 0.25f;

        private IMemoryPool _pool;
        private Tween _despawnTimer;

        private void Awake()
        {
            if (_rectTransform == null)
                _rectTransform = transform as RectTransform;

            if (_particles == null)
                _particles = GetComponentInChildren<ParticleSystem>();
        }

        public void OnSpawned(Vector2 anchoredPosition, IMemoryPool pool)
        {
            _pool = pool;

            KillTimer();

            if (_rectTransform)
                _rectTransform.anchoredPosition = anchoredPosition;

            if (!_particles)
            {
                // Despawning inside Spawn would re-enter the pool, so the release is deferred
                // to the next frame.
                _despawnTimer = DOVirtual.DelayedCall(0f, Dispose, false).SetLink(gameObject);
                return;
            }

            _particles.Clear(true);
            _particles.Play(true);

            var lifetime = _particles.main.duration + _particles.main.startLifetime.constantMax + _despawnPadding;
            _despawnTimer = DOVirtual.DelayedCall(lifetime, Dispose, false).SetLink(gameObject);
        }

        public void OnDespawned()
        {
            KillTimer();

            if (_particles)
                _particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            _pool = null;
        }

        public void Dispose()
        {
            _pool?.Despawn(this);
        }

        private void KillTimer()
        {
            var timer = _despawnTimer;
            _despawnTimer = null;

            if (timer != null && timer.IsActive())
                timer.Kill();
        }

        private void OnDestroy()
        {
            KillTimer();
        }

        public class Pool : MonoPoolableMemoryPool<Vector2, IMemoryPool, ClickVfxView>
        {
        }
    }
}
