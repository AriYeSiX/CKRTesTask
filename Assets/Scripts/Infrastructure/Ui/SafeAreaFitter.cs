using UnityEngine;

namespace CKR.Infrastructure.Ui
{
    /// <summary>
    /// Keeps the content inside the device safe area and re-applies it when the resolution
    /// or orientation changes.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    [DisallowMultipleComponent]
    public sealed class SafeAreaFitter : MonoBehaviour
    {
        [SerializeField] private bool _applyHorizontal = true;
        [SerializeField] private bool _applyVertical = true;

        private RectTransform _rectTransform;
        private Rect _lastSafeArea;
        private Vector2Int _lastScreenSize;
        private ScreenOrientation _lastOrientation;

        private void Awake()
        {
            _rectTransform = (RectTransform)transform;
            Apply();
        }

        private void Update()
        {
            if (_lastSafeArea == Screen.safeArea &&
                _lastScreenSize.x == Screen.width &&
                _lastScreenSize.y == Screen.height &&
                _lastOrientation == Screen.orientation)
            {
                return;
            }

            Apply();
        }

        private void Apply()
        {
            var safeArea = Screen.safeArea;

            _lastSafeArea = safeArea;
            _lastScreenSize = new Vector2Int(Screen.width, Screen.height);
            _lastOrientation = Screen.orientation;

            if (Screen.width <= 0 || Screen.height <= 0)
                return;

            var min = new Vector2(safeArea.xMin / Screen.width, safeArea.yMin / Screen.height);
            var max = new Vector2(safeArea.xMax / Screen.width, safeArea.yMax / Screen.height);

            if (!_applyHorizontal)
            {
                min.x = 0f;
                max.x = 1f;
            }

            if (!_applyVertical)
            {
                min.y = 0f;
                max.y = 1f;
            }

            _rectTransform.anchorMin = min;
            _rectTransform.anchorMax = max;
            _rectTransform.offsetMin = Vector2.zero;
            _rectTransform.offsetMax = Vector2.zero;
        }
    }
}
