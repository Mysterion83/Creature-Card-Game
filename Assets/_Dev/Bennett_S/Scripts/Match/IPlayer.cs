/// <summary>
/// Interface for players within the game.
/// </summary>
public interface IPlayer
{
    PlayerData Data { get; }
    bool IsDefeated { get; }
    CreatureData SelectedCreature { get; }
    
    void StartTurn();
    void EndTurn();
    
    void AddCreature(Creature creature);
}