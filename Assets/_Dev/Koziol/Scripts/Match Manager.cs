using UnityEngine;

public class MatchManager : MonoBehaviour
{
    private const float ENERGY_PER_ROUND = 10;

    public PlayerData Player1;
    public PlayerData Player2;

    private PlayerData currentPlayer => IsPlayerTurn ? Player1 : Player2;

    public bool IsPlayerTurn;

    public MatchPhase Phase;

    public void StartMatch()
    {
        IsPlayerTurn = Random.Range(0, 2) == 0;
    }

    public void StartTurn()
    {
        currentPlayer.Energy += ENERGY_PER_ROUND;
    }

    public void EndTurn()
    {
        IsPlayerTurn = !IsPlayerTurn;
    }


    public void AdvancePhase()
    {
        Phase = (MatchPhase)((int)Phase++ % 2);
        switch (Phase)
        {
            case MatchPhase.Summon:
                break;
            case MatchPhase.Gameplay:
                break;
        }
    }

    public void InitMatch(PlayerData player1Data, PlayerData player2Data)
    {
        Player1 = player1Data;
        Player2 = player2Data;
    }
}

public enum MatchPhase
{
    Summon = 0,
    Gameplay = 1,
}