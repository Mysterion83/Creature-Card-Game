using System;

/// <summary>
/// Static Class for handling <c>IPlayer</c> events
/// </summary>
public static class PlayerEvents
{
    public static event Action<IPlayer> OnPlayerDefeated;
    public static event Action<IPlayer> OnPlayerSummonStageCompleted;

    public static void TriggerDefeated(IPlayer player)
    {
        OnPlayerDefeated?.Invoke(player);
    }

    public static void TriggerSummonStageCompleted(IPlayer player)
    {
        OnPlayerSummonStageCompleted?.Invoke(player);
    }
}