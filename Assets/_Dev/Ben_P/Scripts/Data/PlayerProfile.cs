using System;
using System.Collections.Generic;

[Serializable]
public class PlayerProfile
{
    public int Coins;
    public Element StarterElement;
    public List<string> Collection = new();      // creature ids, duplicates allowed
    public List<string> Deck = new();            // max 30
    public List<string> CompletedLevels = new(); // LevelData ids
    public List<string> ShopStock = new();       // current shop offers
    public string ProgressLabel;                 // "Level 3 - Wind Wing"
    public string SavedAt;                       // "12/10/2026 14:32"
}