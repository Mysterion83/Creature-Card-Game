using UnityEngine;

public class MatchManager
{
    public bool IsPlayerTurn;



    public void EndTurn()
    {
        IsPlayerTurn = !IsPlayerTurn;
    }
}


public class PlayerData 
{
    public int Energy;
}