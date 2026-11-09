using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SnapshotTransitionTest))]
public class SnapshotTransitionTestEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SnapshotTransitionTest transitionTest = (SnapshotTransitionTest)target;

        EditorGUILayout.Space(15);
        EditorGUILayout.LabelField("Testing Controls", EditorStyles.boldLabel);

        bool isPlaying = Application.isPlaying;

        // Grey out buttons if not in Play Mode
        EditorGUI.BeginDisabledGroup(!isPlaying);
        {
            if (GUILayout.Button("Transition to A", GUILayout.Height(25)))
            {
                transitionTest.TransitionToA();
            }
            if (GUILayout.Button("Transition to B", GUILayout.Height(25)))
            {
                transitionTest.TransitionToB();
            }
        }
        EditorGUI.EndDisabledGroup();

        if (isPlaying) return;
        
        EditorGUILayout.Space(5);
        EditorGUILayout.HelpBox("Enter Play Mode to use this test script.", MessageType.Info);
    }
}