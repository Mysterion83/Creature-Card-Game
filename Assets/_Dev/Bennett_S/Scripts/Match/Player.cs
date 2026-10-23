using System;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour, IPlayer
{
    [SerializeField] private PlayerData _data;

    [Header("Settings")]
    [SerializeField] private List<Creature> _activeCreatures;
    [SerializeField] private int _requiredSummonCount;
    
    [Header("Debug")]
    [SerializeField] private bool _enableTurnDebugging;
    
    public PlayerData Data => _data;
    public bool IsDefeated => _activeCreatures.Count == 0;
    public CreatureData SelectedCreature { get; private set; }

    private void OnEnable()
    {
        PlayerRegistry.RegisterPlayer(_data.Name, this);
    }

    private void OnDisable()
    {
        PlayerRegistry.UnregisterPlayer(_data.Name);

        foreach (var creature in _activeCreatures)
        {
            if (creature != null) creature.OnCreatureDeath -= HandleCreatureDeath;
        }
    }
    
    public void StartTurn()
    {
        if (_enableTurnDebugging) Debug.Log($"Starting turn for {_data.Name}");
    }

    public void EndTurn()
    {
        if (_enableTurnDebugging) Debug.Log($"Ending turn for {_data.Name}");
    }

    public void SelectCreature(CreatureData creatureData)
    {
        SelectedCreature = creatureData;
    }
    
    public void AddCreature(Creature creature)
    {
        if (creature == null || _activeCreatures.Contains(creature)) return;
        
        _activeCreatures.Add(creature);
        creature.OnCreatureDeath += HandleCreatureDeath;
        if (_enableTurnDebugging) Debug.Log($"{_data.Name} placed a unit! ({_activeCreatures.Count}/{_requiredSummonCount})");

        // Temp link to deck for vertical slice testing, update when works
        if (TryGetComponent(out PlayerDeck deck))
        {
            deck.MarkCurrentCardAsPlaced();
        }
        
        if (_activeCreatures.Count < _requiredSummonCount) return;
        
        // Tell the MatchController summoning is complete
        PlayerEvents.TriggerSummonStageCompleted(this);
        if (_enableTurnDebugging) Debug.Log($"{_data.Name} has placed all required units!");
    }
    
    private void HandleCreatureDeath(Creature creature)
    {
        if (!_activeCreatures.Contains(creature)) return;
        creature.OnCreatureDeath -= HandleCreatureDeath;
        _activeCreatures.Remove(creature);
        
        if (_enableTurnDebugging) 
            Debug.Log($"{_data.Name} lost a unit! Remaining units: {_activeCreatures.Count}");

        if (!IsDefeated) return;
        
        if (_enableTurnDebugging) Debug.Log($"Player {_data.Name} has no alive creatures left and has been defeated!");
        PlayerEvents.TriggerDefeated(this);
    }
}