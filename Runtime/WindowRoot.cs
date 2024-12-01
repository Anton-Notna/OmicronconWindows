using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace OmicronWindows
{
    public class WindowRoot : MonoBehaviour, IWindowRoot
    {
        [SerializeField]
        private Canvas _canvas;
        [SerializeField]
        private CanvasScaler _canvasScaler;
        [SerializeField]
        private CanvasGroup _canvasGroup;
        [SerializeField]
        private RectTransform _rectTransform;

        private Windows _windows;
        private Window _window;
        private PositionComputer _positionComputer;
        private int? _overrideSortingOrder;
        private bool _destroying;
        private bool _cull;

        internal Window Window => _window;

        internal int SortingOrder => _overrideSortingOrder.HasValue ? _overrideSortingOrder.Value : _window.SortingOrder;

        internal WindowBounds Bounds => _window.Bounds;

        internal bool ScreenBlock => _window.ScreenBlock && _window.InAnimation == false;

        internal bool ScreenOverlap => _window.ScreenOverlap && _window.InAnimation == false;

        internal float Init(Windows windows, Window window)
        {
            _windows = windows;
            _window = window;
            _positionComputer = new PositionComputer(_canvas, _windows);
            _window.Init(this, _positionComputer);
            float animationDuration = _window.StartShowAnimation();
            Invoke(nameof(RefreshCanvasGroup), animationDuration);
            return animationDuration;
        }

        internal void SetPlaneDistance(float planeDistance)
        { 
            _canvas.planeDistance = planeDistance * _rectTransform.localScale.x;
        }

        internal void ApplySortingOrder() => _canvas.sortingOrder = SortingOrder;

        internal void SetCamera(Camera camera)
        {
            if (camera == null)
            {
                _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }
            else
            {
                _canvas.worldCamera = camera;
                _canvas.renderMode = RenderMode.ScreenSpaceCamera;
            }
        }

        internal void SetCull(bool cull) => _cull = cull;

        internal void RefreshCanvasGroup()
        {
            _canvasGroup.interactable = _cull == false && _window.InAnimation == false;
            _canvasGroup.alpha = _cull ? 0 : 1;
        }

        internal float Destroy()
        {
            _destroying = true;

            float duration = _window.StartHideAnimation();
            if (duration > 0)
                Destroy(gameObject, duration);
            else
                Destroy(gameObject);

            return duration;
        }
        public void ForceFront()
        {
            if (_overrideSortingOrder.HasValue && _overrideSortingOrder.Value == _windows.MaxSortingOrder)
                return;

            _overrideSortingOrder = Mathf.Clamp(_windows.MaxSortingOrder + 1, Constants.MinSortingOrder, Constants.MaxSortingOrder);
            _windows.MarkDirtyMomentum();
        }

        public void ClearForceFront()
        {
            if (_overrideSortingOrder.HasValue == false)
                return;

            _overrideSortingOrder = null;
            _windows.MarkDirtyMomentum();
        }

        public void Close()
        {
            if (_destroying)
                return;

            var type = _window.GetType();
            if (_windows.Contains(type) == false)
                return;

            if (_windows.Get(type).Equals(_window) == false)
                return;

            _windows.Destroy(type);
        }
    }
}