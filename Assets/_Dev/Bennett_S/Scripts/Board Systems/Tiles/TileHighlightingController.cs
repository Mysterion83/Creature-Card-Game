using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class TileHighlightingController : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject _decalPrefab;
    
    [Header("Highlighting Materials")]
    [SerializeField] private Material _moveMaterial;
    [SerializeField] private Material _attackMaterial;

    [Header("Highlight Settings")] 
    [SerializeField] private float _highlightYOffset = 1;
    
    [Header("Container")]
    [SerializeField] private GameObject _containerObject;
    
    // Tile tracking
    private readonly List<GameObject> _activeHighlights = new();

    /// <summary>
    /// Highlight a Tile at a given position
    /// </summary>
    /// <param name="position">The positon of the tile in world space</param>
    /// <param name="isAttackRange">If the highlighting is for attack or movement patterns</param>
    public void HighLightTile(Vector3 position, bool isAttackRange)
    {
        Vector3 spawnPosition = new Vector3(position.x, position.y + _highlightYOffset, position.z);
            
        // Spawn a highlight and make it point downwards
        GameObject highlight = Instantiate(_decalPrefab, spawnPosition, Quaternion.Euler(90, 0, 0));
        highlight.transform.parent = _containerObject.transform;
            
        _activeHighlights.Add(highlight);
            
        // Get decal projector and assign its material
        DecalProjector projector = highlight.GetComponent<DecalProjector>();
        if (projector != null) projector.material = isAttackRange ? _attackMaterial : _moveMaterial;
    }
    
    /// <summary>
    /// Highlights all tiles within a given list
    /// </summary>
    /// <param name="tilePositions">List of tile positions in world space</param>
    /// <param name="isAttackRange">If the highlighting is for attack or movement patterns</param>
    public void HighlightTiles(List<Vector3> tilePositions, bool isAttackRange)
    {
        foreach (var tilePos in tilePositions)
        {
            HighLightTile(tilePos, isAttackRange);
        }
    }
    
    /// <summary>
    /// Highlights all tiles within a given array
    /// </summary>
    /// <param name="tilePositions">Array of tile positions in world space</param>
    /// <param name="isAttackRange">If the highlighting is for attack or movement patterns</param>
    public void HighlightTiles(Vector3[] tilePositions, bool isAttackRange)
    {
        foreach (var tilePos in tilePositions)
        {
            HighLightTile(tilePos, isAttackRange);
        }
    }

    /// <summary>
    /// Destroys and clears all highlighted tiles
    /// </summary>
    public void Clear()
    {
        foreach (var highlight in _activeHighlights)
        {
            Destroy(highlight);
        }
        _activeHighlights.Clear();
    }
}
