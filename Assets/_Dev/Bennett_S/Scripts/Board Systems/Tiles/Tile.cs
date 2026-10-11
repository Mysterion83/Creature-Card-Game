using System;
using UnityEngine;

[Serializable]
public class Tile : MonoBehaviour
{
    public TileData Data 
    { 
        set { 
            Data = value;
            OnDataUpdate(); 
        } 
    }
    public int RegionID;
    // Change to creature
    public Creature Occupant;

    private void OnDataUpdate()
    {

    }
}