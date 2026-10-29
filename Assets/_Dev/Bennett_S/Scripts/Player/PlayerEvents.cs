using System;

/// <summary>
/// Static Class for handling <c>IPlayer</c> events
/// </summary>
public static class PlayerEvents
{
    public static event Action<Player> OnPlayerDefeated;
    public static event Action<Player> OnPlayerSummonStageCompleted;

    public static void TriggerDefeated(Player player)
    {
        OnPlayerDefeated?.Invoke(player);
    }

    public static void TriggerSummonStageCompleted(Player player)
    {
        OnPlayerSummonStageCompleted?.Invoke(player);
    }
}