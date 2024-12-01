using System;

namespace OmicronWindows
{
    [Serializable]
    public struct WindowBounds
    {
        public float Front;
        public float Back;

        public float Total => Front + Back;

        public static WindowBounds operator *(WindowBounds bounds, float value)
        {
            return new WindowBounds
            {
                Front = bounds.Front * value,
                Back = bounds.Back * value,
            };
        }
    }
}