using System;

/// <summary>
/// Static Class for handling <c>Player</c> events
/// </summary>
public static class PlayerEvents
{
    public static event Action<Player> OnPlayerDefeated;
    public static void TriggerDefeated(Player player)
    {
        OnPlayerDefeated?.Invoke(player);
    }
}