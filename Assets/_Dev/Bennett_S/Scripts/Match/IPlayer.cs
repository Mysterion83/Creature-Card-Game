using UnityEngine;

public interface IPlayer
{
    PlayerData Data { get; }
    
    void StartTurn();
    void EndTurn();
}
