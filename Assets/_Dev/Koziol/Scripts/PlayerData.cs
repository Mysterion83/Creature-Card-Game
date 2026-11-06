using System;
using System.Collections.Generic;

[Serializable]
public class PlayerData
{
    public string Name;
    public bool IsAI;
    public float Energy;
    public float MaxEnergy;
    public List<Card> Hand;
    public UnityEngine.Color TeamColor;
}
