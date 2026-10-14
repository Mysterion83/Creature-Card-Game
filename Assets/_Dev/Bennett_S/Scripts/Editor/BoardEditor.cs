using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BoardBuilder))]
public class BoardEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        BoardBuilder boardBuilder = (BoardBuilder)target;
        if (GUILayout.Button("Build Tiles"))
        {
            boardBuilder.Build();
        }
    }
}
