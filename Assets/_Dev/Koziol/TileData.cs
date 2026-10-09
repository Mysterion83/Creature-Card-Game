using UnityEngine;

[CreateAssetMenu(fileName = "TileData", menuName = "Scriptable Objects/TileData")]
public class TileData : ScriptableObject
{
    public string Name;
    public ElementType Element;
    [Range(0,3)] public int EffectStrength = 0;
    public bool IsPassable = true;
}