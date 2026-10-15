using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BoardBuilder))]
public class BoardBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        BoardBuilder boardBuilder = (BoardBuilder)target;

        if (GUILayout.Button("Load From Asset"))
        {
            boardBuilder.LoadFromAsset();
        }
        if (GUILayout.Button("Clear Tiles"))
        {
            boardBuilder.DestroyTiles();
        }
        if (GUILayout.Button("Build Tiles"))
        {
            boardBuilder.Build();
        }
        if (GUILayout.Button("Construct Map Asset"))
        {
            boardBuilder.ConstructSaveData();
        }
    }
}
