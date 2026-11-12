using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SFXTest))]
public class SFXTestEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        SFXTest sfxTest = (SFXTest)target;
        
        EditorGUILayout.Space(15);
        EditorGUILayout.LabelField("Testing Controls", EditorStyles.boldLabel);
        
        bool isPlaying = Application.isPlaying;

        // Grey out buttons if not in Play Mode
        EditorGUI.BeginDisabledGroup(!isPlaying);
        {
            if (GUILayout.Button("Start Test", GUILayout.Height(25)))
            {
                sfxTest.StartTest();
            }
            if (GUILayout.Button("Stop Test", GUILayout.Height(25)))
            {
                sfxTest.EndTest();
            }
        }
        EditorGUI.EndDisabledGroup();

        if (isPlaying) return;
        
        EditorGUILayout.Space(5);
        EditorGUILayout.HelpBox("Enter Play Mode to use this test script.", MessageType.Info);
    }
}
