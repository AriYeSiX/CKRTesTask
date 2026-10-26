using DG.Tweening;
using UnityEngine;

namespace CKR.Configs
{
    /// <summary>
    /// Tuning for the click feedback: particles, flying currency, button reaction and sound.
    /// </summary>
    [CreateAssetMenu(fileName = "ClickerFeedbackConfig", menuName = "CKR/Configs/Clicker Feedback", order = 2)]
    public sealed class ClickerFeedbackConfig : ScriptableObject
    {
        [Header("Button reaction")]
        [SerializeField, Min(0f)] private float _releasePunch = 0.12f;
        [SerializeField, Min(0.01f)] private float _releaseDuration = 0.22f;
        [SerializeField] private Ease _releaseEase = Ease.OutBack;

        [Header("Rejected click (no energy)")]
        [SerializeField, Min(0f)] private float _rejectShakeStrength = 12f;
        [SerializeField, Min(0.01f)] private float _rejectShakeDuration = 0.3f;
        [SerializeField] private Color _rejectColor = new Color(1f, 0.45f, 0.45f);

        [Header("Flying currency")]
        [SerializeField, Min(0.01f)] private float _flyDuration = 0.9f;
        [SerializeField] private Vector2 _flyOffset = new Vector2(0f, 260f);
        [SerializeField] private Vector2 _spawnScatter = new Vector2(60f, 30f);
        [SerializeField] private Ease _flyEase = Ease.OutCubic;
        [SerializeField, Min(0f)] private float _flyFadeStartNormalized = 0.6f;
        [SerializeField, Min(0f)] private float _flyPunchScale = 0.25f;

        [Header("Counters")]
        [SerializeField, Min(0f)] private float _counterTweenDuration = 0.25f;

        [Header("Audio")]
        [SerializeField] private AudioClip _clickSfx;
        [SerializeField] private AudioClip _autoClickSfx;
        [SerializeField] private AudioClip _rejectSfx;
        [SerializeField, Range(0f, 1f)] private float _sfxVolume = 0.8f;
        [SerializeField, Range(0f, 0.5f)] private float _sfxPitchJitter = 0.08f;

        public float ReleasePunch => _releasePunch;
        public float ReleaseDuration => _releaseDuration;
        public Ease ReleaseEase => _releaseEase;

        public float RejectShakeStrength => _rejectShakeStrength;
        public float RejectShakeDuration => _rejectShakeDuration;
        public Color RejectColor => _rejectColor;

        public float FlyDuration => _flyDuration;
        public Vector2 FlyOffset => _flyOffset;
        public Vector2 SpawnScatter => _spawnScatter;
        public Ease FlyEase => _flyEase;
        public float FlyFadeStartNormalized => Mathf.Clamp01(_flyFadeStartNormalized);
        public float FlyPunchScale => _flyPunchScale;

        public float CounterTweenDuration => _counterTweenDuration;

        public AudioClip ClickSfx => _clickSfx;
        public AudioClip AutoClickSfx => _autoClickSfx != null ? _autoClickSfx : _clickSfx;
        public AudioClip RejectSfx => _rejectSfx;
        public float SfxVolume => _sfxVolume;
        public float SfxPitchJitter => _sfxPitchJitter;
    }
}
