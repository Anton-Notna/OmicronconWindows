using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace OmicronWindows
{
    public class Windows : MonoBehaviour, IWindows, IPositionProjection
    {
        [SerializeField]
        private WindowsSource _windowsPrefabs;
        [SerializeField]
        private WindowRoot _rootPrefab;
        [Space]
        [SerializeField]
        private bool _useBackButton = true;
        [SerializeField]
        private KeyCode _back = KeyCode.Escape;
        [Space]
        [SerializeField]
        private Camera _camera;

        private readonly Dictionary<Type, WindowRoot> _live = new Dictionary<Type, WindowRoot>();
        private readonly List<WindowRoot> _destroying = new List<WindowRoot>();
        private readonly List<float> _refreshTimestamps = new List<float>();

        private WindowRoot[] _sorted = new WindowRoot[0];
        private bool _screenOverlap;
        private bool _screenBlock;
        private int _maxSortingOrder;
        private CameraData _previousCameraData;
        private bool _destroyed;

        public Camera Camera => _camera;

        public bool ScreenOverlap => _screenOverlap;

        public bool ScreenBlock => _screenBlock;

        public int MaxSortingOrder => _maxSortingOrder;

        private float CameraOffset => _camera == null ? 0 : _camera.nearClipPlane;

        private float PlaneCoefficient => _camera == null ? 1 : _camera.orthographicSize;

        private int TotalWindows => _live.Count + _destroying.Count;

        private WindowRoot DominateWindow => TotalWindows == 0 ? null : _sorted[TotalWindows - 1];

        private CameraData CurrentCameraData => _camera == null ? default : new CameraData(_camera);

        public void SetCamera(Camera camera) => _camera = camera;

        public void ClearCamera() => _camera = null;

        public bool Contains(Type window) => _live.ContainsKey(window);

        public Window Get(Type window)
        {
            if (_live.TryGetValue(window, out var root) == false)
                throw new NullReferenceException($"There is no live window typeof({window})");

            return root.Window;
        }

        public Window Spawn(Type window, bool forceFront = true)
        {
#if UNITY_EDITOR
            if (Application.isPlaying == false)
                throw new InvalidOperationException("Cannot spawn from editor");
#endif

            if (_destroyed || this == null || gameObject == null || transform == null)
                throw new InvalidOperationException("Windows already destroyed");

            if (Contains(window))
                throw new Exception($"Already contains {window}");

            var prefab = _windowsPrefabs.GetPrefab(window);
            if (prefab == null)
                throw new Exception($"There is no window typeof({window}) in {_windowsPrefabs.name}");

            WindowRoot rootInstance = Instantiate(_rootPrefab, transform);
            Window instance = InstantiateWindow(prefab, rootInstance.transform);

            instance.RootRectTransform.anchoredPosition = Vector2.zero;
            rootInstance.name = $"Root of {instance.gameObject.name}";
            float animationDuration = rootInstance.Init(this, instance);

            _live.Add(window, rootInstance);

            if (forceFront)
                rootInstance.ForceFront();
            else
                MarkDirtyMomentum();

            if (animationDuration > 0f)
                MarkDirtyDelay(animationDuration);

            return instance;
        }

        public bool Destroy(Type window)
        {
            if (_live.TryGetValue(window, out var root) == false)
                return false;

            float animationDuration = root.Destroy();
            _live.Remove(window);
            _destroying.Add(root);
            MarkDirtyMomentum();
            if (animationDuration > 0f)
                MarkDirtyDelay(animationDuration);

            return true;
        }

        internal void MarkDirtyMomentum() => Refresh();

        protected virtual Window InstantiateWindow(Window prefab, Transform root) => Instantiate(prefab, root);

        private void LateUpdate()
        {
            UpdateInput();

            if (ComputeRefreshNeed())
                Refresh();
        }

        private void OnDestroy() => _destroyed = true;

        private void UpdateInput()
        {
            if (_useBackButton == false)
                return;

            if (Input.GetKeyDown(_back) == false)
                return;

            if (DominateWindow == null)
                return;

            DominateWindow.Window.OnBack();
        }

        private bool ComputeRefreshNeed()
        {
            bool refresh = false;
            for (int i = _refreshTimestamps.Count - 1; i >= 0; i--)
            {
                if (Time.time > _refreshTimestamps[i])
                {
                    refresh = true;
                    _refreshTimestamps.RemoveAt(i);
                }
            }

            if (_previousCameraData != CurrentCameraData)
            {
                _previousCameraData = CurrentCameraData;
                refresh = true;
            }

            return refresh;
        }

        private void MarkDirtyDelay(float delay) => _refreshTimestamps.Add(Time.time + delay);

        [ContextMenu(nameof(PrintInfo))]
        private void PrintInfo()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Windows info:");

            stringBuilder.AppendLine($"Total: {TotalWindows}");
            stringBuilder.AppendLine($"DominateWindow: {(DominateWindow == null ? "None" : DominateWindow.name)}");

            stringBuilder.AppendLine("Live windows:");
            foreach (var item in _live)
                stringBuilder.AppendLine($"{item.Key.Name}: {item.Value.name}");

            stringBuilder.AppendLine("Destroying windows:");
            foreach (var item in _destroying)
                stringBuilder.AppendLine(item.name);

            stringBuilder.AppendLine($"Sorted:");
            for (int i = 0; i < TotalWindows; i++)
                stringBuilder.AppendLine((_sorted[i] == null ? "None" : _sorted[i].name));

            Debug.Log(stringBuilder.ToString());
        }

        [ContextMenu(nameof(Refresh))]
        private void Refresh()
        {
            ClearDestroyed();
            SortByOrder();
            RefreshSortingOrder();
            RefreshCulling();
        }

        private void ClearDestroyed()
        {
            for (int i = _destroying.Count - 1; i >= 0; i--)
            {
                if (_destroying[i] == null)
                    _destroying.RemoveAt(i);
            }
        }

        private void SortByOrder()
        {
            int total = TotalWindows;
            if (_sorted.Length < total)
                Array.Resize(ref _sorted, total * 2);

            int index = 0;

            for (int i = 0; i < _destroying.Count; i++)
                _sorted[index++] = _destroying[i];

            foreach (var pair in _live)
                _sorted[index++] = pair.Value;

            for (int i = 0; i < total - 1; i++)
            {
                for (int j = 0; j < total - i - 1; j++)
                {
                    if (_sorted[j].SortingOrder > _sorted[j + 1].SortingOrder)
                    {
                        var temp = _sorted[j];
                        _sorted[j] = _sorted[j + 1];
                        _sorted[j + 1] = temp;
                    }
                }
            }
        }

        private void RefreshSortingOrder()
        {
            float offset = 0;
            for (int i = TotalWindows - 1; i >= 0; i--)
            {
                var window = _sorted[i];
                float planeDistance = CameraOffset + offset + (window.Bounds.Front);

                window.SetCamera(_camera);
                window.SetPlaneDistance(planeDistance);
                window.ApplySortingOrder();

                offset += window.Bounds.Total;
            }

            _maxSortingOrder = DominateWindow == null ? Constants.MinSortingOrder : DominateWindow.SortingOrder;
        }

        private void RefreshCulling()
        {
            bool overlap = false;
            bool block = false;
            for (int i = TotalWindows - 1; i >= 0; i--)
            {
                var window = _sorted[i];

                window.SetCull(block);
                window.RefreshCanvasGroup();

                if (window.ScreenBlock)
                    block = true;
                if (window.ScreenOverlap)
                    overlap = true;
            }

            _screenOverlap = overlap;
            _screenBlock = block;
        }
    }
}