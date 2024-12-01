using System;
using System.Collections.Generic;
using UnityEngine;

namespace OmicronWindows
{
    [CreateAssetMenu(menuName = "Windows/Sources/New Prefab Source")]
    public class PrefabWindowsSource : WindowsSource
    {
        [SerializeField]
        private List<Window> _prefabs;

        public override Window GetPrefab(Type type) => _prefabs.Find(x => x.GetType() == type);
    }
}