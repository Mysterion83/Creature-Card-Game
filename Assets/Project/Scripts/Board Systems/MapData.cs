using UnityEngine;

[System.Serializable]
public struct MapData
{
    [SerializeField, Min(2)] public Vector2Int MapSize;
    public Tile[,] Tiles;

    public MapData(Vector2Int mapSize, Tile[,] tiles)
    {
        MapSize = mapSize;
        Tiles = tiles;
    }
}