using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TileRegistry))]
public class TileRegistryEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        TileRegistry tileRegistry = (TileRegistry)target;

        EditorGUILayout.Space(15);
        EditorGUILayout.LabelField("Registry Controls", EditorStyles.boldLabel);
        
        bool isPlaying = Application.isPlaying;
        
        // Grey out buttons if not in Play Mode
        EditorGUI.BeginDisabledGroup(isPlaying);
        {
            if (GUILayout.Button("Rebuild", GUILayout.Height(25)))
            {
                tileRegistry.Build();
            }
            if (GUILayout.Button("Clear", GUILayout.Height(25)))
            {
                tileRegistry.Clear();
            }
        }
        EditorGUI.EndDisabledGroup();

        if (!isPlaying) return;
        
        EditorGUILayout.Space(5);
        EditorGUILayout.HelpBox("Exit Play Mode to use the registry build controls.", MessageType.Info);
    }
}
