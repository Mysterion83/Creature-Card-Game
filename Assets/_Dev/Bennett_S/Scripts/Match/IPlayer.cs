using UnityEngine;

public interface IPlayer
{
    string Name { get; }
    
    void StartTurn();
    void EndTurn();
}
