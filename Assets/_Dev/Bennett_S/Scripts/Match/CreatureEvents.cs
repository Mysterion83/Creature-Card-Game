using System;

/// <summary>
/// Static Class for handling <c>Creature</c> events
/// </summary>
public static class CreatureEvents
{
    public static event Action<Creature> OnCreatureDestroyed;

    public static void TriggerCreatureDestroyed(Creature creature)
    {
        OnCreatureDestroyed?.Invoke(creature);
    }
}