using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MusicTest))]
public class MusicTestEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MusicTest musicTest = (MusicTest)target;

        EditorGUILayout.Space(15);
        EditorGUILayout.LabelField("Testing Controls", EditorStyles.boldLabel);
        
        bool isPlaying = Application.isPlaying;

        // Grey out buttons if not in Play Mode
        EditorGUI.BeginDisabledGroup(!isPlaying);
        {
            if (GUILayout.Button("Play Track A", GUILayout.Height(25)))
            {
                musicTest.PlayTrackA();
            }
            if (GUILayout.Button("Play Track B", GUILayout.Height(25)))
            {
                musicTest.PlayTrackB();
            }
            if (GUILayout.Button("Play Ambience", GUILayout.Height(25)))
            {
                musicTest.PlayAmbience();
            }
            if (GUILayout.Button("Play Stinger", GUILayout.Height(25)))
            {
                musicTest.PlayStinger();
            }
            if (GUILayout.Button("Stop Test", GUILayout.Height(25)))
            {
                musicTest.StopTest();
            }
        }
        EditorGUI.EndDisabledGroup();

        if (isPlaying) return;
        
        EditorGUILayout.Space(5);
        EditorGUILayout.HelpBox("Enter Play Mode to use this test script.", MessageType.Info);
    }
}
