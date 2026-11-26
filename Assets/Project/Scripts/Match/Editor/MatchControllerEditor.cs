using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MatchController))]
public class MatchControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MatchController matchController = (MatchController)target;

        EditorGUILayout.Space(15);
        EditorGUILayout.LabelField("Testing Controls", EditorStyles.boldLabel);
        
        bool isPlaying = Application.isPlaying;

        // Grey out buttons if not in Play Mode
        EditorGUI.BeginDisabledGroup(!isPlaying);
        {
            if (GUILayout.Button("End Match (Player Won)", GUILayout.Height(25)))
            {
                matchController.EndMatch(true);
            }
            if (GUILayout.Button("End Match (Opponent Won)", GUILayout.Height(25)))
            {
                matchController.EndMatch(false);
            }
        }
        EditorGUI.EndDisabledGroup();

        if (isPlaying) return;
        
        EditorGUILayout.Space(5);
        EditorGUILayout.HelpBox("Enter Play Mode to use these editor features.", MessageType.Info);
    }
}