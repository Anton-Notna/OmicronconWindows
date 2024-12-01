namespace OmicronWindows
{
    public static class WindowsExtensions
    {
        public static T Spawn<T>(this IWindows windows, bool forceFront = true) where T : Window => windows.Spawn(typeof(T), forceFront) as T;

        public static bool Contains<T>(this IWindows windows) where T : Window => windows.Contains(typeof(T));

        public static T Get<T>(this IWindows windows) where T : Window => windows.Get(typeof(T)) as T;

        public static bool Destroy<T>(this IWindows windows) where T : Window => windows.Destroy(typeof(T));
    }
}