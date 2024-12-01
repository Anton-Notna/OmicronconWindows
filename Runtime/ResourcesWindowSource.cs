using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System.Text.RegularExpressions;



#if UNITY_EDITOR
using UnityEditor;
#endif

namespace OmicronWindows
{
    [CreateAssetMenu(menuName = "Windows/Sources/New Resources Source")]
    public class ResourcesWindowSource : WindowsSource
    {
        [Serializable]
        public class Unit
        {
            [SerializeField]
            private string _name;
            [SerializeField]
            private string _type;
            [SerializeField]
            private string _path;
            [SerializeField]
            private string _rawPath;

            public string Name => _name;

            public Type UnitType => JsonConvert.DeserializeObject<Type>(_type);

            public string Path => _path;

            public string RawPath => _rawPath;

            public Unit(Type type, string path, string rawPath)
            {
                _name = type.Name;
                _type = JsonConvert.SerializeObject(type);
                _path = path;
                _rawPath = rawPath;
            }
        }

        [SerializeField]
        private string _resourcesPath = "Windows";
        [SerializeField]
        private List<Unit> _unitList = new List<Unit>();

        private Dictionary<Type, string> _prefabPaths;

        public string ResourcesPath
        {
            get => _resourcesPath;
            set => _resourcesPath = value;
        }

        public IReadOnlyList<Unit> Units => _unitList;

        public override Window GetPrefab(Type type)
        {
            EnsurePathsInitialized();

            if (_prefabPaths.TryGetValue(type, out var path))
                return Resources.Load<Window>(path);

            return null;
        }

        private void EnsurePathsInitialized()
        {
            if (_prefabPaths != null)
                return;

            _prefabPaths = new Dictionary<Type, string>();
            foreach (var unit in _unitList)
            {
                _prefabPaths[unit.UnitType] = unit.Path;
                Debug.Log(unit.UnitType);
                Debug.Log(_prefabPaths[unit.UnitType]);

            }
        }

#if UNITY_EDITOR
        [ContextMenu(nameof(RefreshPaths))]
        public void RefreshPaths()
        {
            var prefabPaths = new Dictionary<Type, string>();
            var prefabs = Resources.LoadAll<Window>(_resourcesPath);

            foreach (var prefab in prefabs)
            {
                var type = prefab.GetType();

                if (prefabPaths.ContainsKey(type))
                {
                    Debug.LogError($"Duplicate windows of type {type} in {_resourcesPath}\nFirst: {prefabPaths[type]}\nSecond: {prefab.name}");
                    continue;
                }

                var assetPath = AssetDatabase.GetAssetPath(prefab);
                prefabPaths[type] = assetPath;
            }

            UpdateUnits(prefabPaths);
        }

        private static string ConvertToResourcesPath(string assetPath)
        {
            return 
                Regex.Replace(assetPath, @"Assets/.+?Resources/", string.Empty)
                .Replace(".prefab", string.Empty);
        }

        private void UpdateUnits(Dictionary<Type, string> paths)
        {
            if (_unitList == null)
                _unitList = new List<Unit>();

            _unitList.Clear();
            foreach (var kvp in paths)
                _unitList.Add(new Unit(kvp.Key, ConvertToResourcesPath(kvp.Value), kvp.Value));
        }
#endif
    }
}
