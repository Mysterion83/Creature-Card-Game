using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "TileData", menuName = "Scriptable Objects/TileData")]
public class TileData : ScriptableObject
{
    #region Inspector Styling
    const string STYLING_HEADER = "<size=200%><margin=0em><align=\"left\"><b>";
    const string STYLING_SUBHEADER = "<size=150%><margin=0em><align=\"left\">";
    #endregion


    public string Name;
    [Header(STYLING_SUBHEADER+"Element Settings")]
    public ElementType Element;
    [Range(0,3)] public int EffectStrength = 0;

    [Header(STYLING_SUBHEADER+"Movement Settings")]
    [Tooltip("Can a creature pass through the tile")]
    public bool IsPassable = true;
    // Used in case of tile being elvated by default (e.g. a mountainous tile)
    public float CreatureYOffset;

    [Header(STYLING_SUBHEADER + "Rendering Settings")]
    // Prefab used for rendering the Tile
    public GameObject RenderObjectPrefab;
}