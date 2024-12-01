using System.Collections;
using UnityEngine;

namespace OmicronWindows
{
    public abstract class WindowAnimator : MonoBehaviour
    {
        [SerializeField]
        private WindowAnimatorCase _case;
        [SerializeField]
        private float _duration = 1f;

        private Coroutine _process;
        private float? _startTime;

        public WindowAnimatorCase Case => _case;

        public bool InAnimation => _startTime.HasValue && Time.time < _startTime.Value + _duration;

        internal float StartAnimation()
        {
            StopAnimation();
            _startTime = Time.time;
            _process = StartCoroutine(Process());
            return _duration;
        }

        internal void StopAnimation()
        {
            if (_process != null)
            {
                StopCoroutine(_process);
                _process = null;
            }
            _startTime = null;
        }

        protected virtual void OnValidate()
        {
            _duration = Mathf.Clamp(_duration, 0f, float.MaxValue);
        }

        protected abstract void ProcessFrame(float normalizedProgress, WindowAnimatorCase @case);

        private IEnumerator Process()
        {
            if (_duration <= 0f)
            {
                ProcessFrame(1f, _case);
                yield break;
            }

            float time = 0;
            while (time < _duration) 
            {
                ProcessFrame(Mathf.Clamp01(time / _duration), _case);
                yield return null;
                time += Time.deltaTime;
            }

            ProcessFrame(1f, _case);
        }
    }
}