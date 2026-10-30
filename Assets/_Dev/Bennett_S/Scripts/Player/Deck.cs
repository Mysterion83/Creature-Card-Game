using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Testing Script for Vertical Slice, will be improved in future
/// </summary>
public class PlayerDeck : MonoBehaviour
{
    [Header("Summon Order List")]
    [SerializeField] private List<CreatureData> _creaturesToPlace = new();
    public IReadOnlyList<CreatureData> CreaturesToPlace => _creaturesToPlace;
    public bool PlacedAllCreatures => _currentCardIndex >= _creaturesToPlace.Count;
    
    private Player _player;
    private int _currentCardIndex;

    private void Awake()
    {
        _player = GetComponent<Player>();
    }

    private void Start()
    {
        SelectNextAvailableCreature();
    }

    /// <summary>
    /// Finds the next unplaced card in the list and feeds it to the player script.
    /// </summary>
    private void SelectNextAvailableCreature()
    {
        if (_currentCardIndex < _creaturesToPlace.Count)
        {
            _player.SelectCreature(_creaturesToPlace[_currentCardIndex]);
        }
    }
    
    public void MarkCurrentCardAsPlaced()
    {
        _currentCardIndex++;
        SelectNextAvailableCreature();
    }
}
