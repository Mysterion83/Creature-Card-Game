using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Mathf;

[CreateAssetMenu(fileName = "Pattern", menuName = "Project/Pattern")]
public class Pattern : ScriptableObject
{
    public Vector2Int[] ValidPositions => _cachedValidPositions != null ? _cachedValidPositions : EvalulateValidPositions();

    //Cached All Positions for Performance
    private Vector2Int[] _cachedValidPositions;

    [SerializeField] private PatternRuleType _ruleType;
    [SerializeField, Min(1)] private int _patternRadius;

    /// <summary>
    /// Checks if the target position is valid based on the pattern's rule type and radius.
    /// </summary>
    /// <param name="targetPosition">Position to compare</param>
    /// <param name="currentPosition">Centre of the pattern</param>
    /// <returns>If target position is valid</returns>
    public bool ValidatePosition(Vector2Int targetPosition, Vector2Int currentPosition)
    {
        Vector2Int relativePosition = targetPosition - currentPosition;
        foreach (Vector2Int validPosition in ValidPositions)
        {
            if (validPosition == relativePosition) return true;
        }
        return false;
    }

    /// <summary>
    /// Generates and caches all valid positions in the given pattern radius and rule type.
    /// </summary>
    /// <returns>Complete array of valid pattern positions</returns>
    private Vector2Int[] EvalulateValidPositions()
    {
        List<Vector2Int> validPositions = new List<Vector2Int>();

        for (int x = 0; x < _patternRadius*2+1; x++)
        {
            for (int y = 0; y < _patternRadius * 2 + 1; y++)
            {
                if (EvaluateRule(x - _patternRadius, y - _patternRadius))
                {
                    validPositions.Add(new Vector2Int(x - _patternRadius, y - _patternRadius));
                }
            }
        }
        _cachedValidPositions = validPositions.ToArray();
        return _cachedValidPositions;
    }

    private bool EvaluateRule(int x, int y)
    {
        if (x == 0 && y == 0) return false; // Exclude the center position
        if (Abs(x) > _patternRadius || Abs(y) > _patternRadius) return false; // Exclude positions outside the pattern radius
        switch (_ruleType)
        {
            case PatternRuleType.None:
                return false;
            case PatternRuleType.Cross:
                if (x == 0 ^ y == 0) return true;
                break;
            case PatternRuleType.Diagonal:
                if (Abs(x) == Abs(y)) return true;
                break;
            case PatternRuleType.Line:
                if (x == 0 && y >= 0) return true;
                break;
            case PatternRuleType.Ring:
                if (Abs(x) == _patternRadius || Abs(y) == _patternRadius) return true;
                break;
            case PatternRuleType.Jump:
                if (!(Abs(x) == _patternRadius || Abs(y) == _patternRadius)) break;
                if (Abs(x) % 2 == 0 && Abs(y) % 2 == 0) return true;
                break;
        }

        return false;
    }

    private void Awake()
    {
        EvalulateValidPositions();
    }
}