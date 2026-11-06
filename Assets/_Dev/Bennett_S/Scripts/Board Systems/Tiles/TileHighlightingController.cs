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
    [SerializeField] private Material _zoneMaterial;
    [SerializeField] private Material _spawnMaterial;

    [Header("Highlight Settings")] 
    [SerializeField] private float _highlightYOffset = 1f;
    
    [Header("Containers")]
    [SerializeField] private GameObject _patternContainer;
    [SerializeField] private GameObject _zoneContainer;
    
    private static readonly int BaseColour = Shader.PropertyToID("_Emission_Colour");
    
    private readonly List<GameObject> _activePatternHighlights = new();
    private readonly List<GameObject> _activeZoneHighlights = new();
    
    /// <summary>
    /// Instantiate a Decal Prefab at a given position and under a parent
    /// </summary>
    /// <param name="position">Position of the Decal</param>
    /// <param name="container">Container object to spawn under</param>
    /// <param name="material">Material to use for the tile</param>
    /// <param name="customColor">Colour to set the tile</param>
    /// <returns></returns>
    private GameObject CreateDecalInstance(Vector3 position, GameObject container, Material material, Color? customColor = null)
    {
        Vector3 spawnPosition = new Vector3(position.x, position.y + _highlightYOffset, position.z);
        GameObject highlight = Instantiate(_decalPrefab, spawnPosition, Quaternion.Euler(90f, 0f, 0f));
        
        if (container)
        {
            highlight.transform.parent = container.transform;
        }

        DecalProjector projector = highlight.GetComponent<DecalProjector>();
        if (!projector || !material) return highlight;
        
        if (customColor.HasValue)
        {
            Material matInstance = new Material(material);
            matInstance.SetColor(BaseColour, customColor.Value);
            projector.material = matInstance;
        }
        else
        {
            projector.material = material;
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
        Material mat = isAttackRange ? _attackMaterial : _moveMaterial;
        GameObject highlight = CreateDecalInstance(position, _patternContainer, mat);
        _activePatternHighlights.Add(highlight);
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
        GameObject highlight = CreateDecalInstance(position, _zoneContainer, _zoneMaterial, playerColor);
        _activeZoneHighlights.Add(highlight);
    }

    public void HighlightZoneTiles(List<Vector3> tilePositions, Color? playerColor = null)
    {
        foreach (var tilePos in tilePositions)
        {
            HighlightZoneTile(tilePos, playerColor);
        }
    }
    #endregion
    
    #region SpawnHighlights
    public void HighlightSpawnTile(Vector3 position)
    {
        Material mat = _spawnMaterial ? _spawnMaterial : _zoneMaterial;
        GameObject highlight = CreateDecalInstance(position, _zoneContainer, mat);
        _activeZoneHighlights.Add(highlight);
    }

    public void HighlightSpawnTiles(List<Vector3> tilePositions)
    {
        foreach (var tilePos in tilePositions)
        {
            HighlightSpawnTile(tilePos);
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