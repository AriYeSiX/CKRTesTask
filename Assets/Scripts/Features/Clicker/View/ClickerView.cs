using System;
using System.Globalization;
using CKR.Configs;
using CKR.Features.Clicker.Model;
using CKR.Infrastructure.Audio;
using CKR.Infrastructure.Ui;
using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace CKR.Features.Clicker.View
{
    /// <summary>
    /// Clicker screen. Owns presentation only: it reports presses and plays the feedback it is
    /// asked for. Whether a press is rewarded is decided by the model, never here.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ClickerView : MonoBehaviour
    {
        [Header("Screen")]
        [SerializeField] private TabScreenView _screen;

        [Header("Button")]
        [SerializeField] private Button _clickButton;
        [SerializeField] private RectTransform _buttonRect;
        [SerializeField] private Graphic _buttonGraphic;

        [Header("Counters")]
        [SerializeField] private AnimatedCounterView _currencyCounter;
        [SerializeField] private AnimatedCounterView _energyCounter;
        [SerializeField] private Image _energyFill;

        [Header("VFX")]
        [Tooltip("Parent for pooled flying labels and particles. Must be a RectTransform of this canvas.")]
        [SerializeField] private RectTransform _vfxRoot;

        [Tooltip("Where the flying currency lands, usually the currency counter.")]
        [SerializeField] private RectTransform _flyTarget;

        [SerializeField] private string _rewardFormat = "+{0}";

        private ClickerFeedbackConfig _config;
        private FlyingCurrencyView.Pool _flyingCurrencyPool;
        private ClickVfxView.Pool _vfxPool;
        private IAudioService _audio;

        private Color _buttonBaseColor = Color.white;
        private Tween _buttonScaleTween;
        private Tween _buttonColorTween;
        private Tween _shakeTween;
        private Tween _energyFillTween;

        [Inject]
        public void Construct(ClickerFeedbackConfig config, FlyingCurrencyView.Pool flyingCurrencyPool,
            ClickVfxView.Pool vfxPool, IAudioService audio)
        {
            _config = config;
            _flyingCurrencyPool = flyingCurrencyPool;
            _vfxPool = vfxPool;
            _audio = audio;
        }

        public IObservable<Unit> Clicked =>
            _clickButton ? _clickButton.OnClickAsObservable() : Observable.Never<Unit>();

        private void Awake()
        {
            if (_buttonRect == null && _clickButton)
                _buttonRect = _clickButton.transform as RectTransform;

            if (_buttonGraphic)
                _buttonBaseColor = _buttonGraphic.color;
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
        }

        public void SetCurrency(long value, bool instant)
        {
            if (!_currencyCounter)
                return;

            if (instant)
                _currencyCounter.SetImmediate(value);
            else
                _currencyCounter.AnimateTo(value, _config.CounterTweenDuration);
        }

        public void SetEnergy(int value, int max, bool instant)
        {
            if (_energyCounter)
            {
                _energyCounter.SetCapacity(max);

                if (instant)
                    _energyCounter.SetImmediate(value);
                else
                    _energyCounter.AnimateTo(value, _config.CounterTweenDuration);
            }

            if (!_energyFill)
                return;

            var target = max > 0 ? Mathf.Clamp01((float)value / max) : 0f;

            _energyFillTween?.Kill();

            if (instant || _config.CounterTweenDuration <= 0f)
            {
                _energyFill.fillAmount = target;
                return;
            }

            _energyFillTween = DOTween
                .To(() => _energyFill.fillAmount, amount => _energyFill.fillAmount = amount, target,
                    _config.CounterTweenDuration)
                .SetEase(Ease.OutQuad)
                .SetLink(gameObject);
        }

        /// <summary>
        /// Full feedback of a rewarded press: button reaction, particles, flying label and sound.
        /// Automatic presses use the same path as manual ones.
        /// </summary>
        public void PlayRewardFeedback(ClickSource source, long reward)
        {
            PlayButtonReaction();
            SpawnParticles();
            SpawnFlyingCurrency(reward);

            var clip = source == ClickSource.Manual ? _config.ClickSfx : _config.AutoClickSfx;
            _audio.PlaySfx(clip, _config.SfxVolume, _config.SfxPitchJitter);
        }

        public void PlayRejectedFeedback()
        {
            if (_buttonRect)
            {
                _shakeTween?.Kill(true);
                _shakeTween = DOTween
                    .Shake(() => _buttonRect.anchoredPosition3D,
                        value => _buttonRect.anchoredPosition3D = value,
                        _config.RejectShakeDuration, _config.RejectShakeStrength)
                    .SetLink(gameObject);
            }

            if (_buttonGraphic)
            {
                _buttonColorTween?.Kill();
                _buttonColorTween = DOTween
                    .To(() => _buttonGraphic.color, value => _buttonGraphic.color = value, _config.RejectColor,
                        _config.RejectShakeDuration * 0.5f)
                    .SetLoops(2, LoopType.Yoyo)
                    .OnKill(() => _buttonGraphic.color = _buttonBaseColor)
                    .SetLink(gameObject);
            }

            _audio.PlaySfx(_config.RejectSfx, _config.SfxVolume, _config.SfxPitchJitter);
        }

        private void PlayButtonReaction()
        {
            if (!_buttonRect)
                return;

            _buttonScaleTween?.Kill();
            _buttonRect.localScale = Vector3.one;

            _buttonScaleTween = _buttonRect
                .DOPunchScale(Vector3.one * -_config.ReleasePunch, _config.ReleaseDuration, 1, 0.6f)
                .SetEase(_config.ReleaseEase)
                .SetLink(gameObject);
        }

        private void SpawnParticles()
        {
            if (_vfxPool == null || !_vfxRoot)
                return;

            _vfxPool.Spawn(GetButtonPositionInVfxSpace(), _vfxPool);
        }

        private void SpawnFlyingCurrency(long reward)
        {
            if (_flyingCurrencyPool == null || !_vfxRoot)
                return;

            var scatter = new Vector2(
                UnityEngine.Random.Range(-_config.SpawnScatter.x, _config.SpawnScatter.x),
                UnityEngine.Random.Range(-_config.SpawnScatter.y, _config.SpawnScatter.y));

            var start = GetButtonPositionInVfxSpace() + scatter;
            var target = _flyTarget
                ? ToVfxSpace(_flyTarget)
                : start + _config.FlyOffset;

            var text = string.Format(_rewardFormat, reward.ToString(CultureInfo.InvariantCulture));

            _flyingCurrencyPool.Spawn(new FlyingCurrencyRequest(start, target, text), _flyingCurrencyPool);
        }

        private Vector2 GetButtonPositionInVfxSpace()
        {
            return _buttonRect ? ToVfxSpace(_buttonRect) : Vector2.zero;
        }

        private Vector2 ToVfxSpace(RectTransform source)
        {
            var worldCenter = source.TransformPoint(source.rect.center);
            return _vfxRoot.InverseTransformPoint(worldCenter);
        }

        private void OnDestroy()
        {
            _buttonScaleTween?.Kill();
            _buttonColorTween?.Kill();
            _shakeTween?.Kill();
            _energyFillTween?.Kill();
        }
    }
}
