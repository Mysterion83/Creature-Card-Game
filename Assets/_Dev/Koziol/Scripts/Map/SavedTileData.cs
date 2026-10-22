using System;

[Serializable]
public struct SavedTileData
{
    public int Region;
    public string TileID;
    public SavedTileData(int region, string tileID)
    {
        Region = region;
        TileID = tileID;
    }
}