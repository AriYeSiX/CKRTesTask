using System;
using DG.Tweening;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace CKR.Features.Navigation
{
    [DisallowMultipleComponent]
    public sealed class TabButtonView : MonoBehaviour
    {
        [SerializeField] private TabId _tabId;
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _label;
        [SerializeField] private Image _icon;
        [SerializeField] private GameObject _selectionMarker;

        [Header("Visuals")]
        [SerializeField] private Color _selectedColor = Color.white;
        [SerializeField] private Color _normalColor = new Color(0.62f, 0.66f, 0.74f);
        [SerializeField, Min(0.5f)] private float _selectedScale = 1.08f;
        [SerializeField, Min(0.01f)] private float _transitionDuration = 0.18f;

        private Tween _scaleTween;
        private Tween _colorTween;

        public TabId TabId => _tabId;

        public IObservable<Unit> Clicked =>
            _button != null ? _button.OnClickAsObservable() : Observable.Never<Unit>();

        public void SetSelected(bool isSelected, bool instant = false)
        {
            var targetColor = isSelected ? _selectedColor : _normalColor;
            var targetScale = Vector3.one * (isSelected ? _selectedScale : 1f);

            if (_selectionMarker)
                _selectionMarker.SetActive(isSelected);

            _scaleTween?.Kill();
            _colorTween?.Kill();

            if (instant || _transitionDuration <= 0f)
            {
                transform.localScale = targetScale;
                ApplyColor(targetColor);
                return;
            }

            _scaleTween = transform.DOScale(targetScale, _transitionDuration)
                .SetEase(Ease.OutBack)
                .SetLink(gameObject);

            if (_label)
            {
                _colorTween = DOTween.To(() => _label.color, value => _label.color = value, targetColor,
                        _transitionDuration)
                    .SetLink(gameObject);
            }

            if (_icon)
            {
                DOTween.To(() => _icon.color, value => _icon.color = value, targetColor, _transitionDuration)
                    .SetLink(gameObject);
            }
        }

        private void ApplyColor(Color color)
        {
            if (_label)
                _label.color = color;

            if (_icon)
                _icon.color = color;
        }

        private void OnDestroy()
        {
            _scaleTween?.Kill();
            _colorTween?.Kill();
        }
    }
}
