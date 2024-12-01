using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OmicronWindows
{
    [RequireComponent(typeof(RectTransform))]
    public abstract class Window : MonoBehaviour
    {
        [SerializeField]
        private int _sortingOrder;
        [SerializeField]
        private bool _screenOverlap;
        [SerializeField]
        private bool _screenBlock;
        [SerializeField]
        private WindowBounds _bounds;
        [Space]
        [SerializeField]
        private bool _gizmos;
        [SerializeField, HideInInspector]
        private RectTransform _rootRect;
        [SerializeField, HideInInspector]
        private List<WindowAnimator> _showAnimators;
        [SerializeField, HideInInspector]
        private List<WindowAnimator> _hideAnimators;

        private IWindowRoot _root;
        private PositionComputer _positionComputer;

        public RectTransform RootRectTransform => _rootRect;

        public int SortingOrder => _sortingOrder;

        public bool ScreenOverlap => _screenOverlap;

        public bool ScreenBlock => _screenBlock;

        public WindowBounds Bounds => _bounds;

        public bool InAnimation
        {
            get
            {
                foreach (var animator in _showAnimators)
                {
                    if (animator.InAnimation)
                        return true;
                }

                foreach (var animator in _hideAnimators)
                {
                    if (animator.InAnimation)
                        return true;
                }

                return false;
            }
        }

        internal void Init(IWindowRoot root, PositionComputer positionComputer)
        {
            _root = root;
            _positionComputer = positionComputer;
        }

        internal float StartShowAnimation()
        {
            StopAllAnimators();
            return StartAnimators(_showAnimators);
        }

        internal float StartHideAnimation()
        {
            StopAllAnimators();
            return StartAnimators(_hideAnimators);
        }

        public PositionComputer GetPosition() => _positionComputer;

        public void ForceFront() => _root.ForceFront();

        public void ClearForceFront() => _root.ClearForceFront();

        public void Close() => _root.Close();

        public virtual void OnBack() { }

        protected virtual void OnValidate()
        {
            _sortingOrder = Mathf.Clamp(_sortingOrder, Constants.MinSortingOrder, Constants.MaxSortingOrder);

            var animators = GetComponents<WindowAnimator>();
            _showAnimators = animators.Where(a => a.Case == WindowAnimatorCase.Show).ToList();
            _hideAnimators = animators.Where(a => a.Case == WindowAnimatorCase.Hide).ToList();

            if (_rootRect == null)
                _rootRect = GetComponent<RectTransform>();
        }

        protected virtual void OnDrawGizmos()
        {
            if (_gizmos == false)
                return;

            RectTransform rectTransform = GetComponent<RectTransform>();
            Canvas canvas = GetComponentInParent<Canvas>();

            float scaleFactor = canvas == null ? 1f : canvas.transform.localScale.x;

            Gizmos.color = Color.yellow;

            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);

            Vector3 front = -transform.forward * _bounds.Front * scaleFactor;
            Vector3 back = transform.forward * _bounds.Back * scaleFactor;

            for (int i = 0; i < corners.Length; i++)
            {
                Vector3 corner = corners[i];
                Gizmos.DrawRay(corner, front);
                Gizmos.DrawRay(corner, back);

                Gizmos.DrawLine(corner + front, corners[(i + 1) % corners.Length] + front);
                Gizmos.DrawLine(corner + back, corners[(i + 1) % corners.Length] + back);
            }
        }

        private void StopAllAnimators()
        {
            foreach (var animator in _showAnimators)
                animator.StopAnimation();

            foreach (var animator in _hideAnimators)
                animator.StopAnimation();
        }

        private float StartAnimators(List<WindowAnimator> animators)
        {
            float max = 0;
            foreach (var animator in animators)
            {
                float duration = animator.StartAnimation();
                if (duration > max)
                    max = duration;
            }

            return max;
        }
    }
}