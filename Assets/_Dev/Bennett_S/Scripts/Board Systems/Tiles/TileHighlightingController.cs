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
    [Space, SerializeField] private Material _zoneMaterial;

    [Header("Highlight Settings")] 
    [SerializeField] private float _highlightYOffset = 1f;
    
    [Header("Containers")]
    [SerializeField] private GameObject _patternContainer;
    [SerializeField] private GameObject _zoneContainer;
    
    private static readonly int BaseColor = Shader.PropertyToID("_Emission_Colour");
    
    private readonly List<GameObject> _activePatternHighlights = new();
    private readonly List<GameObject> _activeZoneHighlights = new();

    /// <summary>
    /// Instantiate a Decal Prefab at a given position and under a parent
    /// </summary>
    /// <param name="position">Position of the Decal</param>
    /// <param name="container">Container object to spawn under</param>
    /// <returns></returns>
    private GameObject CreateDecalInstance(Vector3 position, GameObject container)
    {
        Vector3 spawnPosition = new Vector3(position.x, position.y + _highlightYOffset, position.z);
        GameObject highlight = Instantiate(_decalPrefab, spawnPosition, Quaternion.Euler(90f, 0f, 0f));
        
        if (container != null)
        {
            highlight.transform.parent = container.transform;
        }

        return highlight;
    }
    
    #region Pattern Highlighting (Movement / Attack)
    /// <summary>
    /// Highlight a Tile at a given position for patterns
    /// </summary>
    /// <param name="position">The position of the tile in world space</param>
    /// <param name="isAttackRange">Required: True for attack patterns, false for movement patterns</param>
    public void HighlightPatternTile(Vector3 position, bool isAttackRange)
    {
        GameObject highlight = CreateDecalInstance(position, _patternContainer);
        _activePatternHighlights.Add(highlight);

        DecalProjector projector = highlight.GetComponent<DecalProjector>();
        if (projector != null)
        {
            projector.material = isAttackRange ? _attackMaterial : _moveMaterial;
        }
    }
    
    public void HighlightPatternTiles(List<Vector3> tilePositions, bool isAttackRange)
    {
        foreach (var tilePos in tilePositions)
        {
            HighlightPatternTile(tilePos, isAttackRange);
        }
    }

    #endregion

    #region Zone Highlighting
    /// <summary>
    /// Highlight a Tile at a given position as a zone (White by default, or custom player colour)
    /// </summary>
    /// <param name="position">The position of the tile in world space</param>
    /// <param name="playerColor">Optional custom colour. If null, defaults to White.</param>
    public void HighlightZoneTile(Vector3 position, Color? playerColor = null)
    {
        GameObject highlight = CreateDecalInstance(position, _zoneContainer);
        _activeZoneHighlights.Add(highlight);

        DecalProjector projector = highlight.GetComponent<DecalProjector>();
        if (!projector || !_zoneMaterial) return;
        
        if (playerColor.HasValue)
        {
            Material zoneMaterialInstance = new Material(_zoneMaterial);
            zoneMaterialInstance.SetColor(BaseColor, playerColor.Value);
            projector.material = zoneMaterialInstance;
        }
        else
        {
            projector.material = _zoneMaterial;
        }
    }

    public void HighlightZoneTiles(List<Vector3> tilePositions, Color? playerColor = null)
    {
        foreach (var tilePos in tilePositions)
        {
            HighlightZoneTile(tilePos, playerColor);
        }
    }

    #endregion
    
    #region Cleanup
    /// <summary>
    /// Clears all tile highlights
    /// </summary>
    public void ClearAll()
    {
        ClearPatterns();
        ClearZones();
    }

    /// <summary>
    /// Destroys and clears only the movement/attack pattern highlights
    /// </summary>
    public void ClearPatterns() => ClearList(_activePatternHighlights);

    /// <summary>
    /// Destroys and clears only the zone highlights
    /// </summary>
    public void ClearZones()
    {
        ClearList(_activeZoneHighlights);
    }
    
    private void ClearList(List<GameObject> listToClear)
    {
        foreach (var highlight in listToClear)
        {
            if (!highlight) continue;
            Destroy(highlight);
        }
        listToClear.Clear();
    }
    #endregion
}