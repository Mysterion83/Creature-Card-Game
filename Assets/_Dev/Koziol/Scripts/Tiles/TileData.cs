using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TileData", menuName = "Project/Tile Data")]
public class TileData : ScriptableObject
{
    #region Inspector Styling
    const string STYLING_HEADER = "<size=200%><margin=0em><align=\"left\"><b>";
    const string STYLING_SUBHEADER = "<size=150%><margin=0em><align=\"left\">";
    #endregion

    [SerializeField] private string _stringID;
    public string StringID =>  _stringID;
    
    [SerializeField] private GUID _id;
    public GUID ID => _id;
    
    [SerializeField] private string _name;
    public string Name => _name;
    
    [TextArea, SerializeField] private string _description;
    
    [Header(STYLING_HEADER + "Tile Data Settings")]
    [Header(STYLING_SUBHEADER+"Element Settings")]
    public Element Element;
    [Range(0,3)] public int EffectStrength;

    [Header(STYLING_SUBHEADER+"Movement Settings")]
    [Tooltip("Can a creature pass through the tile")]
    public bool IsPassable = true;
    // Used in case of tile being elevated by default (e.g. a mountainous tile)
    public float CreatureYOffset;

    [Header(STYLING_SUBHEADER + "Rendering Settings")]
    // Prefab used for rendering the Tile
    public GameObject RenderObjectPrefab;
    
    [Header(STYLING_HEADER + "Decorations")]
    public List<TileDecoration> Decorations = new();

    // Possibly change to be determined by a game rule
    public float AttackerTileModifier => EffectStrength * 0.1f + 1;
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!_id.Empty()) return;
        
        _id = GUID.Generate();
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log("[TileData] ID (GUID) has changed, please ensure the TileRegistry is updated");
    }
#endif
}