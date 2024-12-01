using System;
using UnityEngine;

namespace OmicronWindows
{
    public abstract class WindowsSource : ScriptableObject
    {
        public abstract Window GetPrefab(Type type);
    }
}