using System;

namespace OmicronWindows
{
    public interface IWindows
    {
        public bool ScreenOverlap { get; }

        public bool ScreenBlock { get; }

        public bool Contains(Type window);

        public Window Spawn(Type window, bool forceFront = true);

        public bool Destroy(Type window);

        public Window Get(Type window);
    }
}