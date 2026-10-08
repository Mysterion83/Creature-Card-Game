using UnityEngine;

public class TileData
{
    public int ID;
    public int ZoneID;
    public Vector2 Position;
    public ElementType Element;
    [Min(0)] public int EffectStrength;
    public bool IsOccupied;
    public GameObject Occupant;

    // TODO:
    // Refactor Grid Selection Input into separate input class so camera and grid input can work separately
    // Implement this into the grid system
    // Add checks for unit placement so you can't place a unit inside one thats already there
    // Make camera system, click and drag around a 3D point, have bounds so you cant go too low down below the map, could maybe do this through Mathf Clamps
    // Ask Piotr but I can do a grid movement system
    // Add in checkpoint zones MAYBE
}