/// <summary>
/// Interface for players within the game.
/// </summary>
public interface IPlayer
{
    PlayerData Data { get; }
    bool IsDefeated { get; }
    
    void StartTurn();
    void EndTurn();
}