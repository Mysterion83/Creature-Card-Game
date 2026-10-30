using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StarterDeck", menuName = "Ben_P/Starter Deck")]
public class StarterDeck : ScriptableObject
{
    public Element Element;                       // piotrs enum
    public Sprite Art;                            
    public Sprite ElementIcon;                    
    public List<CreatureData> Cards = new();      // duplicates allowed
        
    public List<string> CardIds()
    {
        var ids = new List<string>();
        foreach (var card in Cards)
            if (card != null) ids.Add(card.name);
        return ids;
    }

    void OnValidate()
    {
        if (Cards.Count < 4)
            Debug.LogWarning($"{name}: starter decks need at least 4 cards for a match to start.");
    }
}