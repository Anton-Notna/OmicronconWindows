using UnityEngine;

namespace OmicronWindows
{
    public struct InWindowPosition
    {
        public Vector2 ScreenNormalized;
        public Vector2 ScreenPixel;
        public Vector3 Canvas;
        public Vector3 AbsoluteWorld;
        public Vector3 Relative;
        public RectTransform RelativeTransform;
    }
}