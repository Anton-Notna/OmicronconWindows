using UnityEngine;

namespace OmicronWindows
{
    public class CanvasGroupFadeWindowAnimator : WindowAnimator
    {
        [SerializeField]
        private CanvasGroup _canvasGroup;

        protected override void ProcessFrame(float normalizedProgress, WindowAnimatorCase @case)
        {
            if (@case == WindowAnimatorCase.Hide)
                normalizedProgress = 1f - normalizedProgress;

            _canvasGroup.alpha = normalizedProgress;
        }
    }
}