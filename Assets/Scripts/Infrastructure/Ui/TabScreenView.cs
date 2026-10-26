using DG.Tweening;
using UnityEngine;

namespace CKR.Infrastructure.Ui
{
    /// <summary>
    /// Container of a single tab. Hidden screens are deactivated so their layout groups,
    /// animations and raycasts stop costing anything.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    [DisallowMultipleComponent]
    public sealed class TabScreenView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField, Min(0f)] private float _fadeDuration = 0.15f;

        private Tween _tween;

        private void Reset()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        /// <summary>
        /// Resolved lazily: a presenter may hide a screen that starts inactive in the scene,
        /// in which case Awake has not run yet.
        /// </summary>
        private CanvasGroup CanvasGroup =>
            _canvasGroup != null ? _canvasGroup : _canvasGroup = GetComponent<CanvasGroup>();

        private void Awake()
        {
            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();
        }

        public void Show(bool instant = false)
        {
            gameObject.SetActive(true);
            SetInteractable(true);
            Fade(1f, instant);
        }

        public void Hide(bool instant = false)
        {
            SetInteractable(false);

            if (instant || !gameObject.activeInHierarchy)
            {
                KillTween();
                CanvasGroup.alpha = 0f;
                gameObject.SetActive(false);
                return;
            }

            Fade(0f, false, () => gameObject.SetActive(false));
        }

        private void SetInteractable(bool value)
        {
            CanvasGroup.interactable = value;
            CanvasGroup.blocksRaycasts = value;
        }

        private void Fade(float target, bool instant, TweenCallback onComplete = null)
        {
            KillTween();

            if (instant || _fadeDuration <= 0f)
            {
                CanvasGroup.alpha = target;
                onComplete?.Invoke();
                return;
            }

            // DOTween.To instead of the UI module shortcut: keeps this script usable from any
            // assembly definition, regardless of how DOTween modules are set up in the project.
            _tween = DOTween.To(() => CanvasGroup.alpha, value => CanvasGroup.alpha = value, target, _fadeDuration)
                .SetEase(Ease.OutQuad)
                .SetLink(gameObject)
                .OnComplete(() => onComplete?.Invoke());
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
