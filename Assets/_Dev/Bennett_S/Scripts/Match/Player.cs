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

    private void OnEnable()
    {
        PlayerRegistry.RegisterPlayer(_data.Name, this);
    }

    private void OnDisable()
    {
        PlayerRegistry.UnregisterPlayer(_data.Name);
    }

    private void Start()
    {
        _requiredSummonCount = _activeCreatures.Count;
    }

    public void StartTurn()
    {
        if (_enableTurnDebugging) Debug.Log($"Starting turn for {_data.Name}");
    }

    public void EndTurn()
    {
        if (_enableTurnDebugging) Debug.Log($"Ending turn for {_data.Name}");
    }
    
    public void AddCreature(Creature creature)
    {
        if (creature == null || _activeCreatures.Contains(creature)) return;
        _activeCreatures.Add(creature);
        if (_enableTurnDebugging) Debug.Log($"{_data.Name} placed a unit! ({_activeCreatures.Count}/{_requiredSummonCount})");

        if (_activeCreatures.Count < _requiredSummonCount) return;
        if (_enableTurnDebugging) Debug.Log($"{_data.Name} has placed all required units!");
            
        // Tell the MatchController summoning is complete
        PlayerEvents.TriggerSummonStageCompleted(this);
    }
    
    public void HandleCreatureDestroyed(Creature creature)
    {
        if (!_activeCreatures.Contains(creature)) return;
        _activeCreatures.Remove(creature);
        
        if (_enableTurnDebugging) 
            Debug.Log($"{_data.Name} lost a unit! Remaining units: {_activeCreatures.Count}");

        CheckDefeated();
    }
    
    // Call this after a creature dies, set up events for this for system integration
    private void CheckDefeated()
    {
        if (!IsDefeated) return;
        
        if (_enableTurnDebugging) Debug.Log($"Player {_data.Name} has no alive creatures left and has been defeated!");
        PlayerEvents.TriggerDefeated(this);
    }
}