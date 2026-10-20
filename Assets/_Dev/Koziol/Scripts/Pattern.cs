using UnityEngine;

[CreateAssetMenu(fileName = "PatternSO", menuName = "Scriptable Objects/PatternSO")]
public class Pattern : ScriptableObject
{
    public Vector2Int[] ValidPositions;

    public bool ValidatePosition(Vector2Int position, Vector2Int centre)
    {
        foreach (Vector2Int offset in ValidPositions)
        {
            if (position == centre + offset)
            {
                return true;
            }
        }
        return false;
    }
}
