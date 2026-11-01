using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Definition asset for zones within the board
/// </summary>
[CreateAssetMenu(fileName = "New Zone Definition", menuName = "Project/Zone")]
public class ZoneDefinition : ScriptableObject
{
    [Header("Zone Definition")]
    public int ZoneID = -1;
    public string ZoneName;
    [TextArea, SerializeField] private string ZoneDescription;
    public int BonusEnergy;
    [Tooltip("Relative offsets from the center tile that form a zone's area")]
    public List<Vector2Int> RelativeOffsets = new();
}