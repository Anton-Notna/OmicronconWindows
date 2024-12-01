using UnityEngine;
using UnityEditor;

namespace OmicronWindows
{
    [CustomEditor(typeof(ResourcesWindowSource))]
    public class ResourcesWindowSourceEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            ResourcesWindowSource windowsPaths = (ResourcesWindowSource)target;
            EditorGUILayout.LabelField("Resources Path", EditorStyles.boldLabel);
            windowsPaths.ResourcesPath = EditorGUILayout.TextField("Path:", windowsPaths.ResourcesPath);

            if (GUILayout.Button("Refresh"))
            {
                windowsPaths.RefreshPaths();
                EditorUtility.SetDirty(windowsPaths);
            }

            GUILayout.Space(10);

            if (windowsPaths.Units != null && windowsPaths.Units.Count > 0)
            {
                foreach (var unit in windowsPaths.Units)
                {
                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.LabelField(unit.Name, EditorStyles.boldLabel);


                    if (GUILayout.Button("Select", GUILayout.Width(60)))
                    {
                        var asset = AssetDatabase.LoadAssetAtPath<Object>(unit.RawPath);
                        if (asset != null)
                            EditorGUIUtility.PingObject(asset);
                        else
                            Debug.LogError($"Prefab not found at path: {unit.RawPath}");
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }
            else
            {
                EditorGUILayout.LabelField("No Windows available.", EditorStyles.centeredGreyMiniLabel);
            }
        }
    }
}
