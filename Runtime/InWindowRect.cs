using UnityEngine;

namespace OmicronWindows
{
    public struct InWindowRect
    {
        public InWindowPosition LowerLeft;
        public InWindowPosition UpperLeft;
        public InWindowPosition UpperRight;
        public InWindowPosition LowerRight;

        public InWindowPosition Center
        {
            get
            {
                return new InWindowPosition()
                {
                    ScreenNormalized = Vector2.Lerp(LowerLeft.ScreenNormalized, UpperRight.ScreenNormalized, 0.5f),
                    ScreenPixel = Vector2.Lerp(LowerLeft.ScreenPixel, UpperRight.ScreenPixel, 0.5f),
                    Canvas = Vector2.Lerp(LowerLeft.Canvas, UpperRight.Canvas, 0.5f),
                    AbsoluteWorld = Vector2.Lerp(LowerLeft.AbsoluteWorld, UpperRight.AbsoluteWorld, 0.5f),
                    Relative = Vector2.Lerp(LowerLeft.Relative, UpperRight.Relative, 0.5f),
                    RelativeTransform = null,
                };
            }
        }
    }
}