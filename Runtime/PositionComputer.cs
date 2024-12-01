using UnityEngine;

namespace OmicronWindows
{
    public class PositionComputer
    {
        private readonly Canvas _canvas;
        private readonly IPositionProjection _projection;
        private readonly Vector3[] _rectWorldCorners = new Vector3[4];

        public PositionComputer(Canvas canvas, IPositionProjection positionProjection)
        {
            _canvas = canvas;
            _projection = positionProjection;
        }

        public InWindowPosition FromWindowPoint(Vector3 point)
        {
            bool camera = _projection.Camera != null;
            Vector2 pixel = camera ? _projection.Camera.WorldToScreenPoint(point) : point;
            Vector2 screenNormalized = new Vector2(pixel.x / (float)Screen.width, pixel.y / (float)Screen.height);

            return new InWindowPosition()
            {
                ScreenNormalized = screenNormalized,
                ScreenPixel = pixel,
                Canvas = default, // ?
                AbsoluteWorld = point,
                Relative = point,
                RelativeTransform = null,
            };
        }

        public InWindowPosition FromWindowPoint(Transform point) => FromWindowPoint(point.position);

        public InWindowPosition RelativeTo(Vector3 point, RectTransform relativeTo)
        {
            var position = FromWindowPoint(point);
            position.Relative = relativeTo.InverseTransformPoint(point);
            position.RelativeTransform = relativeTo;

            return position;
        }

        public InWindowPosition RelativeTo(Transform point, RectTransform relativeTo) => RelativeTo(point.position, relativeTo);

        public InWindowRect GetRect(RectTransform rectTransform)
        {
            rectTransform.GetWorldCorners(_rectWorldCorners);
            return new InWindowRect()
            {
                LowerLeft = FromWindowPoint(_rectWorldCorners[0]),
                UpperLeft = FromWindowPoint(_rectWorldCorners[1]),
                UpperRight = FromWindowPoint(_rectWorldCorners[2]),
                LowerRight = FromWindowPoint(_rectWorldCorners[3]),
            };
        }
    }
}