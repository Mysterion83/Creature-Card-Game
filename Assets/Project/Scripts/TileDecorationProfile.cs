using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TileData", menuName = "Project/Tile Decoration Profile")]
public class TileDecorationProfile : ScriptableObject
{
    [Header("Decorations")]
    public List<TileDecoration> Decorations = new();
}